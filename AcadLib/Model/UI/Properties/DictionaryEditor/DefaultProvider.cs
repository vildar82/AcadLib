// ReSharper disable once CheckNamespace
namespace AcadLib.UI.Designer
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Used to specify whether the requested default value is to be used as Key or Value of a dictionary entry
    /// </summary>
    /// <remarks>If the default value is to be used as Key it may NOT be null (because the Dictionary doesn't allow null as Key)</remarks>
    public enum DefaultUsage
    {
        /// <summary>
        /// The requested default value is to be used as Key in the dictionary
        /// </summary>
        Key,

        /// <summary>
        /// The requested default value is to be used as Value in the dictionary
        /// </summary>
        Value
    }

    /// <summary>
    /// Provides default values for the Key or Value properties for new dictionary entries
    /// </summary>
    /// <typeparam name="T">The type of the Key or Value</typeparam>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DefaultProvider<T>
    {
        /// <summary>
        /// Returns a default value for the Key or Value properties for new dictionary entries
        /// </summary>
        /// <param name="usage">Specifies if the desired default value is to be used as Key or Value</param>
        /// <returns>Returns a value of type T to be used as the default</returns>
        /// <remarks>If the default value is to be used as Key it may NOT be null (because the Dictionary doesn't allow null as Key)</remarks>
        [CanBeNull]

        // ReSharper disable once UnusedParameter.Global
        public virtual T GetDefault(DefaultUsage usage)
        {
            var t = typeof(T);
            if (t.IsPrimitive || t.IsEnum)
                return default;
            if (t == typeof(string))
                return (T)(object)string.Empty;
            return Activator.CreateInstance<T>();
        }
    }
}