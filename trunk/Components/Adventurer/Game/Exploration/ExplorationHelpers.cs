﻿using System;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;
using Remit.Core;
using Trinity.UI.Visualizer;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;


namespace Trinity.Components.Adventurer.Game.Exploration
{
    public static class ExplorationHelpers
    {
        //public static AdventurerNode NearestWeightedUnvisitedNodeLocation(HashSet<int> levelAreaIds = null)
        //{
        //    using (new PerformanceLogger("NearestWeightedUnvisitedNodeLocation", true))
        //    {
        //        return NodesStorage.CurrentWorldNodes.Where(n => !n.Visited && n.NavigableCenter.Distance2DSqr(AdvDia.MyPosition) > 100 && n.NavigableCenter.IsInLevelAreas(levelAreaIds))
        //            .OrderByDescending(n => n.NearbyVisitedNodesCount)
        //            .ThenBy(n => n.NavigableCenter.Distance2DSqr(AdvDia.MyPosition))
        //            .FirstOrDefault();
        //    }
        //}

        public static Vector3 PriorityPosition { get; private set; }

        public static void SetExplorationPriority(Vector3 position)
        {
            PriorityPosition = position;
            VisualizerViewModel.DebugPosition = position;

            foreach (var connection in Core.Scenes.CurrentScene.GetConnectedScenes(Core.Scenes.GetScene(position)))
            {
                connection.Scene.Nodes.ForEach(n => n.Priority = true);
            }
        }

        public static bool IsInPriorityDirection(Vector3 position, double degreesDifferenceAllowed)
        {
            return MathUtil.GetRelativeAngularVariance(Core.Player.Position, PriorityPosition, position) <= degreesDifferenceAllowed;
        }

        public static ExplorationNode NearestWeightedUnvisitedNode(HashSet<int> levelAreaIds, List<string> ignoreScenes = null)
        {
            var dynamicWorldId = AdvDia.CurrentWorldDynamicId;
            var myPosition = AdvDia.MyPosition;
            ExplorationNode node = null;

            using (new PerformanceLogger("NearestUnvisitedNodeLocation", true))
            {
                var nearestNode = ExplorationGrid.Instance.GetNearestWalkableNodeToPosition(myPosition);

                // Try for nodes nearby walkable nodes first
                for (var i = 3; i <= 4; i++)
                {
                    var closestUnvisitedNodes = ExplorationGrid.Instance.GetNeighbors(nearestNode, i)
                        .Where(n => !n.IsIgnored && !n.IsVisited && !n.IsBlacklisted && n.HasEnoughNavigableCells &&
                                    n.DynamicWorldId == dynamicWorldId && levelAreaIds.Contains(n.LevelAreaId) && 
                                    Core.Grids.CanRayWalk(myPosition, n.NavigableCenter))
                        .OrderByDescending(n => n.Priority)
                        .ThenBy(PriorityDistanceFormula);
                        
                    var closestUnvisitedNode = closestUnvisitedNodes.FirstOrDefault();
                    if (closestUnvisitedNode != null)
                    {
                        Core.Logger.Debug(LogCategory.Exploration, $"Explore: Selected Nearby Node: [{closestUnvisitedNode.NavigableCenter.X},{closestUnvisitedNode.NavigableCenter.Y},{closestUnvisitedNode.NavigableCenter.Z}] Dist:{closestUnvisitedNode.Distance} {(closestUnvisitedNode.Priority ? "(Priority)" : "")} ");
                        return closestUnvisitedNode;
                    }
                }

                // Try any nearby nodes
                for (var i = 3; i <= 6; i++)
                {
                    var closestUnvisitedNode = ExplorationGrid.Instance.GetNeighbors(nearestNode, i)
                        .Where(n => !n.IsIgnored && !n.IsVisited && !n.IsBlacklisted && n.HasEnoughNavigableCells &&
                        n.DynamicWorldId == dynamicWorldId && levelAreaIds.Contains(n.LevelAreaId))
                        .OrderBy(PriorityDistanceFormula)
                        .FirstOrDefault();

                    if (closestUnvisitedNode != null)
                    {
                        Core.Logger.Debug(LogCategory.Exploration, $"Explore: Selected Nearby Node: [{closestUnvisitedNode.NavigableCenter.X},{closestUnvisitedNode.NavigableCenter.Y},{closestUnvisitedNode.NavigableCenter.Z}] Dist:{closestUnvisitedNode.Distance} {(closestUnvisitedNode.Priority ? "(Priority)" : "")} ");
                        return closestUnvisitedNode;
                    }
                }

                // Try any univisted node by distance.
                node = ExplorationGrid.Instance.WalkableNodes.Where(n =>
                        !n.IsIgnored &&
                        !n.IsVisited &&
                        !n.IsBlacklisted &&
                        //n.DynamicWorldId == dynamicWorldId &&
                        n.NavigableCenter.DistanceSqr(myPosition) > 50 //&& levelAreaIds.Contains(n.LevelAreaId)
                        )
                    .OrderByDescending(PriorityDistanceFormula)
                    .FirstOrDefault();

                if (node != null)
                {
                    Core.Logger.Debug(LogCategory.Exploration, $"Explore: Selected Nearby Node: [{node.NavigableCenter.X},{node.NavigableCenter.Y},{node.NavigableCenter.Z}] Dist:{node.Distance} {(node.Priority ? "(Priority)" : "")} ");
                    return node;
                }

                if (ExplorationGrid.Instance.NearestNode != null && !levelAreaIds.Contains(ZetaDia.CurrentLevelAreaSnoId))
                {
                    Core.Logger.Debug("[ExplorationLogic] Adventurer is trying to find nodes that are not in this LevelArea. DefinedIds='{0}' CurrentId='{0}'. Marking current area's nodes as valid.", string.Join(", ", levelAreaIds), ZetaDia.CurrentLevelAreaSnoId);
                    levelAreaIds.Add(ZetaDia.CurrentLevelAreaSnoId);

                    // Ignore level area match
                    node = ExplorationGrid.Instance.WalkableNodes
                    .Where(n =>
                        !n.IsIgnored &&
                        !n.IsVisited &&
                        !n.IsBlacklisted &&
                        n.DynamicWorldId == dynamicWorldId &&
                        n.NavigableCenter.DistanceSqr(myPosition) > 100)
                    .OrderByDescending(PriorityDistanceFormula)
                    .FirstOrDefault();
                }

                if (node == null)
                {
                    var allNodes = ExplorationGrid.Instance.WalkableNodes.Count(n => levelAreaIds.Contains(n.LevelAreaId));
                    var unvisitedNodes = ExplorationGrid.Instance.WalkableNodes.Count(n => !n.IsVisited && levelAreaIds.Contains(n.LevelAreaId));

                    Core.Logger.Log("[ExplorationLogic] Couldn't find any unvisited nodes. Current AdvDia.LevelAreaSnoIdId: {0}, " +
                                "ZetaDia.CurrentLevelAreaSnoId: {3}, Total Nodes: {1} Unvisited Nodes: {2} Searching In [{4}] HasNavServerData={5}",
                                AdvDia.CurrentLevelAreaId, allNodes, unvisitedNodes,
                                ZetaDia.CurrentLevelAreaSnoId, string.Join(", ", levelAreaIds),
                                AdvDia.MainGridProvider.Width != 0);

                    //Core.Scenes.Reset();
                }

                return node;
            }
        }

        public static float PriorityDistanceFormula(ExplorationNode n)
        {
            var directionMultiplier = IsInPriorityDirection(n.NavigableCenter, 30) ? 4 : 0;
            var priorityMultiplier = n.Priority ? 2 : 0;
            var baseDistanceFactor = 1/n.NavigableCenter.Distance(AdvDia.MyPosition);
            return baseDistanceFactor * (n.UnvisitedWeight + directionMultiplier + priorityMultiplier);
        }

        /// <summary>
        /// Defines areas of a scene where exploration nodes should be blacklisted
        /// Index by SceneSnoId (Can be found on InfoDumping Tab). Each WorldScene grabs this list of BlacklistedPositions
        /// When Exploration nodes are created within a scene, they are marked 'IsBlacklsited' if any position is inside their bounds.
        /// </summary>
        public static Dictionary<int, HashSet<Vector3>> BlacklistedPositionsBySceneSnoId = new Dictionary<int, HashSet<Vector3>>
        {
            { 53711, new HashSet<Vector3>
                {
                    // Dulgur oasis scene with decking area in the bottom left of map.
                    new Vector3(811.1603f,646.8987f,1.828806f),
                    new Vector3(814.6924f,668.8029f,1.828797f),
                    new Vector3(833.3029f,652.3396f,1.828796f)
                }
            }
        };

        private static WorldScene _currentScene;

        public static void MarkNodesAsVisited(IEnumerable<string> sceneNames)
        {
            MarkSceneNodes(sceneNames, true);
        }

        public static void MarkNodesAsUnvisited(IEnumerable<string> sceneNames)
        {
            MarkSceneNodes(sceneNames, false);
        }

        public static void MarkSceneNodes(IEnumerable<string> sceneNames, bool asVisited)
        {
            if (sceneNames == null)
                return;

            sceneNames = sceneNames.Select(s => s.ToLowerInvariant()).ToList();
            foreach (var scene in Core.Scenes)
            {
                var fullname = scene.Name.ToLowerInvariant();
                if (sceneNames.Any(s => fullname.Contains(s)))
                {
                    foreach (var node in scene.Nodes)
                    {
                        node.IsVisited = asVisited;
                    }
                    Core.Logger.Debug($"Marked {scene.Nodes.Count} exploration nodes for scene {fullname} ({scene.SnoId}) as visited");
                }
            }
        }

        public static void UpdateIgnoreRegions()
        {
            var currentScene = Core.Scenes.CurrentScene;
            if (currentScene != null && currentScene != _currentScene)
            {
                MarkIgnoreRegionsAsVisited(currentScene);
                foreach (var connectedScene in currentScene.ConnectedScenes())
                {
                    MarkIgnoreRegionsAsVisited(connectedScene.Scene);
                }
                _currentScene = currentScene;
            }
        }

        public static bool MarkIgnoreRegionsAsVisited(WorldScene scene)
        {
            if (scene?.Nodes == null) return false;
            int modifiedNodes = 0;
            foreach (var node in scene.Nodes)
            {
                if (node == null) continue;
                if (scene.IgnoreRegions.Contains(node.NavigableCenter))
                {
                    modifiedNodes++;
                    node.IsVisited = true;
                }
            }
            Core.Logger.Debug($"Marked {modifiedNodes} exploration nodes for ignore regions in {scene.Name} ({scene.SnoId}) as visited");
            return modifiedNodes > 0;
        }

        public static void MarkNodesAsVisited(IEnumerable<WorldScene> scenes)
        {
            var sceneLookup = new HashSet<int>(scenes.Select(s => s.SnoId));
            ExplorationGrid.Instance.WalkableNodes.Where(s => sceneLookup.Contains(s.Scene.SnoId)).ForEach(s => s.IsVisited = true);
        }

        public static List<Vector3> GetFourPointsInEachDirection(Vector3 center, int radius)
        {
            var result = new List<Vector3>();
            var nearestNode = ExplorationGrid.Instance.GetNearestNode(center) as ExplorationNode;
            if (nearestNode == null) return result;
            return ExplorationGrid.GetExplorationNodesInRadius(nearestNode, radius).Where(n => n.HasEnoughNavigableCells & n.NavigableCenter.Distance(AdvDia.MyPosition) > 10).Select(n => n.NavigableCenter).ToList();
            try
            {
                var node1 = ExplorationGrid.Instance.GetNearestWalkableNodeToPosition(new Vector3(center.X - radius, center.Y + radius, 0));
                if (node1 != null)
                {
                    result.Add(node1.NavigableCenter);
                }
                var node2 = ExplorationGrid.Instance.GetNearestWalkableNodeToPosition(new Vector3(center.X + radius, center.Y + radius, 0));
                if (node2 != null)
                {
                    result.Add(node2.NavigableCenter);
                }

                var node3 = ExplorationGrid.Instance.GetNearestWalkableNodeToPosition(new Vector3(center.X + radius, center.Y - radius, 0));
                if (node3 != null)
                {
                    result.Add(node3.NavigableCenter);
                }

                var node4 = ExplorationGrid.Instance.GetNearestWalkableNodeToPosition(new Vector3(center.X - radius, center.Y - radius, 0));
                if (node4 != null)
                {
                    result.Add(node4.NavigableCenter);
                }
            }
            catch (Exception)
            {
            }

            return result;
        }
    }
}