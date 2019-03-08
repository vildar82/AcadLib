namespace UtilsEditUsers
{
    using System;
    using System.Windows;
    using Model.User;
    using NLog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static ILogger Log { get; } = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                UserSettingsService.UsersEditor();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}