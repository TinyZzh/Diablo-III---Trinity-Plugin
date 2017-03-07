using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Bot.Profile;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Complex
{
    [XmlElement("HasGreaterRiftKeys")]
    public class HasGreaterRiftKeysTag : BaseComplexNodeTag
    {
        protected override Composite CreateBehavior()
        {
            return
             new Decorator(ret => !IsDone,
                 new PrioritySelector(
                     base.GetNodes().Select(b => b.Behavior).ToArray()
                 )
             );
        }

        private Func<ACDItem, bool> IsGreaterRiftKeyFunc
        {
            get { return i => i.ItemType == ItemType.KeystoneFragment && i.TieredLootRunKeyLevel > 0; }
        }

        public override bool GetConditionExec()
        {
            bool backpack = ZetaDia.Me.Inventory.Backpack.Any(IsGreaterRiftKeyFunc);
            bool stash = ZetaDia.Me.Inventory.StashItems.Any(IsGreaterRiftKeyFunc);

            return backpack || stash;
        }
    }
}
