// ReSharper disable once CheckNamespace
namespace AcadLib.XData.Viewer
{
    using System.Windows.Forms;

    public partial class FormXDataView : Form
    {
        public FormXDataView(string info, string entName)
        {
            InitializeComponent();
            label1.Text = entName;
            richTextBox1.Text = info;
        }
    }
}