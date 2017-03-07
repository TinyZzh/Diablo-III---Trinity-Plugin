using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Documents;
using QuestTools.Helpers;
using QuestTools.ProfileTags.Complex;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Bot.Settings;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    /// <summary>
    /// Class CombatSettingTag.
    /// </summary>
    [XmlElement("CombatSetting")]
    class CombatSettingTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone; }
        }

        /// <summary>
        /// Gets or sets the size of the trash pack.
        /// </summary>
        /// <value>The size of the trash pack.</value>
        [XmlAttribute("trashPackSize")]
        public int TrashPackSize { get; set; }

        /// <summary>
        /// Gets or sets the non elite range.
        /// </summary>
        /// <value>The non elite range.</value>
        [XmlAttribute("nonEliteRange")]
        [XmlAttribute("killRadius")]
        public int NonEliteRange { get; set; }

        /// <summary>
        /// Gets or sets the trash pack cluster radius.
        /// </summary>
        /// <value>The trash pack cluster radius.</value>
        [XmlAttribute("trashPackClusterRadius")]
        public float TrashPackClusterRadius { get; set; }

        /// <summary>
        /// Turns on/off the killing of monsters
        /// </summary>
        [XmlAttribute("combat")]
        public string Combat { get; set; }

        /// <summary>
        /// Turns on/off the looting of items
        /// </summary>
        [XmlAttribute("looting")]
        public string Looting { get; set; }

        /// <summary>
        /// Gets or sets the DB looting range
        /// </summary>
        /// <value>The looting range.</value>
        [XmlAttribute("lootRadius")]
        public int LootRadius { get; set; }

        ///// <summary>
        ///// Gets or sets the avoidance of area effect spells
        ///// </summary>
        //[XmlAttribute("avoidance")]
        //public bool AvoidAoe { get; set; }

        /// <summary>
        /// Creates the behavior.
        /// </summary>
        /// <returns>Composite.</returns>
        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ret => CombatSettingTask());
        }

        /// <summary>
        /// Combats the setting task.
        /// </summary>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        private async Task<bool> CombatSettingTask()
        {
            var trinityType = TrinityApi.GetTrinityType();
            var settings = TrinityApi.GetStaticPropertyFromType(trinityType, "Settings");
            var combatSettings = TrinityApi.GetInstancePropertyFromObject(settings, "Combat");
            var miscCombatSetting = TrinityApi.GetInstancePropertyFromObject(combatSettings, "Misc");

            var nonEliteRange = TrinityApi.GetInstancePropertyInfoFromObject(miscCombatSetting, "NonEliteRange");
            var trashPackSize = TrinityApi.GetInstancePropertyInfoFromObject(miscCombatSetting, "TrashPackSize");
            var trashPackClusterRadius = TrinityApi.GetInstancePropertyInfoFromObject(miscCombatSetting, "TrashPackClusterRadius");
            //var avoidAoe = TrinityApi.GetInstancePropertyInfoFromObject(miscCombatSetting, "AvoidAOE");

            new ToggleTargetingTag {
                Combat = string.IsNullOrEmpty(Combat) ? CombatTargeting.Instance.AllowedToKillMonsters : Combat.ChangeType<bool>(),
                Looting = string.IsNullOrEmpty(Looting) ? LootTargeting.Instance.AllowedToLoot : Combat.ChangeType<bool>(),
                LootRadius = LootRadius > 0 ? LootRadius : CharacterSettings.Instance.LootRadius,
                KillRadius = NonEliteRange > 0 ? NonEliteRange : CharacterSettings.Instance.KillRadius,
            }.OnStart();

            if (TrashPackSize > 0)
            {
                Logger.Log("Setting Trinity Combat.Misc.TrashPackSize to {0}", TrashPackSize);
                trashPackSize.SetValue(miscCombatSetting, TrashPackSize);
            }
            if (NonEliteRange > 0)
            {
                Logger.Log("Setting Trinity Combat.Misc.NonEliteRange to {0}", NonEliteRange);
                nonEliteRange.SetValue(miscCombatSetting, NonEliteRange);
            }
            if (TrashPackClusterRadius > 0)
            {
                Logger.Log("Setting Trinity Combat.Misc.TrashPackClusterRadius to {0}", TrashPackClusterRadius);
                trashPackClusterRadius.SetValue(miscCombatSetting, TrashPackClusterRadius);
            }

            //if (avoidAoe.GetValue(miscCombatSetting).ChangeType<bool>() != AvoidAoe)
            //{
            //    Logger.Log("Setting Trinity Combat.Misc.AvoidAOE to {0}", AvoidAoe);
            //    avoidAoe.SetValue(miscCombatSetting, AvoidAoe);
            //}

            _isDone = true;
            return true;
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

