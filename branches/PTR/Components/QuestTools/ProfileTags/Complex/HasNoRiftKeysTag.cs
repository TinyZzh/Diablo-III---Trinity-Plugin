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
    [XmlElement("HasNoRiftKeys")]
    public class HasNoRiftKeysTag : BaseComplexNodeTag
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

        private Func<ACDItem, bool> IsRiftKeyFunc
        {
            get { return i => i.ItemType == ItemType.KeystoneFragment; }
        }

        public override bool GetConditionExec()
        {
            bool backpack = ZetaDia.Me.Inventory.Backpack.Any(IsRiftKeyFunc);
            bool stash = ZetaDia.Me.Inventory.StashItems.Any(IsRiftKeyFunc);

            return !backpack && !stash;
        }
    }
}
