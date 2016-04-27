﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Framework.Objects.Memory.Containers;
using Trinity.Framework.Objects.Memory.Items;
using Trinity.Technicals;
using Zeta.Game;

namespace Trinity.Framework.Objects.Memory.Attributes
{
    public class AttributeManager : MemoryWrapper
    {
        public static Dictionary<int, AttributeDescripter> AttributeDescriptors;
        private static ExpandoContainer<AttributeGroup> _attributeGroups;

        static AttributeManager()
        {
            var descriptors = ReadObjects<AttributeDescripter>((IntPtr)0x01EEA578, 1435, 0x28).ToList();
            AttributeDescriptors = descriptors.ToDictionary(descripter => descripter.Id);
        }

        public static ExpandoContainer<AttributeGroup> AttributeGroups
        {
            get
            {
                if (!IsValid)
                {
                    _attributeGroups = Create<ExpandoContainer<AttributeGroup>>(ZetaDia.FastAttribGroups.BaseAddress);
                }
                if (!IsValid)
                {
                    Logger.LogError("Failed to find AttributeGroupManager");
                    return null;
                }
                return _attributeGroups;
            }
        }

        private static bool IsValid => _attributeGroups != null && !_attributeGroups.IsDisposed && _attributeGroups.Count > 0 && _attributeGroups.Bits < 1000 && _attributeGroups.ItemSize > 0;

        public static AttributeGroup FindGroup(int groupId)
        {
            return AttributeGroups?[(short)groupId];
        }
    }
}


