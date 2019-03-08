namespace AcadLib.Utils.Tabs.UI
{
    using System;
    using System.IO;
    using System.Windows.Media;
    using NetLib.WPF;

    public class TabVM : BaseModel
    {
        public TabVM(string drawing, bool restore)
        {
            File = drawing;
            Name = Path.GetFileNameWithoutExtension(drawing);
            if (Name?.Length > 52)
            {
                Name = $"{Name.Substring(0, 25)}..{Name.Substring(Name.Length - 25, 25)}";
            }

            Restore = restore;
            try
            {
                CheckFileExist();
                var fi = new FileInfo(drawing);
                DateLastWrite = System.IO.File.GetLastWriteTime(drawing);
                if (!fi.Exists)
                    Err = $"Файл '{fi.FullName}' не найден.";
                else
                    Size = fi.Length;
            }
            catch (Exception ex)
            {
                Err = ex.Message;
            }
        }

        private async void CheckFileExist()
        {
            var isFileExists = await NetLib.IO.Path.FileExistsAsync(File);
            if (!isFileExists)
            {
                Name += " (файл не найден)";
                Err = "Файл не найден";
            }
        }

        public string Name { get; set; }

        public string File { get; set; }

        public bool Restore { get; set; }

        public ImageSource Image { get; set; }

        public DateTime? DateLastWrite { get; set; }

        public long Size { get; set; }

        public string Err { get; set; }

        public DateTime Start { get; set; }
    }
}
