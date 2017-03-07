using System;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;

namespace QuestTools.Helpers
{
    public class ChangeMonitor
    {
        private static int _worldId;
        private static int _levelAreaId;
        private static int _questId;
        private static int _questStepId;
        private static Act _currentAct = Act.Invalid;
        private static bool _somethingChanged;

        private static DateTime _lastChangeCheckTime = DateTime.MinValue;
        public static void CheckForChanges()
        {
            if (!QuestToolsSettings.Instance.DebugEnabled)
                return;

            if (DateTime.UtcNow.Subtract(_lastChangeCheckTime).TotalMilliseconds < 1000)
            {
                return;
            }
            _lastChangeCheckTime = DateTime.UtcNow;

            if (!ZetaDia.IsInGame)
                return;

            if (!ZetaDia.Me.IsValid)
                return;

            if (ZetaDia.Globals.IsLoadingWorld)
                return;

            Act newAct = ZetaDia.CurrentAct;
            if (ZetaDia.ActInfo.IsValid && newAct != _currentAct)
            {
                Logger.Verbose("Act changed from {0} to {1} ({2}) SnoId={3}", _currentAct.ToString(), newAct, (int)newAct, ZetaDia.CurrentActSnoId);
                _currentAct = newAct;
                _somethingChanged = true;
            }

            int newWorldId = ZetaDia.Globals.WorldSnoId;
            if (ZetaDia.WorldInfo.IsValid && ZetaDia.Globals.WorldSnoId != _worldId)
            {
                string worldName = ZetaDia.WorldInfo.Name;
                Logger.Verbose("worldId changed from {0} to {1} - {2}", _worldId, newWorldId, worldName);
                _worldId = newWorldId;
                _somethingChanged = true;
            }

            if (ZetaDia.WorldInfo.IsValid && Player.LevelAreaId != _levelAreaId)
            {
                string levelAreaName = ZetaDia.SNO.LookupSNOName(SNOGroup.LevelArea, Player.LevelAreaId);
                Logger.Verbose("levelAreaId changed from {0} to {1} - {2}", _levelAreaId, Player.LevelAreaId, levelAreaName);
                _levelAreaId = Player.LevelAreaId;
                _somethingChanged = true;
            }

            if (ZetaDia.CurrentQuest != null && ZetaDia.CurrentQuest.IsValid)
            {
                int newSno = ZetaDia.CurrentQuest.QuestSnoId;
                if (newSno != _questId)
                {
                    Logger.Verbose("questId changed from {0} to {1} - {2}", _questId, newSno, ZetaDia.CurrentQuest.Name);
                    _questId = newSno;
                    _somethingChanged = true;
                }

                int newStep = ZetaDia.CurrentQuest.StepId;
                if (newStep != _questStepId)
                {
                    Logger.Verbose("questStepId changed from {0} to {1}", _questStepId, newStep);
                    _questStepId = newStep;
                    _somethingChanged = true;
                }
            }
            else if (ZetaDia.CurrentQuest == null)
            {
                Logger.Verbose("questId changed from {0} to NULL", _questId);
                _questId = -1;
                _somethingChanged = true;
            }

            if (!_somethingChanged || !ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld || ZetaDia.Me.Position == Vector3.Zero)
                return;

            Logger.Verbose("Change(s) occured at Position {0} ", StringUtils.GetProfileCoordinates(ZetaDia.Me.Position));
            _somethingChanged = false;
        }

        public static void Reset()
        {
            _currentAct = Act.Invalid;
            _levelAreaId = 0;
            _questId = 0;
            _questStepId = 0;
            _worldId = 0;
            _somethingChanged = true;
        }
    }
}
