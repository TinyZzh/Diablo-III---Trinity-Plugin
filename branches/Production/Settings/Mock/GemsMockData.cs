using System.Collections.Generic;
using Trinity.Components.Adventurer.Settings;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;

namespace Trinity.Settings.Mock
{
    public class GemsMockData : NotifyBase
    {
        private AdventurerGems _gems;

        public AdventurerGems Gems
        {
            get { return _gems; }
            set { SetField(ref _gems, value); }
        }

        public GemsMockData()
        {
            Gems = new AdventurerGems();
            
        }

    }
}