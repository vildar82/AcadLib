namespace AcadLib.PaletteProps.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AcadLib.Editors;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using NetLib.WPF.Data;

    public class PalettePropsVM : BaseModel
    {
        public List<PalettePropsType> Types { get; set; }

        public PalettePropsType SelectedType { get; set; }

        public void Clear()
        {
            Types = null;
        }
    }
}
