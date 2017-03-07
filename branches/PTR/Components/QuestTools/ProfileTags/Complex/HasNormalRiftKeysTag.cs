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
    [XmlElement("HasNormalRiftKeys")]
    public class HasNormalRiftKeysTag : BaseComplexNodeTag
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

        private Func<ACDItem, bool> IsNormalRiftKeyFunc
        {
            get { return i => i.ItemType == ItemType.KeystoneFragment && i.TieredLootRunKeyLevel == -1; }
        }

        public override bool GetConditionExec()
        {
            bool backpack = ZetaDia.Me.Inventory.Backpack.Any(IsNormalRiftKeyFunc);
            bool stash = ZetaDia.Me.Inventory.StashItems.Any(IsNormalRiftKeyFunc);

            return backpack || stash;
        }
    }
}
