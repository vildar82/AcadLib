namespace AcadLib.UI.StatusBar.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using JetBrains.Annotations;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StatusBarMenu
    {
        [NotNull]
        private readonly Action<string> _selectValue;

        public StatusBarMenu(string value, [NotNull] List<string> values, [NotNull] Action<string> selectValue)
        {
            _selectValue = selectValue;
            Left = System.Windows.Forms.Cursor.Position.X;
            Top = System.Windows.Forms.Cursor.Position.Y;
            var menuItems = values
                .Select(s =>
                {
                    var mi = new Item
                    {
                        Text = s,
                        Visible = Equals(s, value) ? Visibility.Visible : Visibility.Hidden
                    };
                    return mi;
                }).ToList();
            MenuItems = menuItems;
            InitializeComponent();
            DataContext = this;
            Activated += (o, e) =>
            {
                Left -= ActualWidth;
                Top -= ActualHeight;
            };
            Deactivated += (o, e) => Close();
        }

        public List<Item> MenuItems { get; set; }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var sp = sender as StackPanel;
            var tb = sp.Children[1] as TextBlock;
            _selectValue(tb.Text);
            Hide();
        }
    }

    public class Item
    {
        public string Text { get; set; }

        public Visibility Visible { get; set; }
    }
}