namespace AcadLib.UI.Ribbon.Elements
{
    using System.Windows.Input;
    using NetLib.WPF.Data;

    public class RibbonToggle : RibbonCommand
    {
        public bool IsChecked { get; set; }

        public override ICommand GetCommand()
        {
            return new RelayCommand(() =>
            {
                Execute();
                RibbonBuilder.ChangeToggleState(Command);
            });
        }
    }
}
