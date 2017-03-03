using System;
using Trinity.Framework.Helpers;
using Zeta.Game.Internals.Actors;

namespace Trinity.Framework.Objects.Memory.Attributes
{
    public class AttributeResult<TModifier, TValue> where TModifier : IConvertible
    {
        public AttributeResult(AttributeItem item)
        {
            AttributeType = item.Key.BaseAttribute;
            ModifierType = item.Descripter.Value.ParameterType;
            Modifier = item.Key.ModifierId.To<TModifier>();
            Value = item.GetValue<TValue>();
        }

        public ActorAttributeType AttributeType;
        public Zeta.Game.Internals.AttributeParameterType ModifierType;
        public TValue Value;
        public TModifier Modifier;
    }
}


