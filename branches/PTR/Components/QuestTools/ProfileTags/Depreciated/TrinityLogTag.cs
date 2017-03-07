using Zeta.Bot.Profile.Common;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Depreciated
{
    [XmlElement("TrinityLog")]
    public class TrinityLogTag : LogMessageTag
    {
        public TrinityLogTag() { }

        public override void OnStart()
        {
            Logger.Error("TrinityLog is decpreciated. Use <LogMessage /> instead.");
            base.OnStart();
        }
    }
}
