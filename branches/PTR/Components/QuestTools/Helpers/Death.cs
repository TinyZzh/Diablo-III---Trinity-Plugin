using System;

namespace QuestTools.Helpers
{
    public class Death
    {
        public static int DeathCount { get; set; }
        public static DateTime LastDeathTime { get; set; }
        public static int MaxDeathsAllowed { get; set; }
    }
}
