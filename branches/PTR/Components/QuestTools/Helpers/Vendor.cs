using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestTools.Helpers;

namespace QuestTools.Helpers
{
    public enum VendorSlot
    {
        None = 0,
        OneHandItem,
        TwoHandItem,
        Quiver,
        Orb,
        Mojo,
        Helm,
        Gloves,
        Boots,
        Chest,
        Belt,
        Shoulder,
        Pants,
        Bracers,
        Shield,
        Ring,
        Amulet,
    }

    public static class Vendor
    {
        public static Dictionary<VendorSlot, int> MysterySlotTypeAndId = new Dictionary<VendorSlot, int>
        {
            {VendorSlot.OneHandItem,377355}, 
            {VendorSlot.TwoHandItem,377356}, 
            {VendorSlot.Quiver,377360}, 
            {VendorSlot.Orb,377358}, 
            {VendorSlot.Mojo,377359}, 
            {VendorSlot.Helm,377344}, 
            {VendorSlot.Gloves,377346}, 
            {VendorSlot.Boots,377347}, 
            {VendorSlot.Chest,377345}, 
            {VendorSlot.Belt,377349}, 
            {VendorSlot.Pants,377350}, 
            {VendorSlot.Bracers,377351}, 
            {VendorSlot.Shield,377357}, 
            {VendorSlot.Ring,377352}, 
            {VendorSlot.Amulet,377353},
            {VendorSlot.Shoulder,377348}                    
        };

        public static Dictionary<VendorSlot, int> MysterySlotTypeAndPrice = new Dictionary<VendorSlot, int>
        {              
            {VendorSlot.OneHandItem,75}, 
            {VendorSlot.TwoHandItem,75}, 
            {VendorSlot.Quiver,25}, 
            {VendorSlot.Orb,25}, 
            {VendorSlot.Mojo,25}, 
            {VendorSlot.Helm,25}, 
            {VendorSlot.Gloves,25}, 
            {VendorSlot.Boots,25}, 
            {VendorSlot.Chest,25}, 
            {VendorSlot.Belt,25}, 
            {VendorSlot.Pants,25}, 
            {VendorSlot.Bracers,25}, 
            {VendorSlot.Shield,25}, 
            {VendorSlot.Ring,50}, 
            {VendorSlot.Amulet,100},
            {VendorSlot.Shoulder,25}    
        };     
   
    }

}
