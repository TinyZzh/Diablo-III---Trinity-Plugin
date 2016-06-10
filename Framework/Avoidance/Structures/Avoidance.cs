﻿using System;
using System.Collections.Generic;
using Trinity.Objects;
using Zeta.Common;

namespace Trinity.Framework.Avoidance.Structures
{
    public class Avoidance
    {
        public DateTime CreationTime;        
        public AvoidanceData Data;
        public List<TrinityCacheObject> Actors = new List<TrinityCacheObject>();
        public Vector3 StartPosition;
        public bool IsImmune;
    }
}
