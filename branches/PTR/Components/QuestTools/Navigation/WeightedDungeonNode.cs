using Zeta.Bot.Dungeons;
using Zeta.Common;

namespace QuestTools.Navigation
{
    class WeightedDungeonNode : DungeonNode
    {
        public WeightedDungeonNode(Vector2 worldTopLeft, Vector2 worldBottomRight) : base(worldTopLeft, worldBottomRight)
        {
        }

        public double Weight { get; set; }
    }
}
