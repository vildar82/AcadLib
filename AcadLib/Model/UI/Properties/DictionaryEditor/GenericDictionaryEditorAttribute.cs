// ReSharper disable once CheckNamespace
namespace AcadLib.UI.Designer
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Provides configuration options for the GenericDictionaryEditor.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class GenericDictionaryEditorAttribute : Attribute
    {
        public GenericDictionaryEditorAttribute()
        {
            KeyDisplayName = "Key";
            ValueDisplayName = "Value";
        }

        /// <summary>
        /// Specifies what type to use as an <see cref="AttributeProvider"/> for the keys in the dictionary.
        /// </summary>
        public Type KeyAttributeProviderType { get; set; }

        /// <summary>
        /// Specifies what type to use as a <see cref="System.ComponentModel.TypeConverter">converter</see> for the keys in the dictionary.
        /// </summary>
        public Type KeyConverterType { get; set; }

        /// <summary>
        /// Specifies what type to use as a <see cref="DefaultProvider{T}"/> for the keys in the dictionary.
        /// </summary>
        public Type KeyDefaultProviderType { get; set; }

        /// <summary>
        /// The display name for key property (defaults to "Key")
        /// </summary>
        public string KeyDisplayName { get; set; }

        /// <summary>
        /// Specifies what type to use as an <see cref="System.Drawing.Design.UITypeEditor">editor</see> to change the key of a dictionary entry.
        /// </summary>
        public Type KeyEditorType { get; set; }

        /// <summary>
        /// The title for the editor window.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Specifies what type to use as <see cref="AttributeProvider"/> for the values in the dictionary.
        /// </summary>
        public Type ValueAttributeProviderType { get; set; }

        /// <summary>
        /// Specifies what type to use as a <see cref="System.ComponentModel.TypeConverter">converter</see> for the values in the dictionary.
        /// </summary>
        public Type ValueConverterType { get; set; }

        /// <summary>
        /// Specifies what type to use as a <see cref="DefaultProvider{T}"/> for the values in the dictionary.
        /// </summary>
        public Type ValueDefaultProviderType { get; set; }

        /// <summary>
        /// The display name for value property (defaults to "Value")
        /// </summary>
        public string ValueDisplayName { get; set; }

        /// <summary>
        /// Specifies what type to use as an <see cref="System.Drawing.Design.UITypeEditor">editor</see> to change the value of a dictionary entry.
        /// </summary>
        public Type ValueEditorType { get; set; }
    }
}