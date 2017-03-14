using Buddy.Coroutines;
using System.Linq;
using System.Threading.Tasks;
using Trinity.Coroutines.Town;
using Trinity.Framework;
using Trinity.Framework.Objects.Enums;
using Zeta.Game;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Coroutines
{
    public class OpenTreasureBags
    {
        public static async Task<bool> Execute()
        {
            if (Core.Settings.Items.StashTreasureBags)
                return false;

            var bagsOpened = 0;
            if (Core.Player.IsInTown)
            {
                foreach (var item in Core.Inventory.Backpack.ToList())
                {
                    if (item.RawItemType == RawItemType.TreasureBag)
                    {
                        Logger.Log($"Opening Treasure Bag {bagsOpened + 1}, Id={item.AnnId}");
                        InventoryManager.UseItem(item.AnnId);
                        bagsOpened++;
                        await Coroutine.Sleep(500);
                    }
                }
                if (bagsOpened > 0)
                {
                    Logger.Log($"Waiting for Treasure Bag loot");
                    await Coroutine.Sleep(2500);
                    TrinityTownRun.IsWantingTownRun = true;
                    return true;
                }
            }
            return false;
        }
    }
}