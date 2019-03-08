namespace AcadLib.CommandLock.UI
{
    using System;
    using System.Diagnostics;
    using System.Windows.Documents;
    using JetBrains.Annotations;

    /// <summary>
    /// Interaction logic for LockView.xaml
    /// </summary>
    public partial class LockView
    {
        public LockView([NotNull] LockViewModel vm) : base(vm)
        {
            DataContext = vm;
            InitializeComponent();
            try
            {
                ParseMessage(vm.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"CommandLock.LockView ParseMessage - {vm.Message}");
                tb.Text = vm.Message;
            }
        }

        private void Addlink([NotNull] string linkText)
        {
            try
            {
                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(linkText);
                hyperlink.NavigateUri = new Uri(linkText);
                hyperlink.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());
                tb.Inlines.Add(hyperlink);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"CommandLock Addlink - {linkText}");
                tb.Inlines.Add(linkText);
            }
        }

        private void ParseMessage(string message)
        {
            while (message.Length > 0)
            {
                var indexDot = message.IndexOf('#');
                if (indexDot == -1)
                {
                    tb.Inlines.Add(message);
                    return;
                }

                var msgBeforeDot = message.Substring(0, indexDot);
                tb.Inlines.Add(msgBeforeDot);
                var msgAfterDot = message.Substring(indexDot + 1);
                var indexSpace = msgAfterDot.IndexOf(' ');
                if (indexSpace == -1)
                {
                    Addlink(msgAfterDot);
                    return;
                }

                var linkText = msgAfterDot.Substring(0, indexSpace);
                Addlink(linkText);
                message = msgAfterDot.Substring(indexSpace + 1);
            }
        }
    }
}