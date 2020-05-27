namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Свойства динамического блока
    /// </summary>
    [PublicAPI]
    public class Property : IEquatable<Property>, ICloneable
    {
        public Property()
        {
        }

        public Property(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public Property(string name, object value, ObjectId idAtrRef)
        {
            Name = name;
            Value = value;
            IdAtrRef = idAtrRef;
            Type = PropertyType.Attribute;
            IsShow = true;
        }

        public Property(string name, object value, PropertyType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public Property([NotNull] DynamicBlockReferenceProperty dynProp)
        {
            Name = dynProp.PropertyName;
            Value = dynProp.Value;
            Type = PropertyType.Dynamic;
            IsShow = dynProp.Show;
            IsReadOnly = dynProp.ReadOnly;
        }

        public short DynPropTypeCode { get; set; }

        /// <summary>
        /// Только, если тип параматера - атрибут!
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public ObjectId IdAtrRef { get; set; }

        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Видит ли пользователь это свойство
        /// </summary>
        public bool IsShow { get; set; }

        public string Name { get; set; }

        public PropertyType Type { get; set; }

        public object Value { get; set; }
        public object[] AllowedValues { get; set; }

        /// <summary>
        /// Все видимые атрибуты и динамические свойства блока
        /// </summary>
        [NotNull]
        public static List<Property> GetAllProperties([NotNull] BlockReference blRef)
        {
            var props = new List<Property>();
            var attrs = AttributeInfo.GetAttrRefs(blRef);
            foreach (var atr in attrs)
            {
                var prop = new Property(atr.Tag, atr.Text.Trim(), atr.IdAtr);
                props.Add(prop);
            }

            props.AddRange(GetDynamicProperties(blRef));
            return props;
        }

        /// <summary>
        /// Динамические свойства блока
        /// </summary>
        [NotNull]
        public static List<Property> GetDynamicProperties([NotNull] BlockReference blRef)
        {
            var props = new List<Property>();
            if (blRef.DynamicBlockReferencePropertyCollection != null)
            {
                foreach (DynamicBlockReferenceProperty dyn in blRef.DynamicBlockReferencePropertyCollection)
                {
                    if (dyn.VisibleInCurrentVisibilityState)
                    {
                        if (dyn.PropertyName.Equals("Origin", StringComparison.OrdinalIgnoreCase))
                            continue;
                        var prop = new Property(dyn)
                        {
                            DynPropTypeCode = dyn.PropertyTypeCode,
                            AllowedValues = dyn.GetAllowedValues(),
                        };

                        props.Add(prop);
                    }
                }
            }

            return props;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(Property other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            var res = Name == other.Name && EqualValue(other.Value);

            return res;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Name.GetHashCode();
        }

        private bool EqualValue(object value)
        {
            if (Value is double d && value is double)
            {
                return Math.Abs(d - (double)value) < 0.0001;
            }

            return Value.Equals(value);
        }
    }
}
