using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Common;
using Zeta.Game;

namespace QuestTools.Helpers
{
    public class PositionCache
    {
        private static HashSet<Vector3> _cache = new HashSet<Vector3>();
        public static HashSet<Vector3> Cache { get { return _cache; } set { _cache = value; } }

        private static DateTime _lastRecordedTime = DateTime.MinValue;

        const float MinRecordDistance = 25f;

        private static Vector3 _lastPosition = Vector3.Zero;

        public static void RecordPosition()
        {
            if (Cache == null)
                Cache = new HashSet<Vector3>();

            if (DateTime.UtcNow.Subtract(_lastRecordedTime).TotalMilliseconds < 1000)
                return;

            Vector3 myPos = ZetaDia.Me.Position;
            if (_lastPosition.Distance2DSqr(myPos) < MinRecordDistance * MinRecordDistance)
                return;

            _lastPosition = myPos;
            _lastRecordedTime = DateTime.UtcNow;
            Cache.Add(myPos);
        }
    }
}
