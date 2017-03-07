using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using log4net.Core;
using Org.BouncyCastle.Utilities.IO;
using QuestTools.Helpers;
using QuestTools.Navigation;
using Zeta.Bot;
using Zeta.Bot.Dungeons;
using Zeta.Bot.Logic;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;

namespace QuestTools.UI
{
    class TabUi
    {
        private static Button _btnDumpActors, _btnOpenLogFile, _btnResetGrid, _btnDumpCPlayer;
        private static Button _btnSafeMoveTo, _btnMoveToActor, _btnMoveToMapMarker, _btnIfTag;
        private static Button _btnIfTagActorExists, _btnWaitTimerTag, _btnExploreAreaTag;
        private static Button _btnUseWaypointTag, _btnDumpEnvironment, _btnIfTagSceneIntersects, _btnTest;
        private static Button _btnDumpBounties, _btnDumpQuests;

        private const string Indent3Hang = "                       ";
        private const string Indent = "    ";

        internal static void InstallTab()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        // 1st column x: 432
                        // 2nd column x: 552
                        // 3rd column x: 672

                        // Y rows: 10, 33, 56, 79, 102

                        _btnDumpActors = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Dump Actor Attribs"
                        };

                        _btnDumpCPlayer = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Dump PlayerData"
                        };

                        _btnOpenLogFile = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Open Log File"
                        };

                        _btnResetGrid = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Force Reset Grid"
                        };

                        _btnSafeMoveTo = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "SafeMoveTo"
                        };

                        _btnMoveToActor = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "MoveToActor"
                        };

                        _btnMoveToMapMarker = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "MoveToMapMarker"
                        };

                        _btnIfTag = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "IfTag"
                        };

                        _btnIfTagActorExists = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "If ActorExists"
                        };

                        _btnWaitTimerTag = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "WaitTag"
                        };

                        _btnExploreAreaTag = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "ExploreTag"
                        };

                        _btnUseWaypointTag = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "UseWaypoint"
                        };

                        _btnDumpEnvironment = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Dump Environment"
                        };

                        _btnIfTagSceneIntersects = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "If SceneIntersects"
                        };

                        _btnTest = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Test"
                        };

                        _btnDumpBounties = new Button
                        {
                            Width = 120,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(3),
                            Content = "Dump Bounties"
                        };

                        Window mainWindow = Application.Current.MainWindow;

                        _btnDumpActors.Click += btnDumpActors_Click;
                        _btnDumpCPlayer.Click += _btnDumpCPlayer_Click;
                        _btnOpenLogFile.Click += btnOpenLogFile_Click;
                        _btnResetGrid.Click += btnResetGrid_Click;

                        _btnSafeMoveTo.Click += btnSafeMoveTo_Click;
                        _btnMoveToActor.Click += btnMoveToActor_Click;
                        _btnMoveToMapMarker.Click += btnMoveToMapMarker_Click;
                        _btnIfTag.Click += btnIfTag_Click;
                        _btnIfTagActorExists.Click += btnIfTagActorExists_Click;
                        _btnExploreAreaTag.Click += btnExploreAreaTag_Click;
                        _btnWaitTimerTag.Click += btnWaitTimerTag_Click;
                        _btnUseWaypointTag.Click += btnUseWaypointTag_Click;
                        _btnDumpEnvironment.Click += btnDumpEnvironment_Click;
                        _btnIfTagSceneIntersects.Click += btnIfTagSceneIntersects_Click;
                        _btnTest.Click += btnTest_Click;
                        _btnDumpBounties.Click += _btnDumpBounties_Click;

                        UniformGrid uniformGrid = new UniformGrid
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Top,
                            MaxHeight = 180,
                        };

                        uniformGrid.Children.Add(_btnDumpActors);
                        uniformGrid.Children.Add(_btnDumpCPlayer);
                        uniformGrid.Children.Add(_btnOpenLogFile);
                        uniformGrid.Children.Add(_btnResetGrid);

                        uniformGrid.Children.Add(_btnSafeMoveTo);
                        uniformGrid.Children.Add(_btnMoveToActor);
                        uniformGrid.Children.Add(_btnMoveToMapMarker);
                        uniformGrid.Children.Add(_btnIfTag);
                        uniformGrid.Children.Add(_btnIfTagActorExists);
                        uniformGrid.Children.Add(_btnExploreAreaTag);
                        uniformGrid.Children.Add(_btnWaitTimerTag);
                        uniformGrid.Children.Add(_btnUseWaypointTag);
                        uniformGrid.Children.Add(_btnDumpEnvironment);
                        uniformGrid.Children.Add(_btnIfTagSceneIntersects);
                        uniformGrid.Children.Add(_btnTest);
                        uniformGrid.Children.Add(_btnDumpBounties);

                        _tabItem = new TabItem
                        {
                            Header = "QuestTools",
                            ToolTip = "Profile Creation Tools",
                            Content = uniformGrid,
                        };

                        var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                        if (tabs == null)
                            return;

                        tabs.Items.Add(_tabItem);
                    }
                )
            );
        }

        static void _btnDumpBounties_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }
            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    Logger.Log("Dumping {0} Bounties", ZetaDia.ActInfo.Bounties.Count());
                    
                    foreach (var bountyInfo in ZetaDia.ActInfo.Bounties)
                    {
                        string levelAreas = bountyInfo.LevelAreas.Aggregate("", (current, area) => current + (area + ", "));

                        Logger.Log("Act={0} Name={1} Quest={2} ({3}) LevelArea={4} StartingLevelArea={5} State={6} QuestType={7} LevelAreas={8}",
                            bountyInfo.Act,
                            bountyInfo.Info.DisplayName,
                            bountyInfo.Quest,
                            (int)bountyInfo.Quest,
                            bountyInfo.Info.LevelArea,
                            bountyInfo.StartingLevelArea,
                            bountyInfo.State,
                            bountyInfo.Info.QuestType,
                            levelAreas);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        static void btnUseWaypointTag_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }
            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    List<GizmoWaypoint> objList = (from o in ZetaDia.Actors.GetActorsOfType<GizmoWaypoint>(true)
                                                   where o.IsValid
                                                   orderby o.Position.Distance(ZetaDia.Me.Position)
                                                   select o).ToList();

                    string tagText = "";
                    if (objList.Any())
                    {
                        GizmoWaypoint obj = objList.FirstOrDefault();

                        if (obj != null)
                            tagText = string.Format("\n<UseWaypoint questId=\"{0}\" stepId=\"{1}\" waypointNumber=\"{2}\" name=\"{3}\" statusText=\"\" /> \n",
                                ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId, obj.WaypointNumber, obj.Name);
                    }
                    else
                    {
                        tagText = string.Format("\n<UseWaypoint questId=\"{0}\" stepId=\"{1}\" waypointNumber=\"\" name=\"\" statusText=\"\" /> \n",
                            ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId);
                    }
                    Clipboard.SetText(tagText);
                    Logger.Raw(tagText);

                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static TabItem _tabItem;

        internal static void RemoveTab()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        Window mainWindow = Application.Current.MainWindow;
                        var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                        if (tabs == null)
                            return;
                        tabs.Items.Remove(_tabItem);

                    }
                )
            );
        }

        private static void btnWaitTimerTag_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }
            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return; ZetaDia.Actors.Update();

                    string tagText = string.Format("\n<WaitTimer questId=\"{0}\" stepId=\"{1}\" waitTime=\"500\" />\n", ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId);
                    Clipboard.SetText(tagText);
                    Logger.Raw(tagText);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }

        }

        private static void btnExploreAreaTag_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }

            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    string questInfo;
                    string questHeader;
                    GetQuestInfoText(out questInfo, out questHeader);

                    string tagText =
                        "\n" + questHeader +
                        "\n<ExploreDungeon " + questInfo + " until=\"ExitFound\" exitNameHash=\"0\" actorId=\"0\" pathPrecision=\"45\" boxSize=\"60\" boxTolerance=\"0.01\" objectDistance=\"45\">" +
                        "\n    <AlternateActors>" +
                        "\n        <AlternateActor actorId=\"0\" objectDistance=\"45\" />" +
                        "\n    </AlternateActors>" +
                        "\n    <AlternateMarkers>" +
                        "\n        <AlternateMarker markerNameHash=\"0\" markerDistance=\"45\" />" +
                        "\n    </AlternateMarkers>" +
                        "\n    <PriorityScenes>" +
                        "\n        <PriorityScene sceneName=\"Exit\" />" +
                        "\n    </PriorityScenes>" +
                        "\n    <IgnoreScenes>" +
                        "\n        <IgnoreScene sceneName=\"_N_\" />" +
                        "\n        <IgnoreScene sceneName=\"_S_\" />" +
                        "\n        <IgnoreScene sceneName=\"_E_\" />" +
                        "\n        <IgnoreScene sceneName=\"_W_\" />" +
                        "\n    </IgnoreScenes>" +
                        "\n</ExploreDungeon>";
                    Clipboard.SetText(tagText);
                    Logger.Raw(tagText);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static void GetQuestInfoText(out string questInfo, out string questHeader, DiaObject actor = null, Scene scene = null)
        {
            string levelAreaName = ZetaDia.SNO.LookupSNOName(SNOGroup.LevelArea, Player.LevelAreaId);
            string worldName = ZetaDia.WorldInfo.Name;
            string actorText = actor != null ? string.Format(" Actor: {0} ({1}) {2} ", actor.Name, actor.ActorSnoId, actor.ActorType) : string.Empty;
            string sceneText = scene != null ? string.Format(" Scene: {0} ({1}) ", scene.Name, scene.SceneInfo.SNOId) : string.Empty;

            if (ZetaDia.CurrentAct == Act.OpenWorld && ZetaDia.ActInfo.ActiveBounty != null)
            {
                questInfo = string.Format("questId=\"{0}\"", ZetaDia.ActInfo.ActiveBounty.Info.QuestSNO);
                questHeader = string.Format("\n<!-- Quest: {0} ({1}) World: {2} ({3}) LevelArea: {4} ({5}){6}{7} -->",
                    ZetaDia.ActInfo.ActiveBounty.Info.DisplayName + " " + ZetaDia.ActInfo.ActiveBounty.Info.Quest,
                    ZetaDia.ActInfo.ActiveBounty.Info.QuestSNO,
                    worldName,
                    ZetaDia.Globals.WorldSnoId,
                    levelAreaName,
                    ZetaDia.CurrentLevelAreaSnoId,
                    actorText,
                    sceneText);
            }
            else
            {
                questInfo = string.Format("questId=\"{0}\" stepId=\"{1}\"", ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId);
                questHeader = string.Format("\n<!-- Quest: {0} ({1}) World: {2} ({3}) LevelArea: {4} ({5}){6}{7} -->",
                   ZetaDia.CurrentQuest.Name,
                   ZetaDia.CurrentQuest.QuestSnoId,
                   worldName,
                   ZetaDia.Globals.WorldSnoId,
                   levelAreaName,
                   ZetaDia.CurrentLevelAreaSnoId,
                   actorText,
                   sceneText);
            }
        }

        private static void btnIfTagSceneIntersects_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }

            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;
                    ZetaDia.Actors.Update();
                    string levelAreaName = ZetaDia.SNO.LookupSNOName(SNOGroup.LevelArea, Player.LevelAreaId);
                    string worldName = ZetaDia.WorldInfo.Name;

                    string questInfo;
                    string questHeader;


                    var locationInfo = string.Format("x={0:0} y={1:0} z={2:0}",
                        ZetaDia.Me.Position.X, ZetaDia.Me.Position.Y, ZetaDia.Me.Position.Z);


                    GetQuestInfoText(out questInfo, out questHeader, null, ZetaDia.Me.CurrentScene);

                    var tagText = string.Format(questHeader + "\n<If condition=\"CurrentLevelAreaSnoId=={0} and SceneIntersects({1},{2:0},{3:0}) \">\n",
                            Player.LevelAreaId, ZetaDia.Me.CurrentScene.SceneInfo.SNOId, ZetaDia.Me.Position.X, ZetaDia.Me.Position.Y);

                    var logText = string.Format("<LogMessage questId=\"{0}\" output=\"Found Scene {1} ({2}) at {3}\" />\n",
                            ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.Me.CurrentScene.Name, ZetaDia.Me.CurrentScene.SceneInfo.SNOId, locationInfo);

                    Logger.Raw(tagText + Indent + logText + "\n</If>");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static void btnIfTag_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }

            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;
                    ZetaDia.Actors.Update();
                    string levelAreaName = ZetaDia.SNO.LookupSNOName(SNOGroup.LevelArea, Player.LevelAreaId);
                    string worldName = ZetaDia.WorldInfo.Name;

                    string tagText = "";
                    string questInfo;
                    string questHeader;

                    GetQuestInfoText(out questInfo, out questHeader);

                    if (ZetaDia.CurrentAct == Act.OpenWorld && ZetaDia.ActInfo.ActiveBounty != null)
                    {
                        tagText = string.Format(questHeader + "\n<If condition=\"HasQuest({5}) and CurrentWorldSnoId=={6} and CurrentLevelAreaSnoId=={7}\">\n\n</If>",
                            ZetaDia.ActInfo.ActiveBounty.Info.Quest, worldName, ZetaDia.Globals.WorldSnoId, levelAreaName, ZetaDia.CurrentLevelAreaSnoId, ZetaDia.ActInfo.ActiveBounty.Info.QuestSNO, ZetaDia.Globals.WorldSnoId, Player.LevelAreaId);
                    }
                    else if (ZetaDia.CurrentAct == Act.OpenWorld && ZetaDia.IsInTown)
                    {
                        if (ZetaDia.ActInfo.ActiveBounty != null)
                            tagText = string.Format(questHeader + "\n<If condition=\"HasQuest(0) and CurrentWorldSnoId=={6} and CurrentLevelAreaSnoId=={7}\">\n\n</If>",
                                ZetaDia.ActInfo.ActiveBounty.Info.Quest, worldName, ZetaDia.Globals.WorldSnoId, levelAreaName, ZetaDia.CurrentLevelAreaSnoId, ZetaDia.ActInfo.ActiveBounty.Info.QuestSNO, ZetaDia.Globals.WorldSnoId, Player.LevelAreaId);
                    }
                    else
                    {
                        tagText = string.Format(questHeader + "\n<If condition=\"IsActiveQuest({3}) and IsActiveQuestStep({4}) and CurrentWorldSnoId=={5} and CurrentLevelAreaSnoId=={6}\">\n\n</If>",
                            ZetaDia.CurrentQuest.Name, worldName, levelAreaName, ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId, ZetaDia.Globals.WorldSnoId, Player.LevelAreaId);

                    }
                    Logger.Raw(tagText);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static void btnDumpEnvironment_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
                BotMain.Stop();

            Thread.Sleep(500);

            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    var objects = ZetaDia.Actors.GetActorsOfType<DiaObject>()
                        .Where(o => o.IsEnvironmentRActor || o is DiaGizmo)
                        .OrderBy(o => o.Distance);

                    Logger.Raw("Environment found: {0}", objects.Count());

                    foreach (DiaObject o in objects)
                    {
                        if (o == null || !o.IsValid)
                            continue;

                        Logger.Raw("Environment {1} ({0}) {2} Distance: {4:0.0} ActorExistsAt({0},{5:0},{6:0},{7:0},50)",
                            o.ActorSnoId,
                            o.Name,
                            o.ActorType,
                            StringUtils.GetProfileCoordinates(o.Position),
                            o.Position.Distance(ZetaDia.Me.Position),
                            o.Position.X, o.Position.Y, o.Position.Z
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }


        private static void btnIfTagActorExists_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }

            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;
                    ZetaDia.Actors.Update();
                    string levelAreaName = ZetaDia.SNO.LookupSNOName(SNOGroup.LevelArea, Player.LevelAreaId);
                    string worldName = ZetaDia.WorldInfo.Name;
                    List<DiaObject> objList = (from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                                               where o.IsValid && o.CommonData != null && (o is DiaGizmo || o is DiaUnit) &&
                                                     !o.Name.StartsWith("Generic_Proxy") &&
                                                     !o.Name.StartsWith("Start_Location") &&
                                                     !o.Name.Contains("Trigger") &&
                                                     !(o is DiaPlayer) &&
                                                     (!(o is DiaUnit) || (o as DiaUnit).PetCreator == ZetaDia.Me.PetCreator) // Exclude pets;
                                               orderby o.Position.Distance(ZetaDia.Me.Position)
                                               select o).ToList();

                    string portalInfo = string.Empty;
                    string locationInfo = "";


                    if (objList.Any())
                    {
                        DiaObject obj = objList.FirstOrDefault();

                        if (obj == null)
                            return;

                        string questHeader;
                        string questInfo;
                        GetQuestInfoText(out questInfo, out questHeader, obj);

                        if (obj is GizmoPortal)
                            portalInfo = " isPortal=\"True\" destinationWorldId=\"-1\"";

                        if (!ZetaDia.WorldInfo.IsGenerated)
                        {
                            locationInfo = string.Format("x={0:0} y={1:0} z={2:0}",
                                obj.Position.X, obj.Position.Y, obj.Position.Z);
                        }

                        var tagText = string.Format(questHeader + "\n<If condition=\"ActorExistsAt({8},{9:0},{10:0},{11:0},50) and CurrentLevelAreaSnoId=={7}\">\n",
                            ZetaDia.ActInfo.ActiveBounty.Info.Quest, worldName, ZetaDia.Globals.WorldSnoId, levelAreaName, ZetaDia.CurrentLevelAreaSnoId, ZetaDia.ActInfo.ActiveBounty.Info.QuestSNO, ZetaDia.Globals.WorldSnoId, Player.LevelAreaId, obj.ActorSnoId, obj.Position.X, obj.Position.Y, obj.Position.Z);

                        var logText = string.Format("<LogMessage questId=\"{0}\" output=\"Actor Found {1} ({2}) at {3}\" />\n",
                            ZetaDia.CurrentQuest.QuestSnoId, obj.Name, obj.ActorSnoId, locationInfo);

                        Logger.Raw(tagText + Indent + logText + "\n</If>");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }

        }

        private static void btnMoveToMapMarker_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }

            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    MinimapMarker marker = ZetaDia.Minimap.Markers.CurrentWorldMarkers
                        .OrderBy(m => !m.IsPortalExit).ThenBy(m => m.Position.Distance2D(ZetaDia.Me.Position)).FirstOrDefault();

                    if (marker == null)
                        return;

                    DiaObject portal = (from o in ZetaDia.Actors.GetActorsOfType<GizmoPortal>(true)
                                        orderby o.Position.Distance(ZetaDia.Me.Position)
                                        select o).FirstOrDefault() ??
                                        (from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                                         orderby o.Position.Distance(marker.Position)
                                         select o).FirstOrDefault();

                    string questInfo;
                    string questHeader;
                    GetQuestInfoText(out questInfo, out questHeader);

                    string locationInfo = "";

                    if (!ZetaDia.WorldInfo.IsGenerated)
                    {
                        locationInfo = string.Format("x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\" ",
                            portal.Position.X, portal.Position.Y, portal.Position.Z);
                    }

                    if (portal != null)
                    {
                        string tagText = string.Format(questHeader + "\n<MoveToMapMarker " + questInfo + " " + locationInfo + "markerNameHash=\"{0}\" actorId=\"{1}\" interactRange=\"{2}\" \n" +
                                                       Indent3Hang + "pathPrecision=\"5\" pathPointLimit=\"250\" isPortal=\"True\" destinationWorldId=\"-1\" statusText=\"\" /> \n",
                            marker.NameHash, portal.ActorSnoId, portal.CollisionSphere.Radius);
                        Clipboard.SetText(tagText);
                        Logger.Raw(tagText);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static void btnMoveToActor_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }
            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    List<DiaObject> objList = (from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true)
                                               where o.IsValid && o.CommonData != null && (o is DiaGizmo || o is DiaUnit) &&
                                                     !o.Name.StartsWith("Generic_Proxy") &&
                                                     !o.Name.StartsWith("Start_Location") &&
                                                     !o.Name.Contains("Trigger") &&
                                                     !(o is DiaPlayer) &&
                                                     (!(o is DiaUnit) || ((o as DiaUnit).PetCreator != null && (o as DiaUnit).PetCreator == ZetaDia.Me.PetCreator)) // Exclude pets;
                                               orderby o.Position.Distance(ZetaDia.Me.Position)
                                               select o).ToList();

                    string portalInfo = string.Empty;
                    string questInfo = string.Empty;
                    string questHeader = string.Empty;


                    string locationInfo = "";

                    string tagText;
                    if (objList.Any())
                    {
                        DiaObject obj = objList.FirstOrDefault();

                        GetQuestInfoText(out questInfo, out questHeader, obj);

                        if (!ZetaDia.WorldInfo.IsGenerated)
                        {
                            locationInfo = string.Format(" x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\" ",
                                obj.Position.X, obj.Position.Y, obj.Position.Z);
                        }

                        if (obj is GizmoPortal)
                            portalInfo = " isPortal=\"True\" destinationWorldId=\"-1\"";


                        tagText = string.Format(questHeader + "\n<MoveToActor " + questInfo + locationInfo + " actorId=\"{0}\" interactRange=\"{1:0}\" name=\"{2}\" " + portalInfo + " pathPrecision=\"5\" pathPointLimit=\"250\" statusText=\"\" /> \n",
                            obj.ActorSnoId, obj.CollisionSphere.Radius + 1f, obj.Name);

                    }
                    else
                    {
                        tagText = string.Format(questHeader + "\n<MoveToActor " + questInfo + " x=\"\" y=\"\" z=\"\" actorId=\"\" interactRange=\"20\" pathPrecision=\"50\" pathPointLimit=\"250\" statusText=\"\" /> \n");
                    }
                    Clipboard.SetText(tagText);
                    Logger.Raw(tagText);

                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static void btnSafeMoveTo_Click(object sender, RoutedEventArgs e)
        {
            if (BotMain.IsRunning)
            {
                BotMain.Stop();
            }

            Thread.Sleep(500);
            try
            {
                using (new ZetaCacheHelper())
                {
                    if (!ZetaDia.IsInGame)
                        return;
                    if (ZetaDia.Me == null)
                        return;
                    if (!ZetaDia.Me.IsValid)
                        return;

                    ZetaDia.Actors.Update();

                    string tagText = string.Format("<SafeMoveTo questId=\"{3}\" stepId=\"{4}\" x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\" pathPrecision=\"5\" pathPointLimit=\"250\" scene=\"{5}\" statusText=\"\" />",
                        ZetaDia.Me.Position.X, ZetaDia.Me.Position.Y, ZetaDia.Me.Position.Z, ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId, ZetaDia.Me.CurrentScene.Name);
                    Clipboard.SetText(tagText);
                    Logger.Raw(tagText);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private static void btnResetGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BotMain.IsRunning)
                {
                    BotMain.Stop();
                }
                if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid)
                    return;

                Thread.Sleep(500);

                GridSegmentation.Reset();
                GridRoute.UpdateGridSegmentation();
                BrainBehavior.DungeonExplorer.Reset();
                //GridRoute.UpdateDungeonExplorer();
            }
            catch (Exception ex)
            {
                Logger.Raw("Could not reset grid: {0}", ex);
            }

        }

        private static void btnOpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            string logFile = "";
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                int myPid = Process.GetCurrentProcess().Id;
                DateTime startTime = Process.GetCurrentProcess().StartTime;
                if (exePath != null)
                    logFile = Path.Combine(exePath, "Logs", myPid + " " + startTime.ToString("yyyy-MM-dd HH.mm") + ".txt");

                if (File.Exists(logFile))
                    Process.Start(logFile);
                else
                {
                    Logger.Error("Unable to open log file {0} - file does not exist", logFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening log file: {0} {1}", logFile, ex.Message);
            }
        }

        static void _btnDumpCPlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ZetaDia.Memory.SaveCacheState())
                {
                    ZetaDia.Memory.DisableCache();

                    if (BotMain.IsRunning)
                    {
                        BotMain.Stop();
                        Thread.Sleep(1500);
                    }

                    DumpCPlayer();

                    foreach (var slot in Enum.GetValues(typeof(HotbarSlot)).Cast<HotbarSlot>())
                    {
                        DiaActiveSkill skill = ZetaDia.PlayerData.GetActiveSkillBySlot(slot);
                        Logger.Raw("{0} Active Skill: {1} RuneIndex: {2}", slot, skill.Power, skill.RuneIndex);
                    }
                    foreach (var power in ZetaDia.PlayerData.PassiveSkills)
                    {
                        Logger.Raw("Passive Skill: {0}", power);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        static void btnTest_Click(object sender, RoutedEventArgs e)
        {
            BotBehaviorQueue.Queue(new UseWaypointTag
            {
                QuestId = 1,
                WaypointNumber = 1
            });
        }

        private static void btnDumpActors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (ZetaDia.Memory.SaveCacheState())
                {
                    ZetaDia.Memory.DisableCache();

                    if (BotMain.IsRunning)
                    {
                        BotMain.Stop();
                        Thread.Sleep(1500);
                    }

                    double iType = -1;

                    ZetaDia.Actors.Update();
                    var units = ZetaDia.Actors.GetActorsOfType<DiaUnit>()
                        .Where(o => o.IsValid)
                        .OrderBy(o => o.Distance);

                    iType = DumpUnits(units, iType);


                    //ZetaDia.Actors.Update();
                    var objects = ZetaDia.Actors.GetActorsOfType<DiaObject>()
                        .Where(o => o.IsValid)
                        .OrderBy(o => o.Distance);

                    iType = DumpObjects(objects, iType);

                    //ZetaDia.Actors.Update();
                    var gizmos = ZetaDia.Actors.GetActorsOfType<DiaGizmo>(true)
                        .Where(o => o.IsValid)
                        .OrderBy(o => o.Distance);

                    iType = DumpGizmos(gizmos, iType);

                    var items = ZetaDia.Actors.GetActorsOfType<DiaItem>(true)
                        .Where(o => o.IsValid)
                        .OrderBy(o => o.Distance);

                    DumpItems(items, iType);

                    ZetaDia.Actors.Update();
                    var players = ZetaDia.Actors.GetActorsOfType<DiaPlayer>(true)
                        .Where(o => o.IsValid)
                        .OrderBy(o => o.Distance);

                    DumpPlayers(players);

                    //DumpService();

                    Logger.Raw("ZetaDia.Service.Hero.IsValid={0}", ZetaDia.Service.Hero.IsValid);

                    //DumpPlayerProperties();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }

        }

        private static void DumpPlayerProperties()
        {
            Logger.Log("DiaActivePlayer properties:");

            Type type = typeof(DiaActivePlayer);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
                {
                    Logger.Raw("\nName: {0} Value: {1} Type: {2}", prop.Name, prop.GetValue(ZetaDia.Me, null), prop.PropertyType.Name);
                }
            }
        }

        private static double DumpUnits(IEnumerable<DiaUnit> units, double iType)
        {
            Logger.Debug("Units found: {0}", units.Count());
            foreach (DiaUnit o in units)
            {
                if (!o.IsValid)
                    continue;

                string attributesFound = "";

                foreach (ActorAttributeType aType in Enum.GetValues(typeof(ActorAttributeType)))
                {
                    if (IgnoreActorAttributeTypes.Contains(aType))
                        continue;
                    iType = GetAttribute(iType, o, aType);
                    if (iType > 0)
                    {
                        attributesFound += aType.ToString() + "=" + iType + ", ";
                    }
                }

                string propertiesFound = ReadProperties(o, null);

                try
                {
                    Logger.Raw("\nUnit ActorSnoId: {0} Name: {1} Type: {2} Radius: {7:0.00} Position: {3} ({4}) Animation: {5} has Attributes:\n{6}\nProperties:\n{8}\n\n",
                                        o.ActorSnoId, o.Name, o.ActorInfo.GizmoType, StringUtils.GetProfileCoordinates(o.Position),
                                        StringUtils.GetSimplePosition(o.Position),
                                        o.CommonData.CurrentAnimation, attributesFound, o.CollisionSphere.Radius, propertiesFound);
                }
                catch { }

            }
            return iType;
        }

        private static string ReadProperties<T>(T obj, HashSet<Tuple<Type, string>> checkedTypes)
        {
            if (obj == null)
                return "";

            string propertiesFound = "";
            if (checkedTypes == null)
                checkedTypes = new HashSet<Tuple<Type, string>>();
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Tuple<Type, string> typeTuple = new Tuple<Type, string>(property.PropertyType, property.Name);

                if (property.PropertyType.IsValueType)
                {
                    try
                    {
                        string val = property.GetValue(obj, null).ToString().Replace("{", "<").Replace("}", ">");
                        propertiesFound += property.Name + ": " + val + " ";
                    }
                    catch (Exception ex)
                    {
                        propertiesFound += string.Format("\nError reading {0}: {1}", property.Name, ex.Message);
                    }
                }
                else if (property.PropertyType.IsClass && !checkedTypes.Contains(typeTuple))
                {
                    checkedTypes.Add(typeTuple);
                    try
                    {
                        if (!property.PropertyType.IsArray)
                        {
                            propertiesFound += "\n" + property.Name + ":" + ReadProperties(property.GetValue(obj, null), checkedTypes);
                        }
                    }
                    catch (Exception ex)
                    {
                        propertiesFound += string.Format("\nError reading {0}: {1}", property.Name, ex.Message);
                    }
                }
            }
            return propertiesFound;
        }


        private static double DumpObjects(IEnumerable<DiaObject> objects, double iType)
        {
            try
            {
                string locationInfo;

                Logger.Log("Objects found: {0}", objects.Count());
                foreach (DiaObject o in objects)
                {
                    if (!o.IsValid)
                        continue;

                    string attributesFound = "";

                    foreach (ActorAttributeType aType in Enum.GetValues(typeof(ActorAttributeType)))
                    {
                        if (IgnoreActorAttributeTypes.Contains(aType))
                            continue;
                        try
                        {
                            iType = GetAttribute(iType, o, aType);
                        }
                        catch
                        {
                        }

                        if (iType > 0)
                        {
                            attributesFound += aType.ToString() + "=" + iType + ", ";
                        }
                    }

                    if (!ZetaDia.WorldInfo.IsGenerated)
                    {
                        locationInfo = string.Format("x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\" ",
                            o.Position.X, o.Position.Y, o.Position.Z);
                    }

                    Vector3 myPos = ZetaDia.Me.Position;

                    Logger.Raw("\nObject ActorSnoId: {0} Name: {1} Type: {2} Radius: {3:0.00} Position: {4} ({5}) Animation: {6} Distance: {7:0.0} has Attributes: {8}\n\n",
                        o.ActorSnoId, o.Name, o.ActorType, o.CollisionSphere.Radius, StringUtils.GetProfileCoordinates(o.Position), StringUtils.GetSimplePosition(o.Position), o.CommonData.CurrentAnimation, o.Position.Distance(myPos),
                        attributesFound);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception {0}", ex.ToString());
            }
            return iType;
        }

        private static double DumpGizmos(IEnumerable<DiaGizmo> gizmos, double iType)
        {
            Logger.Log("Gizmos found: {0}", gizmos.Count());
            foreach (DiaGizmo o in gizmos)
            {
                if (!o.IsValid)
                    continue;

                if (o.ActorInfo.GizmoType == Zeta.Game.Internals.SNO.GizmoType.Trigger)
                    continue;

                if (o.ActorInfo.GizmoType == Zeta.Game.Internals.SNO.GizmoType.Checkpoint)
                    continue;

                string attributesFound = "";

                foreach (ActorAttributeType aType in Enum.GetValues(typeof(ActorAttributeType)))
                {
                    if (IgnoreActorAttributeTypes.Contains(aType))
                        continue;
                    iType = GetAttribute(iType, o, aType);
                    if (iType > 0)
                    {
                        attributesFound += aType.ToString() + "=" + iType + ", ";
                    }
                }

                if (o is GizmoBanner)
                {
                    var banner = (GizmoBanner)o;
                    attributesFound += "BannerIndex=" + banner.BannerPlayerIndex + ", ";
                    attributesFound += "BannerACDId=" + banner.BannerACDId + ", ";
                    attributesFound += "BannerCPlayerACDGuid=" + banner.BannerPlayer.ACDId + ", ";
                    attributesFound += "IsBannerUsable=" + banner.IsBannerUsable + ", ";
                    attributesFound += "IsBannerPlayerInCombat=" + banner.IsBannerPlayerInCombat + ", ";
                    attributesFound += "IsMyBanner=" + (banner.BannerPlayer.ACDId == ZetaDia.Me.ACDId).ToString() + ", ";
                }

                try
                {
                    Logger.Raw("\nGizmo ActorSnoId: {0} Name: {1} Type: {2} Radius: {3:0.00} Position: {4} ({5}) Distance: {6:0} Animation: {7} AppearanceSnoId: {8} has Attributes: {9}\n\n",
                        o.ActorSnoId, o.Name, o.ActorInfo.GizmoType, o.CollisionSphere.Radius, StringUtils.GetProfileCoordinates(o.Position), StringUtils.GetSimplePosition(o.Position), o.Distance, o.CommonData.CurrentAnimation, o.AppearanceSnoId, attributesFound);
                }
                catch { }
            }
            return iType;
        }

        private static void DumpItems(IEnumerable<DiaItem> items, double iType)
        {
            var diaItems = items as IList<DiaItem> ?? items.ToList();
            Logger.Log("Items found: {0}", diaItems.Count());
            foreach (DiaItem o in diaItems)
            {
                if (!o.IsValid)
                    continue;

                string attributesFound = "";

                foreach (ActorAttributeType aType in Enum.GetValues(typeof(ActorAttributeType)))
                {
                    if (IgnoreActorAttributeTypes.Contains(aType))
                        continue;
                    iType = GetAttribute(iType, o, aType);
                    if (iType > 0)
                    {
                        attributesFound += aType.ToString() + "=" + iType + ", ";
                    }
                }

                try
                {
                    Logger.Raw("\nItem ActorSnoId: {0} Name: {1} Type: {2} Radius: {3:0.00} Position: {4} ({5}) Distance: {6:0} Animation: {7} AppearanceSnoId: {8} has Attributes: {9}\n\n",
                        o.ActorSnoId, o.Name, o.ActorInfo.GizmoType, o.CollisionSphere.Radius, StringUtils.GetProfileCoordinates(o.Position), StringUtils.GetSimplePosition(o.Position), o.Distance, o.CommonData.CurrentAnimation, o.AppearanceSnoId, attributesFound);
                }
                catch { }
            }
        }

        private static void DumpPlayers(IEnumerable<DiaPlayer> players)
        {
            var diaPlayers = players as IList<DiaPlayer> ?? players.ToList();
            Logger.Log("Players found: {0}", diaPlayers.Count());
            HashSet<string> scannedAttributes = new HashSet<string>();
            foreach (DiaPlayer o in diaPlayers)
            {
                if (!o.IsValid)
                    continue;

                string attributesFound = "";
                const string propertiesFound = "";

                foreach (ActorAttributeType aType in Enum.GetValues(typeof(ActorAttributeType)))
                {
                    if (IgnoreActorAttributeTypes.Contains(aType))
                        continue;
                    if (scannedAttributes.Contains(aType.ToString()))
                        continue;

                    double aat = o.CommonData.GetAttribute<int>(aType);

                    if (aat == 0d || aat == -1d || aat == double.NaN || aat == double.MinValue || aat == double.MaxValue)
                        continue;

                    if (aat.ToString(CultureInfo.InvariantCulture) == "NaN")
                        continue;
                    scannedAttributes.Add(aType.ToString());

                    attributesFound += aType.ToString() + "=" + aat + ", ";
                }

                //propertiesFound = ReadProperties<DiaPlayer>(o, null);

                try
                {
                    Logger.Raw("\nPlayer ActorSnoId: {0} Name: {1} Type: {2} Radius: {3:0.00} Position: {4} ({5}) RActorGUID: {6} ACDId: {7} SummonerId: {8} " +
                        "SummonedByAcdId: {9} Animation: {10} isHidden: {11} ApperanceSnoId: {12} has Attributes: {13}\nProperties:\n{14}\n\n",
                    o.ActorSnoId, o.Name, o.ActorInfo.GizmoType, o.CollisionSphere.Radius, StringUtils.GetProfileCoordinates(o.Position), StringUtils.GetSimplePosition(o.Position),
                    o.RActorId, o.ACDId, o.SummonerId, o.SummonedByACDId,
                    o.CommonData.CurrentAnimation, o.IsHidden, o.ActorInfo.ApperanceSnoId, attributesFound, propertiesFound);
                }
                catch { }

            }
        }

        private static void DumpCPlayer()
        {
            string propertiesFound = ReadProperties(ZetaDia.PlayerData, null);
            try
            {
                Logger.Log("\n\nCPlayer: " + propertiesFound);
            }
            catch { }
        }

        private static void DumpService()
        {
            string propertiesFound = ReadProperties(ZetaDia.Service, null);

            try
            {
                Logger.Log("\n\nService: " + propertiesFound);
            }
            catch { }
        }

        private static double GetAttribute(double iType, DiaObject o, ActorAttributeType aType)
        {
            try
            {
                iType = (double)o.CommonData.GetAttribute<ActorAttributeType>(aType);
            }
            catch
            {
                iType = -1;
            }

            return iType;
        }
        private static double GetAttribute(DiaObject o, ActorAttributeType aType)
        {
            try
            {
                return o.CommonData.GetAttribute<double>(aType);
            }
            catch
            {
                return -1;
            }
        }
        private static readonly List<ActorAttributeType> IgnoreActorAttributeTypes = new List<ActorAttributeType>
        {
            /*
             * [QuestTools] Unit ActorSnoId: 5388 
             * Name: SkeletonSummoner_B-422 
             * Type: None 
             * Radius: 9.44 
             * Position: x="293" y="258" z="-10"  (293, 258, -10) 
             * Animation: SkeletonSummoner_attack_01 has 
             * Attributes: 
             * Level=60, 
             * TeamID=10, 
             * HitpointsCur=1201855092, 
             * HitpointsMax=1206500143, 
             * HitpointsMaxTotal=1206500143, 
             * EnchantRangeMax=255, 
             * SummonerID=2037973425, 
             * LastDamageACD=2018508947, 
             * ProjectileReflectDamageScalar=1065353216, 
             * BuffVisualEffect=1, 
             * ScreenAttackRadiusConstant=1114636288, 
             * TurnRateScalar=1065353216, 
             * TurnAccelScalar=1065353216, 
             * TurnDeccelScalar=1065353216, 
             * UnequippedTime=1, 
             * CoreAttributesFromItemBonusMultiplier=1065353216, 
             * IsTemporaryLure=1, 
             * PowerPrimaryResourceCostOverride=2139095039, 
             * PowerSecondaryResourceCostOverride=2139095039, 
             * PowerChannelCostOverride=2139095039, 
             */

            //ActorAttributeType.EnchantRangeMax, // Removed in D3 2.2.0
            ActorAttributeType.ScreenAttackRadiusConstant,
            ActorAttributeType.TurnRateScalar,
            ActorAttributeType.TurnAccelScalar,
            ActorAttributeType.TurnDeccelScalar,
            ActorAttributeType.UnequippedTime,
            ActorAttributeType.CoreAttributesFromItemBonusMultiplier,
            ActorAttributeType.IsTemporaryLure,
            ActorAttributeType.PowerPrimaryResourceCostOverride, 
            ActorAttributeType.PowerSecondaryResourceCostOverride,
            ActorAttributeType.PowerChannelCostOverride,

        };
    }
}
