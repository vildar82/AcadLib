namespace AcadLib.Errors
{
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Autodesk.AutoCAD.ApplicationServices;
    using JetBrains.Annotations;
    using NetLib;
    using Visual;

    /// <summary>
    /// Логика взаимодействия для WindowErrors.xaml
    /// </summary>
    public partial class ErrorsView
    {
        private readonly Document doc;

        public ErrorsView([NotNull] ErrorsVM errVM)
            : base(errVM)
        {
            doc = AcadHelper.Doc;
            InitializeComponent();
            DataContext = errVM;
            KeyDown += ErrorsView_KeyDown;
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            var subject = $"Обращение по работе команды {CommandStart.CurrentCommand}";
            Process.Start($"mailto:khisyametdinovvt@pik.ru?subject={subject}");
        }

        private void ErrorsView_KeyDown(object sender, [NotNull] KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;

                case Key.Delete:
                    var model = (ErrorsVM)DataContext;
                    model.DeleteSelectedErrors();
                    break;
            }
        }

        private void HeaderTemplateStretchHack(object sender, RoutedEventArgs e)
        {
            ((ContentPresenter)((Grid)sender).TemplatedParent).HorizontalAlignment = HorizontalAlignment.Stretch;
        }
    }
}
