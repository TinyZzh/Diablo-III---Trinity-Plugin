using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace QuestTools.Helpers
{
    public static class Keys
    {
//        private static readonly int[] KeyIds = { 364694, 364695, 364696, 364697 };
        private static readonly int[] KeyIds = { 366946, 366947, 366948, 366949 };

        private static readonly double[] KeyCounts = { 0, 0, 0, 0 };

        private static DateTime _lastCheckedKeys = DateTime.MinValue;
        private static readonly Func<ACDItem, int, bool> IsKeyId = (acd, sno) => KeyIds.Any(k => k == acd.ActorSnoId);
        private static IOrderedEnumerable<double> _orderedKeyCounts;

        public static double Act1
        {
            get { Update(); return KeyCounts[0]; }
        }

        public static double Act2
        {
            get { Update(); return KeyCounts[1]; }
        }

        public static double Act3
        {
            get { Update(); return KeyCounts[2]; }
        }

        public static double Act4
        {
            get { Update(); return KeyCounts[3]; }
        }

        public static bool IsAllSameCount
        {
            get { Update(); return KeyCounts.All(count => count == KeyCounts[0]); }
        }

        public static int HighestKeyId
        {
            get { Update(); return KeyIds[Array.IndexOf(KeyCounts, KeyCounts.Max())]; }
        }

        public static int LowestKeyId
        {
            get { Update(); return KeyIds[Array.IndexOf(KeyCounts, KeyCounts.Min())]; }
        }

        public static double HighestKeyCount
        {
            get { Update(); return KeyCounts.Max(); }
        }

        public static double LowestKeyCount
        {
            get { Update(); return KeyCounts.Min(); }
        }

        private static double _upperQuartile;
        public static double UpperQuartile
        {
            get { Update(); return _upperQuartile; }
        }
        
        private static double _lowerQuartile;
        public static double LowerQuartile
        {
            get { Update(); return _lowerQuartile; }
        }

        private static double _interQuartileRange;
        public static double InterQuartileRange
        {
            get { Update(); return _interQuartileRange; }
        }

        private static double _lowerFence;
        public static double LowerFence
        {
            get { Update(); return _lowerFence; }
        }

        private static double _upperFence;
        public static double UpperFence
        {
            get { Update(); return _upperFence; }
        }

        private static double _median;
        public static double Median
        {
            get { Update(); return _median; }
        }

        public static bool Update()
        {
            if (DateTime.UtcNow.Subtract(_lastCheckedKeys).TotalSeconds < 10) 
                return false;

            KeyCounts[0] = 0;
            KeyCounts[1] = 0;
            KeyCounts[2] = 0;
            KeyCounts[3] = 0;

            var keys = ZetaDia.Me.Inventory.StashItems.Where(IsKeyId).Concat(ZetaDia.Me.Inventory.Backpack.Where(IsKeyId)).ToList();
            keys.ForEach(key => { KeyCounts[Array.IndexOf(KeyIds, key.ActorSnoId)] += key.ItemStackQuantity; });
            _lastCheckedKeys = DateTime.UtcNow;

            _orderedKeyCounts = KeyCounts.OrderBy(k => k) as IOrderedEnumerable<Double>;
            _upperQuartile = _orderedKeyCounts.UpperQuartile();
            _lowerQuartile = _orderedKeyCounts.LowerQuartile();
            _median = _orderedKeyCounts.MiddleQuartile();
            _interQuartileRange = _orderedKeyCounts.InterQuartileRange();
            _lowerFence = _lowerQuartile - (1.5 * _interQuartileRange);
            _upperFence = _upperQuartile + (1.5 * _interQuartileRange);

            return true;
        }

        public static double GetKeyCount(Act act)
        {
            Update();
            switch (act)
            {
                case Act.A1: return KeyCounts[0];
                case Act.A2: return KeyCounts[1];
                case Act.A3: return KeyCounts[2];
                case Act.A4: return KeyCounts[3];
            }
            return 0;
        }

        public static double GetKeyCount(int actorId)
        {
            Update();
            var keyIdIndex = Array.IndexOf(KeyIds, actorId);   
            return KeyCounts.ElementAtOrDefault(keyIdIndex);      
        }

        public static void PrintKeyCounts()
        {
            Update();

            Logger.Log(string.Format(

                "Counts: " +
                "\n           Act 1 => {0} " +
                "\n           Act 2 => {1}" +
                "\n           Act 3 => {2}" +
                "\n           Act 4 => {3}",

                KeyCounts[0],
                KeyCounts[1],
                KeyCounts[2],
                KeyCounts[3]));

            Logger.Log(string.Format(
                
                "Stats:" +
                "\n           LF => {0:0.#}" +
                "\n           LQ => {1:0.#}" +
                "\n           M => {2:0.#}" +
                "\n           UQ => {3:0.#}" +
                "\n           UF => {4:0.#}" +
                "\n           IQR => {5:0.#}",

                _lowerFence,
                _lowerQuartile,
                _median,
                _upperQuartile,
                _upperFence,
                _interQuartileRange));            
        }

        public static int GetKeyIdNotWithinRange(int range = 2)
        {
            Update();

            foreach (var keyId in KeyIds)
            {
                var count = GetKeyCount(keyId);
                if (count < HighestKeyId - range && count > HighestKeyId + range)
                    return keyId;                
            }

            return 0;
        }

    }
}
