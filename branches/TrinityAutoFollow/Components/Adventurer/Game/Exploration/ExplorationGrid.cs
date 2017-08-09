﻿using System;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Trinity.Components.Adventurer.Game.Events;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;

namespace Trinity.Components.Adventurer.Game.Exploration
{
    public sealed class ExplorationGrid : Grid<ExplorationNode>
    {
        private const int GRID_BOUNDS = 500;

        private static readonly ConcurrentDictionary<int, List<Vector3>> KnownPositions = new ConcurrentDictionary<int, List<Vector3>>();

        private static Lazy<ExplorationGrid> _currentGrid;
        private static Lazy<ExplorationGrid> _lastGrid;

        public ExplorationGrid()
        {
            BotMain.OnStart += (ibot) => Clear();
        }

        public static ExplorationGrid GetWorldGrid(int worldDynamicId)
        {
            var worldId = ZetaDia.Globals.WorldId;
            if (_lastGrid?.Value?.WorldDynamicId == worldId)
            {
                var cur = _currentGrid;
                _currentGrid = _lastGrid;
                _lastGrid = cur;
            }
            if (_currentGrid?.Value == null || worldId != _currentGrid.Value.WorldDynamicId)
            {
                _lastGrid = _currentGrid;
                _currentGrid = new Lazy<ExplorationGrid>(() => new ExplorationGrid());
            }
            return _currentGrid.Value;
        }

        public static void Clear()
        {
            KnownPositions.Clear();
            _currentGrid = null;
            _lastGrid = null;
        }

        public static ExplorationGrid Instance
        {
            get { return GetWorldGrid(AdvDia.CurrentWorldDynamicId); }
        }

        public List<ExplorationNode> WalkableNodes = new List<ExplorationNode>();
        private static WorldScene _currentScene;

        public override float BoxSize { get; } = 20;

        public override int GridBounds
        {
            get { return GRID_BOUNDS; }
        }

        public override bool CanRayCast(Vector3 from, Vector3 to)
        {
            return false;
        }

        public override bool CanRayWalk(Vector3 from, Vector3 to)
        {
            return GetRayLineAsNodes(from, to).All(node => node.HasEnoughNavigableCells);
        }

        public override void Reset()
        {
            //WorldGrids.Clear();
        }

        public ExplorationNode GetNearestWalkableNodeToPosition(Vector3 position)
        {
            var nodeLine = GetRayLineAsNodes(AdvDia.MyPosition, position);
            var lastNode = nodeLine.LastOrDefault(n => n.HasEnoughNavigableCells);
            return lastNode;
        }

        private IEnumerable<ExplorationNode> GetRayLineAsNodes(Vector3 from, Vector3 to)
        {
            var rayLine = GetRayLine(from, to);
            return rayLine.Select(point => InnerGrid[point.X, point.Y]).Where(n => n is ExplorationNode).Cast<ExplorationNode>();
        }

        public static List<IGroupNode> GetExplorationNodesInRadius(ExplorationNode centerNode, float radius)
        {
            var gridDistance = Instance.ToGridDistance(radius);
            var neighbors = centerNode.GetNeighbors(gridDistance, true);
            return neighbors.Where(n => n.Center.DistanceSqr(centerNode.NavigableCenter2D) < radius * radius).ToList();
        }

        public static void ResetKnownPositions()
        {
            KnownPositions.Clear();
        }

        public static void PulseSetVisited()
        {
            var nearestNode = Instance.NearestNode as ExplorationNode;

            if (nearestNode != null && !nearestNode.IsKnown)
            {
                var currentWorldKnownPositions = KnownPositions.GetOrAdd(AdvDia.CurrentWorldDynamicId, new List<Vector3>());
                currentWorldKnownPositions.Add(nearestNode.Center.ToVector3());
                nearestNode.IsKnown = true;
                nearestNode.IsVisited = true;

                var worldScene = AdvDia.CurrentWorldScene;
                worldScene.HasBeenVisited = true;               

                var radius = 40;
                switch (PluginEvents.CurrentProfileType)
                {
                    case ProfileType.Rift:
                        radius = 55;
                        
                        if (worldScene != null && worldScene.Name.Contains("Exit"))
                        {
                            radius = 30;
                        }
                        break;

                    case ProfileType.Bounty:
                        radius = 55;
                        break;

                    case ProfileType.Keywarden:
                        radius = 70;
                        break;
                }
                foreach (var node in GetExplorationNodesInRadius(nearestNode, radius))
                {
                    node.IsVisited = true;
                }
            }

            if (!BotMain.IsRunning)
            {
                // Update ignore regions for debug on visualizer.
                // Normally this should only be executed if ExploreCoroutine chooses,
                // because it introduces potential stucks and skipping target actors
                ExplorationHelpers.UpdateIgnoreRegions();
            }

        }



        protected override void OnUpdated(SceneData newNodes)
        {
            var nodes = newNodes.ExplorationNodes.Cast<ExplorationNode>().ToList();

            UpdateInnerGrid(nodes);

            foreach (var node in nodes)
            {
                if (node == null)
                    continue;

                node.AStarValue = (byte)(node.HasEnoughNavigableCells ? 1 : 2);

                if (!node.IsIgnored && node.HasEnoughNavigableCells)
                    WalkableNodes.Add(node);
            }

            Core.Logger.Debug("[ExplorationGrid] Updated WalkableNodes={0}", WalkableNodes.Count);
        }
    }
}