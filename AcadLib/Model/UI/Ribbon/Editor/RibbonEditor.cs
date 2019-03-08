namespace AcadLib.UI.Ribbon.Editor
{
    using Autodesk.AutoCAD.ApplicationServices.Core;

    /// <summary>
    /// Редактор ленты
    /// </summary>
    public class RibbonEditor
    {
        public void Edit()
        {
            if (!General.IsBimUser)
            {
                "Только для BIM".WriteToCommandLine();
                return;
            }

            var ribbonVm = new RibbonVM();
            var ribbonView = new RibbonView(ribbonVm);
            Application.ShowModelessWindow(ribbonView);
        }
    }
}
