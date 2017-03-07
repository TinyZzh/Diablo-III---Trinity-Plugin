using System;
using System.Collections.Generic;
using Org.BouncyCastle.Pkix;
using QuestTools.Helpers;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Dungeons;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;

namespace QuestTools.Navigation
{
    public class QTNavigator // : INavigationProvider
    {
        private DateTime _lastGeneratedRoute = DateTime.MinValue;

        public QTNavigator()
        {
            PathPrecision = 10f;
        }

        public bool Clear()
        {
            Navigator.Clear();
            return true;
        }

        //public MoveResult MoveTo(Vector3 destination, string destinationName = null, bool useRaycast = true)
        //{
        //    return MoveTo(destination, destinationName, useRaycast, true);
        //}

        private List<int> _forcePathFindingLevelAreaIds = new List<int>
        {
            19953,
            62752,
        };

        private List<int> _forceNavigatorWorldSnOs = new List<int>
        {
            71150,
            70885,
            95804,
            109513,
            75049,
            50585,
        };

        public MoveResult MoveTo(Vector3 destination, string destinationName = null, bool useRaycast = true, bool useNavigator = false)
        {
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.Me.IsDead || ZetaDia.Globals.IsLoadingWorld || !ZetaDia.Service.IsValid || !ZetaDia.WorldInfo.IsValid)
            {
                return MoveResult.Failed;
            }

            try
            {
                return NavExtensions.NavigateTo(destination, destinationName);
            }
            catch (Exception ex)
            {
                Logger.Log("{0}", ex);
                GridSegmentation.Reset();

                return MoveResult.Failed;
            }
        }

        public float PathPrecision { get; set; }
    }
}
