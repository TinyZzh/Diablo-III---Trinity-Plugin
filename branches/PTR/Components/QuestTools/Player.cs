using System;
using Zeta.Game;

namespace QuestTools
{
    public class Player
    {
        private static int _cachedLevelAreaId = -1;
        private static DateTime _lastUpdatedLevelAreaId = DateTime.MinValue;
        public static int LevelAreaId
        {
            get
            {
                if (_cachedLevelAreaId != -1 && DateTime.UtcNow.Subtract(_lastUpdatedLevelAreaId).TotalSeconds < 2)
                    return _cachedLevelAreaId;
                _cachedLevelAreaId = ZetaDia.CurrentLevelAreaSnoId;
                _lastUpdatedLevelAreaId = DateTime.UtcNow;
                return _cachedLevelAreaId;
            }
        }

        private static long _cachedCoinage = -1;
        private static DateTime _lastUpdatedCoinage = DateTime.MinValue;
        public static long Coinage
        {
            get
            {
                if (_cachedCoinage != -1 && DateTime.UtcNow.Subtract(_lastUpdatedCoinage).TotalSeconds < 2)
                    return _cachedCoinage;
                _cachedCoinage = ZetaDia.PlayerData.Coinage;
                _lastUpdatedCoinage = DateTime.UtcNow;
                return _cachedCoinage;
            }
        }

        public static bool IsValid
        {
            get
            {
                if (ZetaDia.Me != null &&
                    ZetaDia.Me.IsValid &&
                    ZetaDia.Service.IsValid &&
                    ZetaDia.IsInGame &&
                    !ZetaDia.Globals.IsLoadingWorld)
                    return true;

                return false;                
            }
        }

    }
}
