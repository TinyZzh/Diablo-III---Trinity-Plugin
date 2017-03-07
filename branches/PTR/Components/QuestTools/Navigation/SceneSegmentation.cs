using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using QuestTools.Helpers;
using Zeta.Bot;
using Zeta.Bot.Dungeons;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.SNO;

namespace QuestTools.Navigation
{
    class SceneSegmentation
    {
        static SceneSegmentation()
        {
            GameEvents.OnWorldChanged += (sender, args) =>
            {
                _scenes.Clear();
            };
        }

        private static ConcurrentBag<DungeonNode> _nodes = new ConcurrentBag<DungeonNode>();
        public static ConcurrentBag<DungeonNode> Nodes
        {
            get
            {
                if (_nodes.IsEmpty)
                    Update();
                return _nodes;
            }
            set { _nodes = value; }
        }

        private static Dictionary<int,CachedScene> _scenes = new Dictionary<int,CachedScene>();
        public static List<CachedScene> Scenes
        {
            get
            {
                if (!_scenes.Any())
                    Update();

                return new List<CachedScene>(_scenes.Values);
            }
        }

        internal static CachedScene CurrentScene
        {
            get { return SceneHistory.CurrentScene; }
        }

        internal static CachedScene PreviousScene
        {
            get { return SceneHistory.PreviousScene; }
        }

        public class CachedScene
        {
            public int UniqueId { get; set; }
            public int SceneHash { get; set; }
            public Scene Scene { get; set; }
            public string Name { get; set; }
            public List<DungeonNode> Nodes = new List<DungeonNode>();
            public DungeonNode BaseNode { get; set; }
            public DungeonNode NorthNode { get; set; }
            public DungeonNode SouthNode { get; set; }
            public DungeonNode EastNode { get; set; }
            public DungeonNode WestNode { get; set; }
            public Vector2 ZoneMin { get; set; }
            public Vector2 ZoneMax { get; set; }
            public NavZone Zone { get; set; }
            public List<Direction> ExitCodes = new List<Direction>();
            public bool IsDeadEnd { get; set; }
            public bool IsMainHub { get; set; }
            public bool IsCorridor { get; set; }
            public bool IsExit { get; set; }
            public bool IsEntrance { get; set; }
            public int MinEdgeLength { get; set; }
            public int HalfEdgeLength { get; set; }

            public bool IsConnectedEast
            {
                get { return EastConnection != null; }
            }
            public bool IsConnectedWest
            {
                get { return WestConnection != null; }
            }
            public bool IsConnectedNorth
            {
                get { return NorthConnection != null; }
            }
            public bool IsConnectedSouth
            {
                get { return SouthConnection != null; }
            }

            public HashSet<int> ConnectedSceneGuids = new HashSet<int>();
            public List<CachedScene> ConnectedScenes = new List<CachedScene>();
            public int ConnectionCount { get; set; }

            public CachedScene SouthConnection { get; set; }
            public CachedScene NorthConnection { get; set; }
            public CachedScene EastConnection { get; set; }
            public CachedScene WestConnection { get; set; }

            public Vector3 Center { get; set; }
            public Vector3 NorthEdgeCenter { get; set; }
            public Vector3 SouthEdgeCenter { get; set; }
            public Vector3 EastEdgeCenter { get; set; }
            public Vector3 WestEdgeCenter { get; set; }

            internal bool IsConnectedTo(CachedScene scene)
            {
                return GetSceneConnectionPoint(this, scene) != Vector3.Zero;
            }

            internal List<Direction> UnexploredExits
            {
                get
                {
                    var result = new List<Direction>();
                    ExitCodes.ForEach(exitCode =>
                    {
                        if (exitCode == Direction.South && !IsConnectedSouth)
                            result.Add(exitCode);
                        else if (exitCode == Direction.East && !IsConnectedEast)
                            result.Add(exitCode);
                        else if (exitCode == Direction.West && !IsConnectedWest)
                            result.Add(exitCode);
                        else if (exitCode == Direction.North && !IsConnectedNorth)
                            result.Add(exitCode);
                    });
                    return result;
                }
            }

            /// <summary>
            /// We know (most of the time) from the scene name, how many connections each scene has to other scenes.
            /// So a scene is considered explored if we have a CachedScene object reference stored for each required direction.
            /// </summary>
            public bool IsExplored
            {
                get { return ConnectedScenes.Count == ExitCodes.Count; }
            }

            public override string ToString()
            {
                return String.Format("SceneHash={0} Name={1} Exits={2} UnexploredExits={3}",
                    SceneHash,
                    Name,
                    String.Join(",", ExitCodes.Select(p => p.ToString()).ToArray()),
                    String.Join(",", UnexploredExits.Select(p => p.ToString()).ToArray())
                );
            }

            public override bool Equals(object obj)
            {
                return obj.GetHashCode() == GetHashCode();
            }

            public override int GetHashCode()
            {
                return Scene.SceneId;
            }
        }

        private static readonly Regex SceneConnectionDirectionsRegex = new Regex("_([NSEW]{1,})_", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static void Update()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var oldNodes = _nodes;

            var newScenes = ZetaDia.Scenes.Where(s => s.Mesh.Zone != null).ToList();

            int minEdgeLength = (int)Math.Ceiling(newScenes.Min(s => Math.Min(s.Mesh.Zone.ZoneMax.X - s.Mesh.Zone.ZoneMin.X, s.Mesh.Zone.ZoneMax.Y - s.Mesh.Zone.ZoneMin.Y)));
            int halfEdgeLength = minEdgeLength / 2;

            var nodes = new List<DungeonNode>();
            var cachedScenes = new List<CachedScene>();

            var completeScenes = new HashSet<int>(_scenes.Where(pair => pair.Value.IsExplored).Select(pair => pair.Key));

            // Iterate through scenes, find connecting scene names and create a dungeon node to navigate to the scene center
            newScenes.AsParallel().ForEach(scene =>
            {
                if (scene.Name.EndsWith("_Filler") || !scene.IsValid)
                    return;

                var zone = scene.Mesh.Zone;
                var zoneMin = zone.ZoneMin;
                var zoneMax = zone.ZoneMax;
                var sceneHash = GetSceneHash(zone);

                if (completeScenes.Contains(sceneHash))
                    return;

                var cachedScene = new CachedScene
                {
                    Scene = scene,
                    SceneHash = sceneHash,
                    Name = scene.Name,
                    Zone = zone,
                    ZoneMax = zoneMax,
                    ZoneMin = zoneMin,
                    MinEdgeLength = minEdgeLength,
                    HalfEdgeLength = halfEdgeLength
                };
                
                PopulateExitCodes(cachedScene);

                var baseNode = new DungeonNode(zoneMin, zoneMax);
                cachedScene.BaseNode = baseNode;
                cachedScene.Nodes.Add(baseNode);
                if (nodes.All(node => node.WorldTopLeft != baseNode.WorldTopLeft))
                    nodes.Add(baseNode);

                cachedScene.Center = new Vector3(zoneMax.X - (zoneMax.X - zoneMin.X) / 2, zoneMax.Y - (zoneMax.Y - zoneMin.Y) / 2, 0);
                cachedScene.NorthEdgeCenter = new Vector3(zoneMin.X, zoneMax.Y - (zoneMax.Y - zoneMin.Y) / 2, 0);
                cachedScene.SouthEdgeCenter = new Vector3(zoneMax.X, zoneMax.Y - (zoneMax.Y - zoneMin.Y) / 2, 0);
                cachedScene.EastEdgeCenter = new Vector3(zoneMax.X - (zoneMax.X - zoneMin.X) / 2, zoneMax.Y, 0);
                cachedScene.WestEdgeCenter = new Vector3(zoneMax.X - (zoneMax.X - zoneMin.X) / 2, zoneMin.Y, 0);

                // North
                var northNode = (new DungeonNode(new Vector2(zoneMin.X - halfEdgeLength, zoneMin.Y), new Vector2(zoneMax.X - halfEdgeLength, zoneMin.Y)));
                cachedScene.NorthNode = northNode;
                cachedScene.Nodes.Add(northNode);
                if (nodes.All(node => node.WorldTopLeft != northNode.WorldTopLeft))
                    nodes.Add(northNode);

                // South
                var southNode = (new DungeonNode(new Vector2(zoneMin.X + halfEdgeLength, zoneMin.Y), new Vector2(zoneMax.X + halfEdgeLength, zoneMin.Y)));
                cachedScene.SouthNode = southNode;
                cachedScene.Nodes.Add(southNode);
                if (nodes.All(node => node.WorldTopLeft != southNode.WorldTopLeft))
                    nodes.Add(southNode);

                // East
                var eastNode = (new DungeonNode(new Vector2(zoneMin.X, zoneMin.Y - halfEdgeLength), new Vector2(zoneMax.X, zoneMin.Y - halfEdgeLength)));
                cachedScene.EastNode = eastNode;
                cachedScene.Nodes.Add(eastNode);
                if (nodes.All(node => node.WorldTopLeft != eastNode.WorldTopLeft))
                    nodes.Add(eastNode);
  
                // West
                var westNode = (new DungeonNode(new Vector2(zoneMin.X, zoneMin.Y + halfEdgeLength), new Vector2(zoneMax.X, zoneMin.Y + halfEdgeLength)));
                cachedScene.WestNode = westNode;
                cachedScene.Nodes.Add(westNode);
                if (nodes.All(node => node.WorldTopLeft != westNode.WorldTopLeft))
                    nodes.Add(westNode);


                if (!_scenes.ContainsKey(cachedScene.SceneHash))
                    _scenes.Add(cachedScene.SceneHash, cachedScene);
                else
                    _scenes[cachedScene.SceneHash] = cachedScene;
            });

            _scenes.Values.ForEach(PopulateSceneConnections);

            if (oldNodes != null && oldNodes.Any())
            {
                nodes.AsParallel().ForEach(node =>
                {
                    var oldNode = oldNodes.FirstOrDefault(n => node.WorldTopLeft == n.WorldTopLeft);
                    if (oldNode != null && oldNode.Visited)
                        node.Visited = true;
                });

                oldNodes.AsParallel().ForEach(oldNode =>
                {
                    if (nodes.All(newNode => newNode.Center != oldNode.WorldTopLeft))
                    {
                        nodes.Add(oldNode);
                    }
                });
            }

            SceneHistory.UpdateSceneHistory();

            _nodes = new ConcurrentBag<DungeonNode>(nodes.Distinct());
            Logger.Debug("Updated SceneSegmentation {0} Scenes {1} nodes in {2:0}ms", _scenes.Count, _nodes.Count, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Exit codes (the _NSEW_ part of a scene name) are used to link scenes together
        /// I am aware that some scenes (entrances/exit/special) scenes do not have these codes. 
        /// Todo: address non-coded scenes.
        /// </summary>
        public static void PopulateExitCodes(CachedScene scene)
        {
            var exitCodes = SceneConnectionDirectionsRegex.Matches(scene.Name);

            foreach (var exitCode in from object exitCode in exitCodes select exitCode.ToString())
            {
                var exitCount = exitCode.Trim('_').Count();

                scene.ConnectionCount = exitCount;

                if (exitCount == 1)
                    scene.IsDeadEnd = true;

                else if (exitCount == 2)
                    scene.IsCorridor = true;

                else if (exitCount == 4 || exitCount == 3)
                    scene.IsMainHub = true;

                foreach (var exitId in exitCode)
                {
                    if (exitId == 'E')
                        scene.ExitCodes.Add(Direction.East);

                    if (exitId == 'W')
                        scene.ExitCodes.Add(Direction.West);

                    if (exitId == 'N')
                        scene.ExitCodes.Add(Direction.North);

                    if (exitId == 'S')
                        scene.ExitCodes.Add(Direction.South);
                }
            }
        }

        /// <summary>
        /// Links scenes objects together by cardinal direction
        /// </summary>
        /// <param name="scene"></param>
        public static void PopulateSceneConnections(CachedScene scene)
        {
            scene.ConnectedScenes.Clear();
            scene.ConnectedSceneGuids.Clear();

            scene.WestConnection = Scenes.FirstOrDefault(s => s.SceneHash != scene.SceneHash && s.EastEdgeCenter == scene.WestEdgeCenter && s.ExitCodes.Contains(Direction.East));
            scene.EastConnection = Scenes.FirstOrDefault(s => s.SceneHash != scene.SceneHash && s.WestEdgeCenter == scene.EastEdgeCenter && s.ExitCodes.Contains(Direction.West));
            scene.NorthConnection = Scenes.FirstOrDefault(s => s.SceneHash != scene.SceneHash && s.SouthEdgeCenter == scene.NorthEdgeCenter && s.ExitCodes.Contains(Direction.South));
            scene.SouthConnection = Scenes.FirstOrDefault(s => s.SceneHash != scene.SceneHash && s.NorthEdgeCenter == scene.SouthEdgeCenter && s.ExitCodes.Contains(Direction.North));

            if (scene.SouthConnection != null)
            {
                scene.ConnectedSceneGuids.Add(scene.SouthConnection.SceneHash);
                scene.ConnectedScenes.Add(scene.SouthConnection);
            }

            if (scene.WestConnection != null)
            {
                scene.ConnectedSceneGuids.Add(scene.WestConnection.SceneHash);
                scene.ConnectedScenes.Add(scene.WestConnection);
            }

            if (scene.NorthConnection != null)
            {
                scene.ConnectedSceneGuids.Add(scene.NorthConnection.SceneHash);
                scene.ConnectedScenes.Add(scene.NorthConnection);
            }

            if (scene.EastConnection != null)
            {
                scene.ConnectedSceneGuids.Add(scene.EastConnection.SceneHash);
                scene.ConnectedScenes.Add(scene.EastConnection);
            }            
        }

        /// <summary>
        /// Scene History is used in scene based pathfinding to avoid backtracking.
        /// </summary>
        public static class SceneHistory
        {
            private static CachedScene _currentScene;
            private static CachedScene _previousScene;

            public static CachedScene CurrentScene
            {
                get
                {
                    if (_currentScene == null)
                        UpdateSceneHistory();

                    return _currentScene;
                }
                set { _currentScene = value; }
            }

            public static CachedScene PreviousScene
            {
                get
                {
                    if (_currentScene == null)
                        UpdateSceneHistory();

                    return _previousScene;
                }
                set { _previousScene = value; }
            }

            internal static void UpdateSceneHistory()
            {
                if (ZetaDia.Me == null || !ZetaDia.Me.IsValid || ZetaDia.Me.IsDead)
                    return;

                CachedScene currentScene;

                if (!_scenes.TryGetValue(GetSceneHash(ZetaDia.Me.CurrentScene.Mesh.Zone), out currentScene))
                    return;

                if (currentScene == null)
                    return;

                if (_currentScene == null)
                {
                    _currentScene = currentScene;
                    return;
                }

                if (currentScene.SceneHash != _currentScene.SceneHash)
                    _previousScene = _currentScene;

                _currentScene = currentScene;
 
            }
        }

        /// <summary>
        /// Scene names/SNOs are reused on dynamic worlds, as are SceneGuids
        /// SceneHash is a unique Id based on its location in the current world.
        /// </summary>
        public static int GetSceneHash(Scene scene)
        {
            return GetSceneHash(scene.Mesh.Zone);
        }

        public static int GetSceneHash(NavZone zone)
        {
            return (int)((zone.ZoneMin.X + zone.ZoneMin.Y * 17) + (zone.ZoneMax.X + zone.ZoneMax.Y * 21)) ^ zone.SceneSnoId;
        }

        /// <summary>
        /// Clears and updates the Node List
        /// </summary>
        public static void Reset()
        {
            _scenes.Clear();
            _nodes = new ConcurrentBag<DungeonNode>();
            Update();
        }

        /// <summary>
        /// Gets the center of a given Navigation Zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        internal static Vector3 GetNavZoneCenter(NavZone zone)
        {            
            float x = zone.ZoneMin.X + ((zone.ZoneMax.X - zone.ZoneMin.X) / 2);
            float y = zone.ZoneMin.Y + ((zone.ZoneMax.Y - zone.ZoneMin.Y) / 2);
            return new Vector3(x, y, 0);
        }

        /// <summary>
        /// Gets the center of a given Navigation Cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        internal static Vector3 GetNavCellCenter(NavCell cell, NavZone zone)
        {
            return GetNavCellCenter(cell.Min, cell.Max, zone);
        }

        /// <summary>
        /// Gets the center of a given box with min/max, adjusted for the Navigation Zone
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        internal static Vector3 GetNavCellCenter(Vector3 min, Vector3 max, NavZone zone)
        {
            float x = zone.ZoneMin.X + min.X + ((max.X - min.X) / 2);
            float y = zone.ZoneMin.Y + min.Y + ((max.Y - min.Y) / 2);
            float z = min.Z + ((max.Z - min.Z) / 2);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Returns the point at which two givens scenes connect; Vector3.Zero if they don't connect.
        /// </summary>
        internal static Vector3 GetSceneConnectionPoint(CachedScene targetScene, CachedScene thisScene = null)
        {
            thisScene = thisScene ?? CurrentScene;

            if (thisScene.IsConnectedEast && thisScene.EastConnection.Equals(targetScene))
                return thisScene.EastEdgeCenter;

            if (thisScene.IsConnectedWest && thisScene.WestConnection.Equals(targetScene))
                return thisScene.WestEdgeCenter;

            if (thisScene.IsConnectedSouth && thisScene.SouthConnection.Equals(targetScene))
                return thisScene.SouthEdgeCenter;

            if (thisScene.IsConnectedNorth && thisScene.NorthConnection.Equals(targetScene))
                return thisScene.NorthEdgeCenter;

            return Vector3.Zero;
        }

        /// <summary>
        /// Returns a list of points that cover the closest walkable points to the four edges and center of the specified scene.
        /// </summary>
        internal static List<Vector3> GetSceneExploreRoute(CachedScene targetScene = null)
        {
            targetScene = targetScene ?? CurrentScene;
            var zone = targetScene.Scene.Mesh.Zone;
            var points = zone.NavZoneDef.NavCells.Where(n => n.IsValid && n.Flags.HasFlag(NavCellFlags.AllowWalk)).Select(n => GetNavCellCenter(n, zone)).ToList();

            var route = new List<Vector3>
            {
                points.OrderBy(point => point.Distance2D(targetScene.Center)).FirstOrDefault(), 
                points.OrderBy(point => point.Distance2D(targetScene.EastEdgeCenter)).FirstOrDefault(), 
                points.OrderBy(point => point.Distance2D(targetScene.NorthEdgeCenter)).FirstOrDefault(), 
                points.OrderBy(point => point.Distance2D(targetScene.SouthEdgeCenter)).FirstOrDefault(), 
                points.OrderBy(point => point.Distance2D(targetScene.WestEdgeCenter)).FirstOrDefault()
            };

            if (QuestToolsSettings.Instance.DebugEnabled)
            {
                Logger.Debug("Scene Explore Route generated for {0} ({1}) with {2} points", targetScene.Name, targetScene.SceneHash, route.Count);
                route.ForEach(n => Logger.Log("Route Point: {0} Distance={1} Direction={2}", n, n.Distance(ZetaDia.Me.Position), MathUtil.GetDirectionToPoint(n)));
            }                

            return route;
        }

        /// <summary>
        /// Returns a list of points between the source scene and the target scene.
        /// </summary>
        /// <param name="targetScene"></param>
        /// <param name="fromScene"></param>
        /// <returns></returns>
        internal static List<Vector3> GetVectorPathToScene(CachedScene targetScene, CachedScene fromScene = null)
        {
            fromScene = fromScene ?? CurrentScene;
            var scenePath = GetPathToScene(targetScene, fromScene);
            var vectorPath = new List<Vector3>();
            var previousScene = fromScene;

            scenePath.ForEach(scene =>
            {
                vectorPath.Add(GetSceneConnectionPoint(previousScene, scene));
                vectorPath.Add(scene.Center);
                previousScene = scene;
            });

            if (QuestToolsSettings.Instance.DebugEnabled)
            {
                Logger.Debug("Scene Route generated from {0} ({1}) to {2} ({3}) with {4} points", fromScene.Name, fromScene.SceneHash, targetScene.Name, targetScene.SceneHash, vectorPath.Count);
                vectorPath.ForEach(n => Logger.Log("Route Point: {0} Distance={1} Direction={2}", n, n.Distance(ZetaDia.Me.Position), MathUtil.GetDirectionToPoint(n)));
            }    

            return vectorPath;
        }

        /// <summary>
        /// Returns a list of scenes between the source scene and the target scene.
        /// </summary>
        internal static List<CachedScene> GetPathToScene(CachedScene targetScene, CachedScene fromScene = null)
        {
            fromScene = fromScene ?? CurrentScene;
            var direction = MathUtil.GetDirectionToPoint(targetScene.Center, fromScene.Center);
            var checkedSceneHashes = new List<int>();
            var path = new List<CachedScene>();
            var found = false;
            var done = false;
            var currentScene = fromScene;
            const int depthStop = 20;
            var depth = 0;

            while (!found || depth > depthStop || done)
            {
                if (currentScene.IsConnectedTo(targetScene))
                {
                    path.Add(targetScene); 
                    found = true;
                }
                else
                {
                    checkedSceneHashes.Add(currentScene.SceneHash);
                    
                    // Attempt to get new scene by direction
                    var newScene = GetConnectedScene(direction, currentScene);

                    // Failing that, just grab any connected scene
                    if (checkedSceneHashes.Contains(newScene.SceneHash))
                        newScene = currentScene.ConnectedScenes.FirstOrDefault(s => !checkedSceneHashes.Contains(s.SceneHash));

                    if (newScene == null)
                    {
                        done = true;
                        continue;
                    }

                    currentScene = newScene;
                    path.Add(newScene);                        
                }
                depth++;
            }
            return path;
        }

        /// <summary>
        /// The closest scene that has its one or more of its connected scenes undiscovered.
        /// </summary>
        internal static CachedScene GetNearestUnexploredScene(CachedScene thisScene = null)
        {
            var scene = thisScene ?? CurrentScene;
            return Scenes.Where(s => !s.IsExplored).OrderBy(s => scene.Center.Distance2D(s.Center)).FirstOrDefault();
        }

        /// <summary>
        /// Returns a scene that is directly connected to the specified scene; avoids DeadEnds and leans towards the specified direction.
        /// </summary>
        /// <param name="direction">the long range direction you want to go</param>
        /// <param name="thisScene">source scene</param>
        /// <param name="avoidPreviousScenes">whether to avoid scenes you've been in recently</param>
        /// <returns></returns>
        internal static CachedScene GetConnectedScene(Direction direction = Direction.Any, CachedScene thisScene = null, bool avoidPreviousScenes = true)
        {
            var scene = thisScene ?? CurrentScene;
            var previousScene = SceneHistory.PreviousScene;

            CachedScene result = null;

            switch (direction)
            {
                case Direction.North:
                    if (scene.IsConnectedNorth && !scene.NorthConnection.IsDeadEnd)
                        result = scene.NorthConnection;
                    else if (scene.IsConnectedEast && !scene.EastConnection.IsDeadEnd && (!avoidPreviousScenes || previousScene.SceneHash != scene.EastConnection.SceneHash))
                        result = scene.EastConnection;
                    else if (scene.IsConnectedWest && !scene.WestConnection.IsDeadEnd)
                        result = scene.WestConnection;
                    break;

                case Direction.South:
                    if (scene.IsConnectedSouth && !scene.SouthConnection.IsDeadEnd)
                        result = scene.SouthConnection;
                    else if (scene.IsConnectedEast && !scene.EastConnection.IsDeadEnd && (!avoidPreviousScenes || previousScene.SceneHash != scene.EastConnection.SceneHash))
                        result = scene.EastConnection;
                    else if (scene.IsConnectedWest && !scene.WestConnection.IsDeadEnd)
                        result = scene.WestConnection;
                    break;

                case Direction.East:
                    if (scene.IsConnectedEast && !scene.EastConnection.IsDeadEnd)
                        result = scene.EastConnection;
                    else if (scene.IsConnectedSouth && !scene.SouthConnection.IsDeadEnd && (!avoidPreviousScenes || previousScene.SceneHash != scene.SouthConnection.SceneHash))
                        result = scene.SouthConnection;
                    else if (scene.IsConnectedNorth && !scene.NorthConnection.IsDeadEnd)
                        result = scene.NorthConnection;
                    break;

                case Direction.West:
                    if (scene.IsConnectedWest && !scene.WestConnection.IsDeadEnd)
                        result = scene.WestConnection;
                    else if (scene.IsConnectedSouth && !scene.SouthConnection.IsDeadEnd && (!avoidPreviousScenes || previousScene.SceneHash != scene.SouthConnection.SceneHash))
                        result = scene.SouthConnection;
                    else if (scene.IsConnectedNorth && !scene.NorthConnection.IsDeadEnd)
                        result = scene.NorthConnection;
                    break;

                case Direction.SouthEast:
                    if (scene.IsConnectedSouth && !scene.SouthConnection.IsDeadEnd)
                        result = scene.SouthConnection;
                    else if (scene.IsConnectedEast && !scene.EastConnection.IsDeadEnd)
                        result = scene.EastConnection;
                    break;

                case Direction.NorthEast:
                    if (scene.IsConnectedNorth && !scene.NorthConnection.IsDeadEnd)
                        result = scene.NorthConnection;
                    else if (scene.IsConnectedEast && !scene.EastConnection.IsDeadEnd)
                        result = scene.EastConnection;
                    break;

                case Direction.SouthWest:
                    if (scene.IsConnectedSouth && !scene.SouthConnection.IsDeadEnd)
                        result = scene.SouthConnection;
                    else if (scene.IsConnectedWest && !scene.WestConnection.IsDeadEnd)
                        result = scene.WestConnection;
                    break;

                case Direction.NorthWest:
                    if (scene.IsConnectedNorth && !scene.NorthConnection.IsDeadEnd)
                        result = scene.NorthConnection;
                    else if (scene.IsConnectedWest && !scene.WestConnection.IsDeadEnd)
                        result = scene.WestConnection;
                    break;
            }

            return result ?? scene.ConnectedScenes.FirstOrDefault();
        }
    }


}
