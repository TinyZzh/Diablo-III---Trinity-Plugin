﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;

namespace Trinity.UIComponents
{
    /// <summary>
    /// Rule for an item
    /// </summary>
    [DataContract(Namespace = "")]
    public class LRule : NotifyBase
    {
        public LRule()
        {

        }

        public LRule(ItemProperty prop)
        {
            Id = (int) prop;
            Value = GetDefaultValue(prop);
        }

        private double _value;
        private int _variant;
        private List<object> _variants = new List<object>();
        private RuleType _type;
        private RuleType _ruleType;
        private string _attributeValue;
        private string _attributeModifier;
        private string _attributeKey;

        public string Name => ItemProperty.ToString();

        [DataMember]
        public int Id { get; set; }

        public ItemProperty ItemProperty => (ItemProperty)Id;

        [DataMember]        
        public double Value
        {
            get
            {
                if (ItemStatRange != null)
                    _value = CoerceValue(_value);
                                 
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public static double GetDefaultValue(ItemProperty prop)
        {
            switch (prop)
            {
                case ItemProperty.Ancient:
                    return 1;
            }
            return 0;
        }

        private double CoerceValue(double value)
        {
            if (value < Min)
                value = Min;
            else if (value > Max)
                value = Max;
            return value;            
        }

        [DataMember(EmitDefaultValue = false)]
        public int Variant
        {
            get { return _variant; }
            set
            {
                if (_variant != value)
                {
                    _variant = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "AttKey")]
        public string AttributeKey
        {
            get { return _attributeKey; }
            set { SetField(ref _attributeKey, value); }
        }

        [DataMember(EmitDefaultValue = false, Name = "AttMod")]
        public string AttributeModifier
        {
            get { return _attributeModifier; }
            set { SetField(ref _attributeModifier, value); }
        }

        [DataMember(EmitDefaultValue = false, Name = "AttVal")]
        public string AttributeValue
        {
            get { return _attributeValue; }
            set { SetField(ref _attributeValue, value); }
        }

        public List<object> Variants
        {
            get
            {
                if (_variants == null || !_variants.Any())
                {
                    _variants = ItemDataUtils.GetItemPropertyVariants(ItemProperty, TrinityItemType);

                    if (_variant == 0)
                    {
                        var firstVariant = (_variants.FirstOrDefault() as IUnique);         
                        if(firstVariant!=null)
                            _variant = firstVariant.Id;
                    }
                        
                }
                return _variants;
            }
            set
            {
                if (_variants != value)
                {
                    _variants = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        public int TypeId { get; set; }
        public RuleType RuleType
        {
            get { return (RuleType)TypeId; }
            set { TypeId = (int)value; }
        }

        public double Min => ItemStatRange.AbsMin;

        public double Max => ItemStatRange.AbsMax;

        public double Step => ItemStatRange.AbsStep;

        public ItemStatRange ItemStatRange { get; set; }

        public TrinityItemType TrinityItemType { get; set; }

        public override int GetHashCode()
        {
            return TypeId.GetHashCode() * 17 ^ Id * 13 ^ Variant * 31 ^ (int)Value;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }
    }
}
