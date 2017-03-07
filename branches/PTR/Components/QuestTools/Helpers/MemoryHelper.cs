using System;
using Zeta.Game;

namespace QuestTools.Helpers
{
    public class MemoryHelper : IDisposable
    {
        bool _disposed;
        private GreyMagic.FrameLockRelease _frameLockRelease;
        private GreyMagic.ExternalReadCache _externalReadCache;

        /// <summary>
        /// Enables reading DB data while bot is running or stopped
        /// </summary>
        public MemoryHelper()
        {
            _frameLockRelease = ZetaDia.Memory.ReleaseFrame(true);

            if (ZetaDia.Service.IsInGame)
            {
                ZetaDia.Actors.Update();
                _externalReadCache = ZetaDia.Memory.SaveCacheState();
                ZetaDia.Memory.TemporaryCacheState(false);
            }            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MemoryHelper()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    if (_frameLockRelease != null)
                        _frameLockRelease.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Debug("Exception disposing of MemoryHelper._frameLockRelease {0}", ex);
                }

                try
                {
                    if (_externalReadCache != null)
                        _externalReadCache.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Debug("Exception disposing of MemoryHelper._externalReadCache {0}", ex);
                }
            }

            _externalReadCache = null;
            _frameLockRelease = null;

            _disposed = true;
        }
    }
}
