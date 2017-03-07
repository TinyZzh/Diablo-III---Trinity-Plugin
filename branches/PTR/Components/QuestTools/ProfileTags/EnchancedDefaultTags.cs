using QuestTools.Helpers;
using System.Collections.Generic;
using System.Linq;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Bot.Profile.Composites;
using Zeta.Common;
using Zeta.Game;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;


namespace QuestTools.ProfileTags.Complex
{
    public class ActionTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        public Helpers.Common.BoolDelegate Action;
        public Helpers.Common.BoolDelegate IsDoneWhen;

        /// <summary>
        /// Action is a method with return type bool, false will loop, true will end.
        /// </summary>
        public ActionTag(Helpers.Common.BoolDelegate action = null)
        {
            if(action != null) Action = action;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get
            {
                var delegateIsDone = IsDoneWhen != null && IsDoneWhen.Invoke(null);

                if(delegateIsDone)
                    Logger.Verbose("IsDoneWhen returned true, setting done");

                return _isDone || delegateIsDone;
            }
        }

        protected override Composite CreateBehavior()
        {
            return new Action(ret =>
            {
                if(IsDone)
                    return RunStatus.Success;

                if (Action != null)
                {
                    if (Action.Invoke(null))
                    {
                        _isDone = true;
                        return RunStatus.Success;
                    }
                    return RunStatus.Running;                    
                }
                return RunStatus.Success;
            });
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

    public class CompositeTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        public Helpers.Common.BoolDelegate IsDoneWhen;

        /// <summary>
        /// new CompositeTag(ret => MyComposite);
        /// </summary>
        public CompositeTag(Helpers.Common.CompositeDelegate compositeDelegate = null)
        {
            if (compositeDelegate != null) //Composite = compositeDelegate.Invoke(null);
                CompositeDelegate = compositeDelegate;
        }

        /// <summary>
        /// If set, IsDone when this status is returned;
        /// </summary>
        public RunStatus? DoneStatus;

        private bool _isDone;
        public override bool IsDone
        {
            get
            {
                var delegateIsDone = IsDoneWhen != null && IsDoneWhen.Invoke(null);

                if (delegateIsDone)
                    Logger.Verbose("IsDoneWhen returned true, setting done");

                bool doneByStatus = false;
                if (DoneStatus.HasValue)
                {
                    doneByStatus = Composite.LastStatus == DoneStatus;                            
                    if(doneByStatus)
                        Logger.Verbose("Status {0} was returned, setting done", Composite.LastStatus); 
                }

                return _isDone || delegateIsDone || doneByStatus;
            }
        }

        public Helpers.Common.CompositeDelegate CompositeDelegate;

        private Composite _composite;
        public Composite Composite
        {
            get
            {
                return CompositeDelegate != null ? CompositeDelegate.Invoke(null) : _composite;
            }
            set { _composite = value; }
        }

        protected override Composite CreateBehavior()
        {
            return Composite;
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

    public class EnhancedLeaveGameTag : LeaveGameTag, IEnhancedProfileBehavior
    {
        public EnhancedLeaveGameTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || _isDone || base.IsDone; }
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

    public class EnhancedLoadProfileTag : LoadProfileTag, IEnhancedProfileBehavior
    {
        public EnhancedLoadProfileTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || _isDone || base.IsDone; }
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

    public class EnhancedLogMessageTag : LogMessageTag, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone || base.IsDone; }
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

    public class EnhancedUseWaypointTag : UseWaypointTag, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone || base.IsDone; }
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

    public class EnhancedWaitTimerTag : WaitTimerTag, IEnhancedProfileBehavior
    {
        public EnhancedWaitTimerTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || _isDone || base.IsDone; }
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

    public class EnhancedUseObjectTag : UseObjectTag, IEnhancedProfileBehavior
    {
        public EnhancedUseObjectTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone || base.IsDone; }
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

    public class EnhancedUsePowerTag : UsePowerTag, IEnhancedProfileBehavior
    {
        public EnhancedUsePowerTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone || base.IsDone; }
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

    public class EnhancedToggleTargetingTag : ToggleTargetingTag, IEnhancedProfileBehavior
    {
        public EnhancedToggleTargetingTag()
        {
            QuestId = QuestId <= 0 ? 1 : QuestId;
        }

        private bool _isDone;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || _isDone || base.IsDone; }
        }

        public override void OnStart() { }

        protected override Composite CreateBehavior()
        {
            _isDone = true;
            return Helpers.Common.ExecuteReturnAlwaysSuccess(
                ret => !_isDone,
                ret => new Action(r => base.OnStart())
            );
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

    public class EnhancedWaitWhileTag : WaitWhileTag, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone || base.IsDone; }
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

    public class EnhancedIfTag : IfTag, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public Helpers.Common.BoolDelegate IsDoneDelegate;

        public EnhancedIfTag(Helpers.Common.BoolDelegate isDoneDelegate = null, params ProfileBehavior[] children)
        {
            IsDoneDelegate = isDoneDelegate;
            if(children!=null && children.Any())
                Body = children.ToList();
        }

        public override bool IsDone
        {
            get
            {
                if (IsDoneDelegate != null)
                    Conditional = ScriptManager.GetCondition(IsDoneDelegate.Invoke(null) ? "True" : "False");                

                return _isDone || base.IsDone;
            }
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
        }

        #region IEnhancedProfileBehavior : INodeContainer

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
            this.SetChildrenDone();
        }

        #endregion

        public List<ProfileBehavior> Children
        {
            get { return Body; }
            set { Body = value; }
        }

    }

    public class EnhancedWhileTag : WhileTag, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public Helpers.Common.BoolDelegate IsDoneDelegate;

        public EnhancedWhileTag(Helpers.Common.BoolDelegate isDoneDelegate = null, params ProfileBehavior[] children)
        {
            IsDoneDelegate = isDoneDelegate;
            if(children!=null && children.Any())
                Body = children.ToList();
        }

        public override bool IsDone
        {
            get
            {                
                if (IsDoneDelegate != null)
                    Conditional = ScriptManager.GetCondition(IsDoneDelegate.Invoke(null) ? "True" : "False");                     

                return _isDone || base.IsDone;
            }
        }

        public new bool GetConditionExec()
        {
            return IsDoneDelegate != null && IsDoneDelegate.Invoke(null) || ScriptManager.GetCondition(Condition).Invoke();
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
        }

        #region IEnhancedProfileBehavior : INodeContainer

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
            this.SetChildrenDone();
        }

        #endregion

        public List<ProfileBehavior> Children
        {
            get { return Body; }
            set { Body = value; }
        }

    }

}

