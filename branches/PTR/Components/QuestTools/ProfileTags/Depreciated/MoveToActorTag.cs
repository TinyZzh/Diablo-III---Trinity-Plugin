using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using QuestTools.Helpers;
using QuestTools.Navigation;
using QuestTools.ProfileTags.Complex;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Movement
{
    [XmlElement("MoveToActor")]
    public class MoveToActorTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        public MoveToActorTag() { }

        private bool _isDone;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || _isDone; }
        }

        [XmlAttribute("x")]
        public float X { get; set; }

        [XmlAttribute("y")]
        public float Y { get; set; }

        [XmlAttribute("z")]
        public float Z { get; set; }

        public Vector3 Position
        {
            get { return new Vector3(X, Y, Z); }
            set { X = value.X; Y = value.Y; Z = value.Z; }
        }

        /// <summary>
        /// Defines how close we need to be to the actor in order to interact with it. Default=10
        /// </summary>
        [XmlAttribute("interactRange")]
        public int InteractRange { get; set; }

        [XmlAttribute("straightLinePathing")]
        public bool StraightLinePathing { get; set; }

        [XmlAttribute("useNavigator")]
        public bool UseNavigator { get; set; }

        /// <summary>
        /// The ActorSnoId of the object you're looking for - optional
        /// </summary>
        [XmlAttribute("actorId")]
        public int ActorId { get; set; }

        /// <summary>
        /// The number of interact attempts before giving up. Default=5
        /// </summary>
        [XmlAttribute("interactAttempts")]
        public int InteractAttempts { get; set; }

        /// <summary>
        /// The "safe" distance that we will request a dynamic nav point to. We will never actually reach this nav point as it's always going to be <see cref="PathPointLimit"/> away.
        /// If the target is closer than this distance, we will just move to the target.
        /// </summary>
        [XmlAttribute("pathPointLimit")]
        public int PathPointLimit { get; set; }

        /// <summary>
        /// Boolean defining special portal handling
        /// </summary>
        [XmlAttribute("isPortal")]
        public bool IsPortal { get; set; }

        /// <summary>
        /// Required if using IsPortal
        /// </summary>
        [XmlAttribute("destinationWorldId")]
        public int DestinationWorldId { get; set; }

        /// <summary>
        /// When searching for An ActorID at Position, what's the maximum distance from Position that will result in a valid Actor?
        /// </summary>
        [XmlAttribute("maxSearchDistance")]
        public int MaxSearchDistance { get; set; }

        /// <summary>
        /// This is the longest time this behavior can run for. Default is 180 seconds (3 minutes).
        /// </summary>
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// If the given actor has an animation that is matching this, the behavior will end
        /// </summary>
        [XmlAttribute("endAnimation")]
        public string EndAnimation { get; set; }

        /// <summary>
        /// Finishes the tag when the Interactive Conversation button appears
        /// </summary>
        [XmlAttribute("exitWithConversation")]
        public bool ExitWithConversation { get; set; }

        [XmlAttribute("exitWithVendorWindow")]
        public bool ExitWithVendorWindow { get; set; }

        // Special configuration if you want to tweak things:
        private bool _verbose;
        private int _interactWaitMilliSeconds = 250;

        private int _completedInteractions;
        private int _startingWorldId;
        private DateTime _lastInteract = DateTime.MinValue;
        private DateTime _lastPositionUpdate = DateTime.UtcNow;
        private DateTime _tagStartTime = DateTime.MinValue;
        private Vector3 _startInteractPosition = Vector3.Zero;
        private Vector3 _lastPosition = Vector3.Zero;
        private readonly QTNavigator _qtNavigator = new QTNavigator();
        private MoveResult _moveResult = MoveResult.Moved;
        private SNOAnim _endAnimation = SNOAnim.Invalid;

        public override void OnStart()
        {
            if (!ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld || !ZetaDia.Me.IsValid)
                return;

            if (InteractRange == 0)
                InteractRange = 10;
            if (InteractAttempts == 0)
                InteractAttempts = 5;
            if (PathPointLimit == 0)
                PathPointLimit = 250;
            if (Timeout == 0)
                Timeout = 180;


            _startingWorldId = ZetaDia.Globals.WorldSnoId;
            _tagStartTime = DateTime.UtcNow;

            if (!String.IsNullOrEmpty(EndAnimation))
            {
                try
                {
                    Enum.TryParse(EndAnimation, out _endAnimation);
                }
                catch
                {
                    _endAnimation = SNOAnim.Invalid;
                }
            }

            Navigator.Clear();

            _verbose = true;

            _completedInteractions = 0;
            _startingWorldId = 0;
            _lastInteract = DateTime.MinValue;
            _startInteractPosition = Vector3.Zero;
            _lastPosition = ZetaDia.Me.Position;
            _lastPositionUpdate = DateTime.UtcNow;

            Logger.Debug("Initialized {0}", Status());
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ctx => MainCoroutine());
        }


        private async Task<bool> MainCoroutine()
        {
            if (ZetaDia.Me == null)
                return false;

            if (!ZetaDia.IsInGame)
                return false;

            if (ZetaDia.Globals.IsLoadingWorld)
            {
                await Coroutine.Sleep(50);
                return false;
            }
            if (!ZetaDia.Me.IsValid)
                return false;

            if (ZetaDia.Me.IsDead)
                return false;

            if (DateTime.UtcNow.Subtract(_lastInteract).TotalMilliseconds < 500 && WorldHasChanged())
                return true;

            if (Timeout > 0 && DateTime.UtcNow.Subtract(_tagStartTime).TotalSeconds > Timeout)
            {
                End("Timeout of {0} seconds exceeded for Profile Behavior {1}", Timeout, Status());
                return true;
            }

            if (TargetIsDungeonStone && GameUI.IsElementVisible(GameUI.GenericOK))
            {
                GameUI.SafeClickElement(GameUI.GenericOK, "Generic OK");
                await Coroutine.Yield();
                await Coroutine.Sleep(3000);
            }

            GameUI.SafeClickUIButtons();

            if (Vector3.Distance(_lastPosition, ZetaDia.Me.Position) > 5f)
            {
                _lastPositionUpdate = DateTime.UtcNow;
                _lastPosition = ZetaDia.Me.Position;
            }

            if (ExitWithVendorWindow && GameUI.IsElementVisible(UIElements.VendorWindow))
            {
                EndDebug("Vendor window is visible " + Status());
            }

            if (Actor == null && Position == Vector3.Zero && !WorldHasChanged())
            {
                var lastSeenPosition = ActorHistory.GetActorPosition(ActorId);
                if (lastSeenPosition != Vector3.Zero)
                {
                    Warn("Can't find actor! using last known position {0} Distance={1}",
                        lastSeenPosition.ToString(),
                        lastSeenPosition.Distance(ZetaDia.Me.Position));

                    Position = lastSeenPosition;
                    return true;
                }

                EndDebug("ERROR: Could not find an actor or position to move to, finished! {0}", Status());
                return true;
            }
            if (IsPortal && WorldHasChanged())
            {
                if (DestinationWorldId > 0 && ZetaDia.Globals.WorldSnoId != DestinationWorldId && ZetaDia.Globals.WorldSnoId != _startingWorldId)
                {
                    EndDebug("Error! We used a portal intending to go from WorldId={0} to WorldId={1} but ended up in WorldId={2} {3}",
                                                    _startingWorldId, DestinationWorldId, ZetaDia.Globals.WorldSnoId, Status());
                    return true;
                }
                await Coroutine.Sleep(100);
                EndDebug("Successfully used portal {0} to WorldId {1} {2}", ActorId, ZetaDia.Globals.WorldSnoId, Status());
                return true;
            }
            if (Actor == null && ((MaxSearchDistance > 0 && WithinMaxSearchDistance()) || WithinInteractRange()))
            {
                EndDebug("Finished: Actor {0} not found, within InteractRange {1} and  MaxSearchDistance {2} of Position {3} {4}",
                    ActorId, InteractRange, MaxSearchDistance, Position, Status());

                return true;
            }
            if (Position.Distance(ZetaDia.Me.Position) > 1500)
            {
                EndDebug("ERROR: Position distance is {0} - this is too far! {1}", Position.Distance(ZetaDia.Me.Position), Status());
                return true;
            }

            if (_moveResult == MoveResult.ReachedDestination && Actor == null)
            {
                EndDebug("Reached Destination, no actor found! " + Status());
                return true;
            }

            if (Actor == null)
            {
                if (MaxSearchDistance > 0 && !WithinMaxSearchDistance())
                {
                    Move(Position);
                    return true;
                }
                if (InteractRange > 0 && !WithinInteractRange())
                {
                    Move(Position);
                    return true;
                }
            }

            if (Actor == null || !Actor.IsValid)
                return true;

            if (((!IsPortal && _completedInteractions >= InteractAttempts && InteractAttempts > 0) || (IsPortal && WorldHasChanged()) || AnimationMatch()))
            {
                EndDebug("Successfully interacted with Actor {0} at Position {1} " + Status(), Actor.ActorSnoId, Actor.Position);
                return true;
            }
            if (InteractAttempts <= 0 && WithinInteractRange())
            {
                EndDebug("Actor is within interact range {0:0} - no interact attempts " + Status(), Actor.Distance);
                return true;
            }
            if (_completedInteractions >= InteractAttempts)
            {
                EndDebug("Interaction failed after {0} interact attempts " + Status(), _completedInteractions);
                return true;
            }
            if (ExitWithConversation && GameUI.IsElementVisible(GameUI.TalktoInteractButton1))
            {
                GameUI.SafeClickElement(GameUI.TalktoInteractButton1, "Conversation Interaction Button 1");
                EndDebug("Clicked Conversation Interaction Button 1 " + Status());
                return true;
            }
            if (!WithinInteractRange())
            {
                Move(Actor.Position);
                return true;
            }

            await Coroutine.Wait(_interactWaitMilliSeconds, ShouldWaitForInteraction);
            if ((WithinInteractRange() || DateTime.UtcNow.Subtract(_lastPositionUpdate).TotalMilliseconds > 750) && _completedInteractions < InteractAttempts)
            {
                return await InteractRoutine();
            }

            Logger.Debug("No action taken");
            return true;
        }

        private bool TargetIsDungeonStone
        {
            get
            {
                return Actor != null && Actor is DiaGizmo && Actor.ActorInfo != null && Actor.ActorInfo.GizmoType == GizmoType.ReturnPortal;
            }
        }

        /// <summary>
        /// Gets the Actor by ActorId and other parameters
        /// </summary>
        private DiaObject Actor
        {
            get
            {
                List<DiaObject> actorList;
                if (DataDictionary.GuardedBountyGizmoIds.Contains(ActorId))
                {
                    try
                    {
                        actorList = ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                            .Where(i => i.IsValid && i.ActorSnoId == ActorId && i.CommonData != null && i is DiaGizmo && !((DiaGizmo)i).HasBeenOperated)
                            .ToList();
                    }
                    catch (Exception)
                    {
                        actorList = new List<DiaObject>();
                    }
                }
                else
                {
                    actorList = ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                     .Where(i => i.IsValid && i.ActorSnoId == ActorId)
                     .ToList();
                }

                if (!actorList.Any())
                    return null;

                try
                {
                    DiaObject actor;
                    // Find closest actor if we have a position and MaxSearchDistance (only actors within radius MaxSearchDistance from Position)
                    if (Position != Vector3.Zero && MaxSearchDistance > 0)
                    {
                        actor = actorList
                            .Where(o => Position.Distance(Position) <= MaxSearchDistance)
                            .OrderByDescending(o => (o as DiaUnit) == null || !(o.CommonData != null && (o as DiaUnit).IsQuestGiver))
                            .ThenBy(o => Position.Distance2DSqr(o.Position)).FirstOrDefault();
                    }
                    // Otherwise just OrderBy distance from Position (any actor found)
                    else if (Position != Vector3.Zero)
                    {
                        actor = actorList
                            .OrderByDescending(o => (o as DiaUnit) == null || !(o.CommonData != null && (o as DiaUnit).IsQuestGiver))
                            .ThenBy(o => Position.Distance2DSqr(o.Position)).FirstOrDefault();
                    }
                    // If all else fails, get first matching Actor closest to Player
                    else
                    {
                        actor = actorList
                            .OrderByDescending(o => (o as DiaUnit) == null || !(o.CommonData != null && (o as DiaUnit).IsQuestGiver))
                            .ThenBy(o => o.Distance).FirstOrDefault();
                    }
                    if (actor == null)
                        return null;

                    Position = actor.Position;

                    if (actor.CommonData != null && actor is GizmoLootContainer && actor.CommonData.GetAttribute<int>(ActorAttributeType.ChestOpen) != 0)
                    {
                        return null;
                    }

                    if (actor.CommonData != null && actor is DiaUnit && (actor as DiaUnit).IsDead)
                    {
                        return null;
                    }

                    return actor;
                }
                catch (Exception ex)
                {
                    return actorList.FirstOrDefault();
                }
            }
        }

        private bool ShouldWaitForInteraction()
        {
            return ZetaDia.Me.LoopingAnimationEndTime > 0 || Math.Abs(DateTime.UtcNow.Subtract(_lastInteract).TotalSeconds) > _interactWaitMilliSeconds;
        }

        private async Task<bool> InteractRoutine()
        {
            if (Player.IsValid && Actor != null && Actor.IsValid)
            {

                if (TargetIsDungeonStone && (GameUI.IsElementVisible(GameUI.GenericOK) || GameUI.IsElementVisible(UIElements.ConfirmationDialogOkButton)))
                {
                    GameUI.SafeClickElement(GameUI.GenericOK, "Generic OK");
                    await Coroutine.Yield();

                    GameUI.SafeClickElement(UIElements.ConfirmationDialogOkButton);
                    await Coroutine.Yield();

                    await Coroutine.Sleep(3000);
                    return true;
                }


                if (_startingWorldId <= 0)
                {
                    _startingWorldId = ZetaDia.Globals.WorldSnoId;
                }

                _interactWaitMilliSeconds = TargetIsDungeonStone ? 4000 : 250;
                LogInteraction();

                if (IsPortal)
                {
                    //GameEvents.FireWorldTransferStart();
                }

                if (IsChanneling)
                    await Coroutine.Wait(TimeSpan.FromSeconds(3), () => IsChanneling);

                switch (Actor.ActorType)
                {
                    case ActorType.Gizmo:
                        switch (Actor.ActorInfo.GizmoType)
                        {
                            case GizmoType.BossPortal:
                            case GizmoType.Portal:
                            case GizmoType.ReturnPortal:
                                ZetaDia.Me.UsePower(SNOPower.GizmoOperatePortalWithAnimation, Actor.Position);
                                break;
                            default:
                                ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Actor.Position);
                                break;
                        }
                        break;
                    case ActorType.Monster:
                        ZetaDia.Me.UsePower(SNOPower.Axe_Operate_NPC, Actor.Position);
                        break;
                }

                // Doubly-make sure we interact
                Actor.Interact();

                GameUI.SafeClickElement(GameUI.GenericOK, "Generic OK");
                GameUI.SafeClickElement(UIElements.ConfirmationDialogOkButton, "Confirmation Dialog OK Button");

                if (_startInteractPosition == Vector3.Zero)
                    _startInteractPosition = ZetaDia.Me.Position;

                _lastPosition = ZetaDia.Me.Position;
                _completedInteractions++;
                _lastInteract = DateTime.UtcNow;
                if (TargetIsDungeonStone || IsPortal)
                    await Coroutine.Sleep(250);

                return true;
            }
            return false;
        }


        private void LogInteraction()
        {
            Logger.Debug("Interacting with Object: {0} {1} attempt: {2}, lastInteractDuration: {3:0}",
                Actor.ActorSnoId, Status(), _completedInteractions, Math.Abs(DateTime.UtcNow.Subtract(_lastInteract).TotalSeconds));
        }

        private bool WithinMaxSearchDistance()
        {
            return ZetaDia.Me.Position.Distance(Position) < MaxSearchDistance;
        }

        private string _interactReason = "";
        private bool WithinInteractRange()
        {
            if (!ZetaDia.IsInGame)
                return false;
            if (ZetaDia.Globals.IsLoadingWorld)
                return false;
            if (ZetaDia.Me == null)
                return false;
            if (!ZetaDia.Me.IsValid)
                return false;
            if (ZetaDia.Me.HitpointsCurrent <= 0)
                return false;

            if (Actor != null && Actor.IsValid)
            {
                float distance = ZetaDia.Me.Position.Distance2D(Actor.Position);
                float radiusDistance = Actor.Distance - Actor.CollisionSphere.Radius;
                Vector3 radiusPoint = MathEx.CalculatePointFrom(Actor.Position, ZetaDia.Me.Position, Actor.CollisionSphere.Radius);
                if (_moveResult == MoveResult.ReachedDestination)
                {
                    _interactReason = "ReachedDestination";
                    return true;
                }
                if (distance < 7.5f)
                {
                    _interactReason = "Distance < 7.5f";
                    return true;
                }
                if (distance < InteractRange && Actor.InLineOfSight && !Navigator.Raycast(ZetaDia.Me.Position, radiusPoint))
                {
                    _interactReason = "InLoSRaycast";
                    return true;
                }
                if (radiusDistance < 5f)
                {
                    _interactReason = "Radius < 2.5f";
                    return true;
                }
                return false;
            }
            _interactReason = "DefaultInteractRange";
            return ZetaDia.Me.Position.Distance(Position) < InteractRange;
        }

        private bool IsChanneling
        {
            get
            {
                if (!ZetaDia.Me.IsValid)
                    return false;
                if (!ZetaDia.Me.CommonData.IsValid)
                    return false;
                if (ZetaDia.Me.CommonData.AnimationState == AnimationState.Channeling)
                    return true;
                if (ZetaDia.Me.CommonData.AnimationState == AnimationState.Casting)
                    return true;
                if (ZetaDia.Me.LoopingAnimationEndTime > 0)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Checks to see if animation on actor matches EndAnimation
        /// </summary>
        /// <returns></returns>
        private bool AnimationMatch()
        {
            try
            {
                bool match = _endAnimation != SNOAnim.Invalid && Actor.CommonData.CurrentAnimation == _endAnimation;

                return match;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check to see if isPortal is true and destinationWorldId is defined and we've changed worlds
        /// </summary>
        /// <returns>True if we're in the new, desired world</returns>
        private bool WorldHasChanged()
        {
            try
            {
                if (IsPortal && DestinationWorldId > 0)
                {
                    // DestinationWorld Id matches
                    return _completedInteractions > 0 && ZetaDia.Globals.WorldSnoId == DestinationWorldId && ZetaDia.Me.Position.Distance(_startInteractPosition) > InteractRange;
                }
                if (IsPortal && (DestinationWorldId > 0 || DestinationWorldId == -1) && _startingWorldId != 0)
                {
                    // WorldId Changed
                    return _completedInteractions > 0 && ZetaDia.Globals.WorldSnoId != _startingWorldId;
                }
                if (IsPortal && _startInteractPosition != Vector3.Zero)
                {
                    // Player moved from the original interaction position (same world portals and such)
                    return _completedInteractions > 0 && ZetaDia.Me.Position.Distance(_startInteractPosition) > InteractRange;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        private void Move(Vector3 navTarget)
        {
            if (_lastPosition != Vector3.Zero && _lastPosition.Distance(ZetaDia.Me.Position) >= 1)
            {
                _lastPosition = ZetaDia.Me.Position;
            }
            // DB 300+ always uses local nav! Yay :)
            if (navTarget.Distance(ZetaDia.Me.Position) > PathPointLimit)
                navTarget = MathEx.CalculatePointFrom(ZetaDia.Me.Position, navTarget, navTarget.Distance(ZetaDia.Me.Position) - PathPointLimit);

            if (StraightLinePathing)
            {
                Navigator.PlayerMover.MoveTowards(Position);
                _moveResult = MoveResult.Moved;
            }
            else
            {
                _moveResult = _qtNavigator.MoveTo(navTarget, Status());
            }
            switch (_moveResult)
            {
                case MoveResult.Moved:
                case MoveResult.ReachedDestination:
                case MoveResult.PathGenerated:
                case MoveResult.PathGenerating:
                    break;
                case MoveResult.PathGenerationFailed:
                case MoveResult.UnstuckAttempt:
                case MoveResult.Failed:
                    break;
            }
            _lastPosition = ZetaDia.Me.Position;

            if (QuestTools.EnableDebugLogging)
            {
                Logger.Debug("MoveResult: {0} {1}", _moveResult.ToString(), Status());
            }
        }
        private String Status()
        {
            String status = "";
            try
            {
                if (!QuestToolsSettings.Instance.DebugEnabled)
                    return status;

                if (_verbose)
                {
                    status = String.Format(
                        "questId=\"{0}\" stepId=\"{1}\" actorId=\"{10}\" x=\"{2:0}\" y=\"{3:0}\" z=\"{4:0}\" interactRange=\"{5}\" interactAttempts={11} distance=\"{6:0}\" maxSearchDistance={7} rayCastDistance={8} lastPosition={9}, isPortal={12} destinationWorldId={13}, startInteractPosition={14} completedInteractAttempts={15} interactReason={16}",
                        ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId, X, Y, Z, InteractRange,
                        (Actor != null ? Actor.Distance : Position.Distance(ZetaDia.Me.Position)),
                        MaxSearchDistance, PathPointLimit, _lastPosition, ActorId, InteractAttempts, IsPortal, DestinationWorldId, _startInteractPosition, _completedInteractions, _interactReason);
                }
                else
                {
                    status = String.Format("questId=\"{0}\" stepId=\"{1}\" x=\"{2:0}\" y=\"{3:0}\" z=\"{4:0}\" interactRange=\"{5}\" interactAttempts={10} maxSearchDistance={6} rayCastDistance={7} lastPosition={8}, actorId=\"{9}\" isPortal={10} destinationWorldId={11}",
                        ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId, X, Y, Z, InteractRange, PathPointLimit, _lastPosition, ActorId, InteractAttempts, IsPortal, DestinationWorldId);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in MoveToActor Status(): " + ex);
            }
            try
            {
                if (Actor != null && Actor.IsValid && Actor.CommonData != null)
                {
                    status += String.Format(" actorId=\"{0}\", Name={1} InLineOfSight={2} ActorType={3} Position= {4}",
                        Actor.ActorSnoId, Actor.Name, Actor.InLineOfSight, Actor.ActorType, StringUtils.GetProfileCoordinates(Actor.Position));
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in MoveToActor Status(): " + ex);
            }
            return status;

        }

        private void EndDebug(string message, params object[] args)
        {
            Logger.Debug(message, args);
            _isDone = true;

        }
        private void EndDebug(string message)
        {
            EndDebug(message, 0);
        }
        private void End(string message, params object[] args)
        {
            Logger.Log(message, args);
            _isDone = true;
        }
        private void End(string message)
        {
            End(message, 0);
        }

        private void Warn(string message, params object[] args)
        {
            if (QuestTools.EnableDebugLogging)
                Logger.Warn(message, args);
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            _tagStartTime = DateTime.UtcNow;
            _completedInteractions = 0;
            _startingWorldId = 0;
            base.ResetCachedDone();
        }

        #region IEnhancedProfileBehavior

        public void Update()
        {
            UpdateBehavior();
        }

        public void Start()
        {
            OnStart();
        }

        public void Done()
        {
            _isDone = true;
        }

        #endregion
    }
}
