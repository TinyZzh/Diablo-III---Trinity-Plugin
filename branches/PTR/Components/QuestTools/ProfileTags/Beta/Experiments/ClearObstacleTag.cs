//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Zeta.Bot;
//using Zeta.Bot.Navigation;
//using Zeta.Bot.Profile;
//using Zeta.Common;
//using Zeta.Game;
//using Zeta.Game.Internals.Actors;
//using Zeta.Game.Internals.Actors.Gizmos;
//using Zeta.TreeSharp;
//using Zeta.XmlEngine;
//using Action = Zeta.TreeSharp.Action;

//namespace QuestTools.ProfileTags.Complex
//{
//    /// <summary>
//    /// Run a circle around the current location then return to starting point
//    /// </summary>
//    [XmlElement("ClearObstacle")]
//    public class ClearObstacleTag : ProfileBehavior, IEnhancedProfileBehavior
//    {
//        private bool _isDone;

//        private List<Vector3> _points;
//        private DefaultNavigationProvider _navigator;

//        [XmlAttribute("actorId")]
//        public int Radius { get; set; }

//        public override bool IsDone
//        {
//            get { return !IsActiveQuestStep || _isDone; }
//        }

//        public override void OnStart()
//        {
//            Radius = Radius < 10 ? 10 : Radius;
//            _points = GetObstacleLocations(Radius, ZetaDia.Me.Position);
//            _points.Add(ZetaDia.Me.Position);
//            _navigator = Navigator.GetNavigationProviderAs<DefaultNavigationProvider>();

//            base.OnStart();
//        }

//        protected override Composite CreateBehavior()
//        {
//            return new Decorator(ret => !_isDone, new Sequence(

//                new Decorator(ret => _points.Any(), CommonBehaviors.MoveTo(ret => _points.First())),

//                new Decorator(ret => !IsReachable, new Action(ret => _points.RemoveAt(0))),

//                new Decorator(ret => !_points.Any(), new Action(ret => _isDone = true))

//               )
//            );
//        }

//        private bool IsReachable
//        {
//            get
//            {
//                if (!_points.Any() || _points.First().Distance2D(ZetaDia.Me.Position) < 10f)
//                    return false;

//                if (!_navigator.CanPathWithinDistance(_points.First(), 10) || Navigator.StuckHandler.IsStuck)
//                    return false;

//                return true;
//            }
//        }

//        private List<Vector3> GetObstacleLocations(double radius, Vector3 center, int actorId = 0)
//        {
//            return (List<Vector3>) ZetaDia.Actors.GetActorsOfType<DiaObject>()
//                .Where(o => o is GizmoDestructible && o.Distance <= radius && (actorId == 0 || o.ActorSnoId == actorId ))
//                .OrderBy(o => o.Distance)
//                .Select(o => o.Position);
//        }

//        public override void ResetCachedDone()
//        {
//            _isDone = false;
//            base.ResetCachedDone();
//        }

//        #region IEnhancedProfileBehavior

//        public void Update()
//        {
//            UpdateBehavior();
//        }

//        public void Start()
//        {
//            OnStart();
//        }

//        public void Done()
//        {
//            _isDone = true;
//        }

//        #endregion
//    }
//}