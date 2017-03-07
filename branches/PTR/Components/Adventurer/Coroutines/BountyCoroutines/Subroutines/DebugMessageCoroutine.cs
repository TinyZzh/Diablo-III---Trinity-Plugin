using System.Threading.Tasks;
using Trinity.Components.Adventurer.Game.Quests;
using Logger = Trinity.Components.Adventurer.Util.Logger;

namespace Trinity.Components.Adventurer.Coroutines.BountyCoroutines.Subroutines
{
    public class DebugMessageCoroutine : IBountySubroutine
    {
        private string _message;
        private bool _isDone;

        public DebugMessageCoroutine(string message)
        {
            _message = message;
        }

        public bool IsDone => _isDone;

        public async Task<bool> GetCoroutine()
        {
            Logger.Warn(_message);
            _isDone = true;
            return true;
        }

        public void Reset()
        {
        }

        public void DisablePulse()
        {
        }

        public BountyData BountyData { get; }
    }
}