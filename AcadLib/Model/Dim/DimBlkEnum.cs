namespace AcadLib.Dim
{
    public enum DimBlkEnum
    {
        /// <summary>
        /// Заполненная замкнутая
        /// </summary>
        FilledClosed,

        /// <summary>
        /// точка
        /// </summary>
        DOT,

        /// <summary>
        /// малая точка
        /// </summary>
        DOTSMALL,

        /// <summary>
        /// контурная точка
        /// </summary>
        DOTBLANK,

        /// <summary>
        /// указатель исходной точки
        /// </summary>
        ORIGIN,

        /// <summary>
        /// указатель исходной точки 2
        /// </summary>
        ORIGIN2,

        /// <summary>
        /// разомкнутая
        /// </summary>
        OPEN,

        /// <summary>
        /// прямой угол
        /// </summary>
        OPEN90,

        /// <summary>
        /// разомкнутая 30
        /// </summary>
        OPEN30,

        /// <summary>
        /// замкнутая
        /// </summary>
        CLOSED,

        /// <summary>
        /// контурная малая точка
        /// </summary>
        SMALL,

        /// <summary>
        /// ничего
        /// </summary>
        NONE,

        /// <summary>
        /// засечка
        /// </summary>
        OBLIQUE,

        /// <summary>
        /// заполненный прямоугольник
        /// </summary>
        BOXFILLED,

        /// <summary>
        /// прямоугольник
        /// </summary>
        BOXBLANK,

        /// <summary>
        /// контурная замкнутая
        /// </summary>
        CLOSEDBLANK,

        /// <summary>
        /// заполненный треугольник базы отсчета
        /// </summary>
        DATUMFILLED,

        /// <summary>
        /// треугольник базы отсчета
        /// </summary>
        DATUMBLANK,

        /// <summary>
        /// интеграл
        /// </summary>
        INTEGRAL,

        /// <summary>
        /// двойная засечка
        /// </summary>
        ARCHTICK
    }
}