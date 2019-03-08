namespace AcadLib.UI
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using JetBrains.Annotations;

    /// <summary>
    /// Диалог выбора файлов с возможностью включания выбора папки IsFolderDialog=true.
    /// Если включить IsFolderDialog, то отключается проверка CheckFileExists и CheckPathExists и dialog.FileName = " ";
    /// </summary>
    [PublicAPI]
    public class FileFolderDialog : CommonDialog
    {
        public OpenFileDialog Dialog { get; private set; } = new OpenFileDialog();

        public bool IsFolderDialog { get; set; }

        /// <summary>
        /// Helper property. Parses FilePath into either folder path (if Folder Selection. is set)
        /// or returns file path
        /// </summary>
        [NotNull]
        public string SelectedPath => (IsFolderDialog ? Path.GetDirectoryName(Dialog.FileName) : Dialog.FileName) ??
                                      throw new InvalidOperationException();

        public override void Reset()
        {
            Dialog.Reset();
        }

        public new DialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        public new DialogResult ShowDialog([CanBeNull] IWin32Window owner)
        {
            if (IsFolderDialog)
            {
                Dialog.FileName = "п";
                Dialog.Title += @" Для выбора текущей папки оставьте в поле имени файла 'п' и нажмите открыть.";
                Dialog.CheckFileExists = false;
                Dialog.CheckPathExists = false;
                Dialog.ValidateNames = false;
            }

            return owner == null ? Dialog.ShowDialog() : Dialog.ShowDialog(owner);
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            return true;
        }
    }
}