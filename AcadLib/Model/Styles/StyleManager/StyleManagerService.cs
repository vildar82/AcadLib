namespace AcadLib.Styles.StyleManager
{
    using UI;

    public static class StyleManagerService
    {
        public static void ManageStyles()
        {
            var smVM = new StyleManagerVM();
            var smView = new StyleManagerView(smVM);
            smView.Show();
        }
    }
}