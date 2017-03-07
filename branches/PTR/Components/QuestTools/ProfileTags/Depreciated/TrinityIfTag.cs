using Zeta.Bot.Profile.Composites;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Depreciated
{
    [XmlElement("TrinityIf")]
    public class TrinityIfTag : IfTag
    {
        public TrinityIfTag() { }
        public override void OnStart()
        {
            Logger.Error("TrinityIf is decpreciated. Use <If condition=\"\" /> instead.");
            base.OnStart();
        }

    }
}
