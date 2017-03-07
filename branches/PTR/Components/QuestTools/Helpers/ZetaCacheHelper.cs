using System;
using System.Windows.Controls;
using Zeta.Game;

namespace QuestTools
{
    public class ZetaCacheHelper : IDisposable
    {
        private GreyMagic.ExternalReadCache externalReadCache;
        //private GreyMagic.FrameLock frameLock;
        public ZetaCacheHelper()
        {
            //frameLock = ZetaDia.Memory.AcquireFrame();
            ZetaDia.Actors.Update();
            externalReadCache = ZetaDia.Memory.SaveCacheState();
            ZetaDia.Memory.TemporaryCacheState(false);
        }

        ~ZetaCacheHelper()
        {
            Dispose();
        }
        public void Dispose()
        {
            //if (frameLock != null)
            //    frameLock.Dispose();

            if (externalReadCache != null)
                externalReadCache.Dispose();
        }
    }
}
