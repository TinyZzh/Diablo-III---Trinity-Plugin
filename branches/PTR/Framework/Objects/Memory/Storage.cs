﻿using System;
using Trinity.Framework.Objects.Memory.Misc;
using Trinity.Framework.Objects.Memory;
using Zeta.Game;

namespace Trinity.Framework.Objects.Memory
{
    public class Storage : MemoryWrapper
    {
        public Storage(IntPtr ptr) : base(ptr) { }

        public Act CurrentAct => ReadOffset<Act>(0x02C);

        public int CurrentGameLevel => ReadOffset<int>(0x30);

        public GameDifficulty CurrentDifficulty => (GameDifficulty)ReadOffset<int>(0x04)+1;
        public int GameTick => ReadOffset<int>(0x120);
    }

}




