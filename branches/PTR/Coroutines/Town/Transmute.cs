using Buddy.Coroutines;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Coroutines.Town
{
    public static class Transmute
    {
        public static async Task<bool> Execute(List<TrinityItem> transmuteGroup)
        {
            if (!UIElements.TransmuteItemsDialog.IsVisible)
            {
                await MoveToAndInteract.Execute(TownInfo.KanaisCube);
                await Coroutine.Sleep(1000);
            }
            var acds = transmuteGroup.Select(i => Core.Actors.GetAcdItemByAcdId(i.AcdId)).ToList();
            return await Execute(acds);
        }

        public static async Task<bool> Execute(List<ACDItem> transmuteGroup)
        {
            if (!ZetaDia.IsInGame)
                return false;

            if (transmuteGroup.Count > 9)
            {
                Logger.Log(" --> Can't convert with more than 9 items!");
                return false;
            }

            Logger.Log("Transmuting:");

            foreach (var item in transmuteGroup)
            {
                if (item == null || !item.IsValid || item.IsDisposed)
                {
                    Logger.Log(" --> Invalid Item Found {0}");
                    return false;
                }

                if (!item.IsCraftingReagent && item.Level < 70)
                {
                    Logger.Log($" --> The internal item level for {item.Name} is {item.Level}; most items less than 70 level will cause a failed transmute");
                    return false;
                }

                Logger.Log($@" --> {item.Name} StackQuantity={item.ItemStackQuantity} Quality={item.GetItemQuality()} CraftingMaterial={item.IsCraftingReagent}
                                   InventorySlot={item.InventorySlot} Row={item.InventoryRow} Col={item.InventoryColumn}  (Ann={item.AnnId} AcdId={item.ACDId})");
            }

            if (!UIElements.TransmuteItemsDialog.IsVisible)
            {
                Logger.Log("Cube window needs to be open before you can transmute anything.");
                return false;
            }

            Logger.Log("Zip Zap!");
            InventoryManager.TransmuteItems(transmuteGroup);
            return true;
        }
    }
}