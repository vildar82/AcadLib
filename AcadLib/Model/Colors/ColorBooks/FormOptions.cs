// ReSharper disable once CheckNamespace
namespace AcadLib.Colors
{
    using System.Windows.Forms;
    using JetBrains.Annotations;

    [PublicAPI]
    public partial class FormOptions : Form
    {
        public FormOptions(Options options)
        {
            InitializeComponent();

            Options = options;
            propertyGrid1.SelectedObject = options;
        }

        public Options Options { get; set; }
    }
}