using System.Linq;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace QuestTools.ProfileTags
{
    [XmlElement("HaveBounty")]
    public class HaveBountyTag : BaseComplexNodeTag
    {
        public HaveBountyTag() { }
        protected override Composite CreateBehavior()
        {
            return
            new Decorator(ret => !IsDone,
                new PrioritySelector(
                    GetNodes().Select(b => b.Behavior).ToArray()
                )
            );
        }

        public override bool GetConditionExec()
        {
            return ZetaDia.ActInfo.Bounties.Where(bounty => bounty.Info.QuestSNO == QuestId && bounty.Info.State != QuestState.Completed).FirstOrDefault() != null;
        }

        private bool CheckNotAlreadyDone(object obj)
        {
            return !IsDone;
        }
    }
}
