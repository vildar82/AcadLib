namespace UtilsEditUsers.Model.User.UsersEditor
{
    using System;
    using System.Reactive.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using DB;
    using NetLib.WPF;
    using ReactiveUI;

    public class EditAutocadUsers : BaseModel
    {
        public EditAutocadUsers()
        {
        }

        public EditAutocadUsers(AutocadUsers dbUser)
        {
            DbUser = dbUser;
            Login = dbUser.Login;
            FIO = dbUser.FIO;
            Disabled = dbUser.Disabled;
            Description = dbUser.Description;
            Group = dbUser.Group;
            Version = dbUser.Version;
            PreviewUpdate = dbUser.PreviewUpdate ?? false;
            DateRun = dbUser.DateRun;
            AdditionalGroup = dbUser.AdditionalGroup;
        }

        public AutocadUsers DbUser { get; set; }

        public string Login { get; set; }

        public string FIO { get; set; }

        public string AdDepartment { get; set; }

        public string AdPosition { get; set; }

        public BitmapImage Image { get; set; }

        public string Group { get; set; }

        public string AdditionalGroup { get; set; }

        public string Version { get; set; }

        public Brush VersionColor { get; set; }

        public string VersionTooltip { get; set; }

        public bool Disabled { get; set; }

        public string Description { get; set; }

        public bool PreviewUpdate { get; set; }

        public DateTime? DateRun { get; set; }

        public void SaveToDbUser()
        {
            DbUser.Group = Group;
            DbUser.FIO = FIO;
            DbUser.Login = Login;
            DbUser.Description = Description;
            DbUser.Disabled = Disabled;
            DbUser.PreviewUpdate = PreviewUpdate;
            DbUser.DateRun = DateRun;
            if (AdditionalGroup == Group)
            {
                AdditionalGroup = null;
            }

            DbUser.AdditionalGroup = AdditionalGroup;
        }

        public override string ToString()
        {
            return $"{Login}, {FIO}, {Description}, {AdDepartment}, {AdPosition}";
        }
    }
}