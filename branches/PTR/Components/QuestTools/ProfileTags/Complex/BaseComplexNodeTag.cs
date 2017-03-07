using System;
using System.Collections.Generic;
using System.Linq;
using QuestTools.ProfileTags.Complex;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Composites;

namespace QuestTools.ProfileTags
{
    public abstract class BaseComplexNodeTag : ComplexNodeTag, IEnhancedProfileBehavior
    {
        private bool? _ComplexDoneCheck;
        private bool? _AlreadyCompleted;
        private static Func<ProfileBehavior, bool> _BehaviorProcess;

        protected bool? ComplexDoneCheck
        {
            get
            {
                return _ComplexDoneCheck;
            }
            set
            {
                _ComplexDoneCheck = value;
            }
        }

        private readonly HashSet<Guid> _seenGuids = new HashSet<Guid>();

        public override bool IsDone
        {
            get
            {
                if (_AlreadyCompleted.GetValueOrDefault(false))
                    return true;

                var b = ProfileManager.CurrentProfileBehavior;
                if (Body.Contains(b) && !_seenGuids.Contains(b.Behavior.Guid))
                {
                    OnChildStart();
                    _seenGuids.Add(b.Behavior.Guid);
                }

                if (!ComplexDoneCheck.HasValue)
                {
                    ComplexDoneCheck = new bool?(GetConditionExec());
                }
                if (ComplexDoneCheck == false)
                {
                    return true;
                }

                if (_BehaviorProcess == null)
                {
                    _BehaviorProcess = new Func<ProfileBehavior, bool>(p => p.IsDone);
                }
                bool allChildrenDone = Body.All<ProfileBehavior>(_BehaviorProcess);
                if (allChildrenDone)
                {
                    OnChildrenDone();
                    _AlreadyCompleted = true;
                }
                return allChildrenDone;
            }
        }

        public abstract bool GetConditionExec();

        public virtual void OnChildStart() { }

        public virtual void OnChildrenDone() { }

        public override void ResetCachedDone()
        {
            foreach (ProfileBehavior behavior in Body)
            {
                behavior.ResetCachedDone();
            }
            ComplexDoneCheck = null;
            _AlreadyCompleted = null;
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
            ComplexDoneCheck = false;
            _AlreadyCompleted = true;
        }

        #endregion
    }
}
