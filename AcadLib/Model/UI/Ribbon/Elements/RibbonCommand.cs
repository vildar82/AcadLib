namespace AcadLib.UI.Ribbon.Elements
{
    using System.Windows.Input;
    using NetLib.WPF.Data;

    public class RibbonCommand : RibbonItemData
    {
        public string Command { get; set; }

        public void Execute()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            using (doc.LockDocument())
            {
                doc.SendStringToExecute(Command + " ", true, false, true);
            }
        }

        public override ICommand GetCommand()
        {
            return new RelayCommand(Execute);
        }
    }
}
