// ReSharper disable once CheckNamespace
namespace AcadLib.UI.Designer
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using JetBrains.Annotations;

    internal class KeyValueDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _pd;

        private readonly Type m_AttributeProviderType;
        private readonly Type m_ConverterType;
        private readonly Type m_EditorType;

        public KeyValueDescriptor(
            [NotNull] PropertyDescriptor pd,
            Type converterType,
            Type editorType,
            Type attributeProviderType,
            string displayName)
            : base(pd)
        {
            _pd = pd;

            m_ConverterType = converterType;
            m_EditorType = editorType;
            m_AttributeProviderType = attributeProviderType;
            DisplayName = displayName;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                if (m_AttributeProviderType != null)
                {
                    return (Activator.CreateInstance(m_AttributeProviderType) as AttributeProvider)?.GetAttributes(
                               PropertyType) ?? throw new InvalidOperationException();
                }

                return TypeDescriptor.GetAttributes(PropertyType);
            }
        }

        public override Type ComponentType => _pd.ComponentType;

        public override TypeConverter Converter
        {
            get
            {
                if (m_ConverterType != null)
                    return Activator.CreateInstance(m_ConverterType) as TypeConverter;
                return TypeDescriptor.GetConverter(PropertyType);
            }
        }

        public override string DisplayName { get; }

        public override bool IsReadOnly => _pd.IsReadOnly;

        public override Type PropertyType => _pd.PropertyType;

        public override bool CanResetValue(object component)
        {
            return _pd.CanResetValue(component);
        }

        public override object GetEditor(Type editorBaseType)
        {
            if (m_EditorType != null)
                return Activator.CreateInstance(m_EditorType) as UITypeEditor;
            return TypeDescriptor.GetEditor(PropertyType, typeof(UITypeEditor));
        }

        public override object GetValue(object component)
        {
            return _pd.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            _pd.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            _pd.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _pd.ShouldSerializeValue(component);
        }
    }
}