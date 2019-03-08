namespace AcadLib.Utils
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using JetBrains.Annotations;

    /// <summary>
    /// Список загруженных модулей
    /// </summary>
    public static class ListModules
    {
        public static void List()
        {
            var modules = AppDomain.CurrentDomain.GetAssemblies().Select(s => new
            {
                Name = $"{s.FullName}_{GetLocation(s)}",
                References = s.GetReferencedAssemblies().Select(r => r.FullName).ToList()
            }).OrderBy(o => o.Name).ToList();
            var sb = new StringBuilder($"Список модулей {Environment.UserName}_{Environment.MachineName}_{DateTime.Now}");
            sb.AppendLine();
            var i = 1;
            foreach (var module in modules)
            {
                sb.AppendLine($"{i++}. {module.Name}");
                foreach (var reference in module.References)
                {
                    sb.AppendLine($"\t\t{reference}");
                }
            }

            var file = PromptSaveFile();
            File.WriteAllText(file, sb.ToString());
            Process.Start(file);
        }

        [NotNull]
        private static string GetLocation(Assembly assembly)
        {
            try
            {
                return assembly.Location;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string PromptSaveFile()
        {
            var dlg = new SaveFileDialog
            {
                FileName = "AcadModules.txt",
                Title = @"Сохранение списка модулей",
                AddExtension = true,
                DefaultExt = "txt",
                Filter = @"Текстовый файл (*.txt)|*.txt"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
            }

            throw new OperationCanceledException();
        }
    }
}