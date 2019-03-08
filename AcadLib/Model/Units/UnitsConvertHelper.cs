namespace AcadLib.Units
{
    using System;
    using JetBrains.Annotations;
    using UnitsNet;

    /// <summary>
    /// Конвертер единиц
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public static class UnitsConvertHelper
    {
        /// <summary>
        /// Преобразование длины из мм в метры.
        /// </summary>
        public static double ConvertMmToMLength(double mm)
        {
            var len = Length.FromMillimeters(mm);
            return len.Meters;
        }
    }
}