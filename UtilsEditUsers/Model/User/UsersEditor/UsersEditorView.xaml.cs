namespace UtilsEditUsers.Model.User.UsersEditor
{
    using System.Linq;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for UsersEditorView.xaml
    /// </summary>
    public partial class UsersEditorView
    {
        public UsersEditorView(UsersEditorVM vm) : base(vm)
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = DgUsers.SelectedItems.Cast<EditAutocadUsers>().ToList();
            ((UsersEditorVM)Model).SelectedUsers = sel;
        }
    }
}