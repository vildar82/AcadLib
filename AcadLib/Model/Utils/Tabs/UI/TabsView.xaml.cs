namespace AcadLib.Utils.Tabs.UI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using NetLib.WPF.Data;

    /// <summary>
    /// Interaction logic for TabsView.xaml
    /// </summary>
    public partial class TabsView
    {
        public TabsView(TabsVM vm)
            : base(vm)
        {
            InitializeComponent();
        }

        private void Row_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (sender is DataGridRow r)
            //{
            //    e.Handled = true;
            //    var tab = (TabVM)r.DataContext;
            //    tab.Restore = !tab.Restore;
            //}
        }

        private void Row_Select(object sender, RoutedEventArgs e)
        {
            //if (sender is DataGridRow r)
            //{
            //    e.Handled = true;
            //    var tab = (TabVM)r.DataContext;
            //    tab.Restore = !tab.Restore;
            //}
        }

        private void Row_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow r)
            {
                var tab = (TabVM)r.DataContext;
                ((TabsVM)DataContext).OpenFileExec(tab);
            }
        }

        private void EventSetter_OnHandler(object sender, ToolTipEventArgs e)
        {
            if ((sender as DataGridRow)?.DataContext is TabVM tabVM && tabVM.Err == null && tabVM.Image == null)
            {
                try
                {
                    tabVM.Image = NetLib.IO.Path.GetThumbnail(tabVM.File).ConvertToBitmapImage();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            SelectAllTabs(sender as CheckBox, true);
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectAllTabs(sender as CheckBox, false);
        }

        private void SelectAllTabs(CheckBox cb, bool isChecked)
        {
            var parent = (DependencyObject)cb;
            do
            {
                parent = VisualTreeHelper.GetParent(parent);
            } while (!(parent is DataGrid));

            var dg = (DataGrid)parent;
            var session = (SessionVM)dg.DataContext;
            session.Tabs.ForEach(s => s.Restore = isChecked);
        }
    }
}
