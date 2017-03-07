using System.Linq;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    [XmlElement("ActBountiesComplete")]
    public class ActBountiesComplete : BaseComplexNodeTag
    {
        public ActBountiesComplete() { }
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
            var b = ZetaDia.ActInfo.Bounties.Where(bounty => bounty.Act.ToString().Equals(Act) && bounty.Info.State == QuestState.Completed);
            if (b.FirstOrDefault() != null) Logger.Log("Bounties Complete count:" + b.Count());
            else Logger.Log("Bounties complete returned null.");

            foreach (var c in ZetaDia.ActInfo.Bounties.Where(bounty => bounty.Act.ToString().Equals(Act) && bounty.Info.State != QuestState.Completed))
            {
                Logger.Log("Bounty " + c.Info.Quest.ToString() + " (" + c.Info.QuestSNO + ") unsupported or invalid.");
            }

            return b.FirstOrDefault() != null && b.Count() == 5;
        }

        [XmlAttribute("act", true)]
        public string Act { get; set; }
    }
}
