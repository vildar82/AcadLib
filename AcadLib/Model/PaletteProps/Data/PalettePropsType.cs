namespace AcadLib.PaletteProps
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Autodesk.AutoCAD.DatabaseServices;

    public class PalettePropsType
    {
        /// <summary>
        /// Название типа объектов
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Примитивы чертежа
        /// </summary>
        public int Count { get; set; }

        public List<PalettePropsGroup> Groups { get; set; }

        public ICommand SelectType { get; set; }
    }
}
