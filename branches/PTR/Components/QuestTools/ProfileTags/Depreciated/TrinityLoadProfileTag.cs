using Zeta.Bot.Profile.Common;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Depreciated
{
    [XmlElement("TrinityLoadProfile")]
    public class TrinityLoadProfileTag : LoadProfileTag
    {
        public TrinityLoadProfileTag() { }

        public override void OnStart()
        {
            Logger.Error("TrinityLoadProfile is decpreciated. Use <LoadProfile /> instead.");
            base.OnStart();
        }
    }
}
