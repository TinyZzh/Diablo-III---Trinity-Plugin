using QuestTools.Helpers;
using Zeta.Common;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Complex
{
    [XmlElement("When")]
    public class WhenTag : BaseComplexNodeTag
    {
        [XmlAttribute("condition")]
        public string Condition { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("persist")]
        public bool Persist { get; set; }

        [XmlAttribute("repeat")]
        public bool Repeat { get; set; }

        public override bool GetConditionExec()
        {
            if (QuestTools.EnableDebugLogging)
                Logger.Log("Initializing '{0}' with condition={1}", Name, Condition);

            BotBehaviorQueue.Queue(new QueueItem
            {
                Condition = ret => ScriptManager.GetCondition(Condition).Invoke(),
                Name = Name,
                Nodes = Body,
                Persist = Persist,
                Repeat = Repeat,
            });

            return false;
        }
    }
}

