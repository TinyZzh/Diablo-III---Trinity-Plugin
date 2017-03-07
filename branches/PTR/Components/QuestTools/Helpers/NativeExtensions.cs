using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.SNO;

namespace QuestTools.Helpers
{
    public static class NativeExtensions
    {
        static readonly Dictionary<int, NativeSceneSNO> CachedNatives = new Dictionary<int, NativeSceneSNO>();

        static NativeExtensions()
        {
            GameEvents.OnGameChanged += (sender, args) => CachedNatives.Clear();
        }

        public static NativeSceneSNO GetNativeObject(this Scene scene)
        {
            if (CachedNatives.ContainsKey(scene.SceneId))
            {
                return CachedNatives[scene.SceneId];
            }

            var native = ZetaDia.Memory.Read<NativeSceneSNO>(scene.Mesh.Zone.SceneRecord);

            CachedNatives.Add(scene.SceneId, native);

            return native;
        }
    }
}
