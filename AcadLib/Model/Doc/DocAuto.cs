namespace AcadLib.Doc
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices;
    using Lisp;

    /// <summary>
    /// Операции с документом
    /// </summary>
    public static class DocAuto
    {
        public static void Start()
        {
            DocSysVarAuto.Start();
            LispAutoloader.Start();

            // DbDictionaryCleaner - фаталит при открытии чертежей!!!
            // DbDictionaryCleaner.Start();
            Application.DocumentManager.DocumentCreated += (s, e) => OnDocumentCreated(e?.Document);
            foreach (Document doc in Application.DocumentManager) OnDocumentCreated(doc);
        }

        private static void OnDocumentCreated(Document doc)
        {
            if (doc == null) return;
            try
            {
                LispAutoloader.LoadLisp(doc);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "LispAutoloader.LoadLisp");
            }

            try
            {
                DocSysVarAuto.SetSysVars(doc);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "DocSysVarAuto.SetSysVars");
            }
        }
    }
}
