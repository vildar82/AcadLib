// ReSharper disable once CheckNamespace
namespace AcadLib.UI
{
    using System;
    using System.Windows.Forms;

    public partial class FormProperties : Form
    {
        public FormProperties()
        {
            InitializeComponent();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            propertyGrid1.ResetSelectedProperty();
        }
    }
}