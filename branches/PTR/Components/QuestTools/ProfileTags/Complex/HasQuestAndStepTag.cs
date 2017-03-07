using System.Linq;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    [XmlElement("HasQuestAndStep")]
    public class HasQuestAndStep : BaseComplexNodeTag
    {
        public HasQuestAndStep() { }
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
            return ZetaDia.ActInfo.AllQuests
                .Where(quest => quest.QuestSNO == QuestId && quest.State != QuestState.Completed && quest.QuestStep == StepId).FirstOrDefault() != null;
        }

        private bool CheckNotAlreadyDone(object obj)
        {
            return !IsDone;
        }
    }
}
