// ReSharper disable once CheckNamespace
namespace AcadLib.UI.Designer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using JetBrains.Annotations;

    [PublicAPI]
    internal class EditableKeyValuePair<TKey, TValue> : CustomTypeDescriptor
    {
        public EditableKeyValuePair(TKey key, TValue value, [NotNull] GenericDictionaryEditorAttribute editorAttribute)
        {
            Key = key;
            Value = value;
            EditorAttribute = editorAttribute ?? throw new ArgumentNullException(nameof(editorAttribute));
        }

        public GenericDictionaryEditorAttribute EditorAttribute { get; set; }

        public TKey Key { get; set; }

        public TValue Value { get; set; }

        [NotNull]
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        [NotNull]
        public override PropertyDescriptorCollection GetProperties()
        {
            var properties = new List<PropertyDescriptor>();

            var KeyDescriptor = new KeyValueDescriptor(TypeDescriptor.CreateProperty(GetType(), "Key", typeof(TKey)),
                EditorAttribute.KeyConverterType, EditorAttribute.KeyEditorType, EditorAttribute.KeyAttributeProviderType,
                EditorAttribute.KeyDisplayName);
            properties.Add(KeyDescriptor);

            var ValueDescriptor = new KeyValueDescriptor(TypeDescriptor.CreateProperty(GetType(), "Value", typeof(TValue)),
                EditorAttribute.ValueConverterType, EditorAttribute.ValueEditorType, EditorAttribute.ValueAttributeProviderType,
                EditorAttribute.ValueDisplayName);
            properties.Add(ValueDescriptor);

            return new PropertyDescriptorCollection(properties.ToArray());
        }

        [NotNull]
        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}