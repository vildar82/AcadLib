namespace AcadLib.Doc
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices;

    /// <summary>
    /// Запуск команды при старте актокада (когда откроется первый документ)
    /// </summary>
    public class StartupCommand
    {
        private readonly string _command;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="command">SendStringToExecute</param>
        public StartupCommand(string command)
        {
            _command = command;
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                Execute(command, doc);
                return;
            }

            Application.DocumentManager.DocumentActivated += OnDocumentActivated;
        }

        private void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document == null)
                return;

            Application.DocumentManager.DocumentActivated -= OnDocumentActivated;
            Execute(_command, e.Document);
        }

        private void Execute(string command, Document doc)
        {
            try
            {
                doc.SendStringToExecute(command, true, true, true);
            }
            catch (Exception ex)
            {
                ex.LogError();
            }
        }
    }
}