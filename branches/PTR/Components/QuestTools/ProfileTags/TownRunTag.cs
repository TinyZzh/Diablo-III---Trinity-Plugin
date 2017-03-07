using System.Linq;
using System.Threading.Tasks;
using QuestTools.Helpers;
using QuestTools.ProfileTags.Complex;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    // TrinityTownRun forces a town-run request
    [XmlElement("TownRun")]
    [XmlElement("TrinityTownRun")]
    public class TownRunTag : ProfileBehavior, IEnhancedProfileBehavior
    {
        public TownRunTag() { }

        private bool _isDone;

        public override bool IsDone
        {
            get { return _isDone; }
        }

        /// <summary>
        /// The number of free bag slots we must have in order to NOT do a town run
        /// </summary>
        [XmlAttribute("minFreeBagSlots")]
        public int MinFreeBagSlots { get; set; }

        /// <summary>
        /// The minimum durability we must have in order to NOT do a townrun
        /// </summary>
        [XmlAttribute("minDurabilityPercent")]
        [XmlAttribute("minDurability")]
        public int MinDurability { get; set; }

        /// <summary>
        /// Returns true when the number of free slots is less than the MinFreeBagSlots
        /// </summary>
        /// <returns></returns>
        public bool CheckMinBagSlots()
        {
            if (MinFreeBagSlots > 60)
                MinFreeBagSlots = 60;
            var freeSlots = GetFreeSlots();
            Logger.Debug("Checking free slots: {0}/{1}", freeSlots, MinFreeBagSlots);
            return freeSlots < MinFreeBagSlots;
        }

        public int GetFreeSlots()
        {
            const int maxFreeSlots = 60;
            int slotsTaken = 0;

            bool participatingInTieredLootRun = ZetaDia.Me.CommonData.GetAttribute<int>(ActorAttributeType.ParticipatingInTieredLootRun) > 0;
            if (participatingInTieredLootRun)
            {
                return maxFreeSlots;
            }


            if (MinFreeBagSlots == 0)
                return maxFreeSlots;

            foreach (var item in ZetaDia.Me.Inventory.Backpack)
            {
                slotsTaken++;
                if (item.IsTwoSquareItem)
                    slotsTaken++;
            }

            return maxFreeSlots - slotsTaken;
        }

        /// <summary>
        /// returns True when the lowest durability item is less than the min durabilty
        /// </summary>
        /// <returns></returns>
        public bool CheckDurability()
        {
            if (MinDurability == 0)
                return false;

            var minDurabilityEquipped = GetMinDurability();
            Logger.Debug("Checking minimum durability: {0}/{1}", minDurabilityEquipped, MinDurability);
            return minDurabilityEquipped <= MinDurability;
        }

        public float GetMinDurability()
        {
            return ZetaDia.Me.Inventory.Equipped.Min(i => i.DurabilityPercent) * 100;
        }

        public override void OnStart()
        {
            Logger.Log("TrinityTownRun, freeBagSlots={0} minDurabilityPercent={1}", MinFreeBagSlots, MinDurability);
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ret => TownRun());
        }

        private async Task<bool> TownRun()
        {
            if (CheckDurability() || CheckMinBagSlots())
            {
                Logger.Log("Town-run request received, will town-run at next possible moment.");
                if (!TrinityApi.SetField("Trinity.Trinity", "ForceVendorRunASAP", true))
                {
                    Logger.Verbose("Unable to set field ForceVendorRunASAP!");
                }
                _isDone = true;
            }
            else
            {
                Logger.Log("Skipping TownRun");
                _isDone = true;
            }
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
