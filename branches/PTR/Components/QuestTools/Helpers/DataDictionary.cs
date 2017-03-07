using System.Collections.Generic;

namespace QuestTools.Helpers
{
    class DataDictionary
    {
        public static readonly Dictionary<int, string> LegendaryGems = new Dictionary<int, string>
        {
            {0,"Equipped Gems"},
            {1,"Lowest Rank"},
            {2,"Highest Rank"},
            {405775,"Bane of the Powerful"},
            {405781,"Bane of the Trapped"},
            {428345,"Bane of the Stricken"},
            {405804,"Taeguk"},
            {405801,"Zei's Stone of Vengeance"},
            {405796,"Gogok of Swiftness"},
            {428033,"Esoteric Alteration"},
            {405792,"Wreath of Lightning"},
            {405793,"Gem of Efficacious Toxin"},
            {405794,"Pain Enhancer"},
            {405795,"Mirinae, Teardrop of the Starweaver"},
            {405797,"Invigorating Gemstone"},
            {405798,"Enforcer"},
            {405800,"Moratorium"},
            {405802,"Simplicity's Strength"},
            {405803,"Boon of the Hoarder"},
            {405783,"Gem of Ease"},
            {428034,"Molten Wildebeest’s Gizzard"},
            {428355,"Iceblink"},
            {428346,"Mutilation Guard"},            
        };

        public static HashSet<int> PandemoniumFortressWorlds { get { return _pandemoniumFortressWorlds; } }
        private static readonly HashSet<int> _pandemoniumFortressWorlds = new HashSet<int>
        {
            271233, // Adventure Pand Fortress 1
            271235, // Adventure Pand Fortress 2
        };

        public static HashSet<int> PandemoniumFortressLevelAreaIds { get { return _pandemoniumFortressLevelAreaIds; } }
        private static readonly HashSet<int> _pandemoniumFortressLevelAreaIds = new HashSet<int>
        {
            333758, //LevelArea: X1_LR_Tileset_Fortress
        };

        public static HashSet<int> DeathGates { get { return _deathGates; } }
        private static readonly HashSet<int> _deathGates = new HashSet<int>()
        {
            328830, // x1_Fortress_Portal_Switch
        };

        /// <summary>
        /// Contains a list of Rift WorldId's
        /// </summary>
        public static List<int> RiftWorldIds { get { return riftWorldIds; } }
        private static readonly List<int> riftWorldIds = new List<int>
        {
            288454,
            288685,
            288687,
            288798,
            288800,
            288802,
            288804,
            288810,
            288814,
            288816,
        };

        /// <summary>
        /// Contains all the Exit Name Hashes in Rifts
        /// </summary>
        public static List<int> RiftPortalHashes { get { return riftPortalHashes; } }
        private static readonly List<int> riftPortalHashes = new List<int>
        {
			1938876094,
			1938876095,
			1938876096,
			1938876097,
			1938876098,
			1938876099,
			1938876100,
			1938876101,
			1938876102,
		};
        public static HashSet<int> ForceTownPortalLevelAreaIds { get { return forceTownPortalLevelAreaIds; } }
        private static readonly HashSet<int> forceTownPortalLevelAreaIds = new HashSet<int>
        {
            55313, // Act 2 Caldeum Bazaar
        };
        public static HashSet<int> BountyTurnInQuests { get { return bountyTurnInQuests; } }
        private static readonly HashSet<int> bountyTurnInQuests = new HashSet<int>
        {
            356988, //x1_AdventureMode_BountyTurnin_A1 
            356994, //x1_AdventureMode_BountyTurnin_A2 
            356996, //x1_AdventureMode_BountyTurnin_A3 
            356999, //x1_AdventureMode_BountyTurnin_A4 
            357001, //x1_AdventureMode_BountyTurnin_A5 
        };

        public static HashSet<int> GuardedBountyGizmoIds { get { return guardedBountyGizmoIds; } }
        private static readonly HashSet<int> guardedBountyGizmoIds = new HashSet<int>
        {
            430733, //A1 Templar Inquisition
            432259, //A1 The Triune Reborn
            434366, //A1 : Wortham Survivors 
            432770, //A1: Queen's Desert
            433051, //A2: Prisoners of the Cult
            432331, //A2: Blood and Iron
            432885, //A2: The Ancient Devices
            433184, //A3: The Lost Patrol
            433295, //A3: The Demon Gates
            433385, //A3: Catapult Command (It's a switch instead of chest, so might be pre 2.2 bounty)
            433402, //A4: Hell Portals
            433124, //A4: Tormented Angels
            433246, //A5: Death's Embrace
            433316 //A5: Rathma's Gift
                                                                          
        };


    }
}
