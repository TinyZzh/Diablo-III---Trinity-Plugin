using System.ComponentModel;
using System.Threading.Tasks;
using QuestTools.Helpers;
using QuestTools.ProfileTags.Complex;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    [XmlElement("TrinitySetQuesting")]
    [XmlElement("SetQuesting")]
    public class SetQuestingTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        public SetQuestingTag() { }
        private bool _isDone;

        public override bool IsDone
        {
            get { return _isDone; }
        }

        [XmlAttribute("mode")]
        [DefaultValue(true)]
        public bool Mode { get; set; }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ret => SetQuestingTask());
        }

        public async Task<bool> SetQuestingTask()
        {
            Logger.Debug("Attempting to set Trinity IsQuestingMode to {0}", Mode);
            if (!TrinityApi.SetProperty("Trinity.Combat.Abilities.CombatBase", "IsQuestingMode", Mode))
            {
                //Logger.Error("Unable to set IsQuestingMode Property");
            }
            object isQuestingMode;
            if (!TrinityApi.GetProperty("Trinity.Combat.Abilities.CombatBase", "IsQuestingMode", out isQuestingMode))
            {
                //Logger.Error("Unable to read IsQuestingMode property for validation");
            }
            if (isQuestingMode as bool? == Mode)
                Logger.Log("Successfully set Trinity Combat mode as QUESTING for the current profile.");
            _isDone = true;
            return true;
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
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
            _isDone = true;
        }

        #endregion
    }
}
