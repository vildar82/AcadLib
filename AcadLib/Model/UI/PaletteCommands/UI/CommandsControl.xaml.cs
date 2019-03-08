using System;

namespace AcadLib.PaletteCommands.UI
{
    using System.Windows;
    using JetBrains.Annotations;

    /// <summary>
    /// Логика взаимодействия для CommandsControl.xaml
    /// </summary>
    public partial class CommandsControl
    {
        public CommandsControl()
        {
            InitializeComponent();
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private static void Dispatcher_UnhandledException(
            object sender,
            [NotNull] System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is OperationCanceledException)
            {
                throw e.Exception;
            }

            if (e.Exception.HResult != -2146233079)
            {
                Logger.Log.Error("CommandsControl.Dispatcher_UnhandledException: " + e.Exception);
            }

            e.Handled = true;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is PaletteCommand selComm))
                return;
            selComm.Execute();
        }
    }
}