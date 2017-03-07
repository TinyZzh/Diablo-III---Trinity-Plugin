using Zeta.Bot.Profile;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Depreciated
{
    // * TrinityMoveTo moves in a straight line without any navigation hits, and allows tag-skips
    [XmlElement("TrinityMoveToSNO")]
    public class TrinityMoveToSNOTag : ProfileBehavior
    {
        public TrinityMoveToSNOTag() { }

        public override void OnStart()
        {
            Logger.Error("TrinityMoveToSNO is depreciated. Use MoveToActor instead.");
            _isDone = true;
            base.OnStart();
        }

        private bool _isDone;

        public override bool IsDone
        {
            get { return _isDone; }
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
        }
    }

}
