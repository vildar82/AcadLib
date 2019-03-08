using System.Diagnostics;

namespace AcadLib.Plot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using AcadLib.Files;
    using AcadLib.Plot.UI;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.PlottingServices;
    using Autodesk.AutoCAD.Publishing;
    using JetBrains.Annotations;
    using Layouts;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    /// <summary>
    /// http://adndevblog.typepad.com/autocad/2012/05/how-to-use-the-autodeskautocadpublishingpublisherpublishdsd-api-in-net.html
    /// </summary>
    [PublicAPI]
    public class PlotDirToPdf
    {
        private static readonly NetLib.Comparers.AlphanumComparator alphaComparer = NetLib.Comparers.AlphanumComparator.New;
        private string dir;
        private string filePdfOutputName;
        private string[] filesDwg;
        private string title = "Печать";

        public PlotDirToPdf([NotNull] string dir, bool includeSubdirs, string filePdfOutputName = "")
        {
            filesDwg = Directory.GetFiles(dir, "*.dwg",
                includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            filesDwg = filesDwg.OrderBy(f => f, alphaComparer).ToArray();
            this.dir = dir;
            this.filePdfOutputName = filePdfOutputName == string.Empty ? Path.GetFileName(dir) : filePdfOutputName;
        }

        public PlotDirToPdf([NotNull] string dir, string filePdfOutputName = "")
            : this(dir, false, filePdfOutputName)
        {
        }

        public PlotDirToPdf([NotNull] string[] filesDwg, string filePdfOutputName)
        {
            dir = Path.GetDirectoryName(filesDwg.First());
            this.filesDwg = filesDwg;
            this.filePdfOutputName = filePdfOutputName;
        }

        public PlotOptions Options { get; set; }

        public static void PromptAndPlot([NotNull] Document doc)
        {
            var ed = doc.Editor;
            var plotOptData = FileDataExt.GetLocalFileData<PlotOptions>("PlotToPdf", "PlotOptions", false);
            plotOptData.TryLoad(() => new PlotOptions());
            var plotOpt = plotOptData.Data;
            var repeat = false;
            do
            {
                var optPrompt = new PromptKeywordOptions($"\nПечать листов в PDF из:");
                optPrompt.Keywords.Add("Текущего");
                optPrompt.Keywords.Add("Папки");
                optPrompt.Keywords.Add("Настройки");
                optPrompt.Keywords.Default = plotOpt.DefaultPlotCurOrFolder ? "Текущего" : "Папки";

                var resPrompt = ed.GetKeywords(optPrompt);
                if (resPrompt.Status == PromptStatus.OK)
                {
                    if (resPrompt.StringResult == "Текущего")
                    {
                        repeat = false;
                        Logger.Log.Info("Текущего");
                        if (!File.Exists(doc.Name))
                            throw new Exception("Нужно сохранить текущий чертеж.");

                        var filePdfName =
                            Path.Combine(Path.GetDirectoryName(doc.Name) ?? throw new InvalidOperationException(),
                                Path.GetFileNameWithoutExtension(doc.Name) + ".pdf");
                        var plotter = new PlotDirToPdf(new[] {doc.Name}, filePdfName) { Options = plotOpt };
                        plotter.Plot();
                    }
                    else if (resPrompt.StringResult == "Папки")
                    {
                        repeat = false;
                        Logger.Log.Info("Папки");
                        var dialog = new AcadLib.UI.FileFolderDialog
                        {
                            Dialog = { Multiselect = true},
                            IsFolderDialog = true
                        };
                        dialog.Dialog.Title = @"Выбор папки или файлов для печати чертежей в PDF.";
                        dialog.Dialog.Filter = @"Чертежи|*.dwg";
                        dialog.Dialog.InitialDirectory = Path.GetDirectoryName(doc.Name);

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            PlotDirToPdf plotter;
                            var firstFileNameWoExt = Path.GetFileNameWithoutExtension(dialog.Dialog.FileNames.First());
                            if (dialog.Dialog.FileNames.Length > 1)
                            {
                                plotter = new PlotDirToPdf(dialog.Dialog.FileNames,
                                    Path.GetFileName(dialog.SelectedPath));
                            }
                            else if (firstFileNameWoExt != null &&
                                     firstFileNameWoExt.Equals("п", StringComparison.OrdinalIgnoreCase))
                            {
                                plotter = new PlotDirToPdf(dialog.SelectedPath, plotOpt.IncludeSubdirs);
                            }
                            else
                            {
                                plotter = new PlotDirToPdf(dialog.Dialog.FileNames, firstFileNameWoExt);
                            }

                            plotter.Options = plotOpt;
                            plotter.Plot();
                        }
                    }
                    else if (resPrompt.StringResult == "Настройки")
                    {
                        // Сортировка; Все файлы в один пдф или для каждого файла отдельная пдф
                        var plotOptVm = new PlotOptionsVM(plotOpt);
                        var plotOptView = new PlotOptionsView(plotOptVm);
                        if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(plotOptView) == true)
                            plotOptData.TrySave();
                        repeat = true;
                    }
                }
                else
                {
                    ed.WriteMessage("\nОтменено пользователем.");
                    return;
                }
            }
            while (repeat);
        }

        [NotNull]
        public List<int> GetFilterNumbers(int countTabs, string filter)
        {
            var resNums = new List<int>();
            if (Options.FilterState && !string.IsNullOrWhiteSpace(filter))
            {
                var clearStr = string.Empty;
                filter = filter.Trim().Replace(" ", string.Empty);
                var negativeNumbersMatchs = Regex.Matches(filter, @"(^-\d+)|[,-](-\d+)");
                var startIndex = 0;
                foreach (Match negMatch in negativeNumbersMatchs)
                {
                    // замена негативного числа на соответствующее
                    var g = negMatch.Groups[1];
                    if (!g.Success)
                        g = negMatch.Groups[2];
                    var negNum = int.Parse(g.Value.Substring(1));
                    var index = countTabs - negNum + 1;
                    clearStr += filter.Substring(0, g.Index).Substring(startIndex) + index;
                    startIndex = g.Index + g.Length;
                }

                if (startIndex != 0)
                {
                    filter = clearStr + filter.Substring(startIndex);
                }

                var query =
                    from x in filter.Split(',')
                    let y = x.Split('-')
                    let b = int.Parse(y[0].Trim())
                    let e = int.Parse(y[y.Length - 1].Trim())
                    from n in Enumerable.Range(e > b ? b : e, (e > b ? e - b : b - e) + 1)
                    select n;
                resNums = query.ToList();
            }

            return resNums;
        }

        public void Plot()
        {
            if (Options == null)
            {
                Options = new PlotOptions();
                PlotFiles();
            }
            else
            {
                if (!Options.OnePdfOrEachDwg)
                {
                    foreach (var file in filesDwg)
                    {
                        PlotFileToPdf(file);
                    }
                }
                else
                {
                    PlotFiles();
                }
            }

            // открыть проводник с файлом
            System.Diagnostics.Process.Start("explorer", dir);
        }

        public void PublisherDSD(DsdEntryCollection collection)
        {
            try
            {
                var dsdFile = Path.Combine(dir, filePdfOutputName + ".dsd");
                if (!filePdfOutputName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    filePdfOutputName += ".pdf";
                var destFile = Path.Combine(dir, filePdfOutputName);
                CheckFileAccess(destFile);
                Application.SetSystemVariable("BACKGROUNDPLOT", 0);
                using (var dsd = new DsdData())
                {
                    if (PikSettings.UserGroup == "ДО")
                    {
                        dsd.PlotStampOn = true;
                        dsd.ProjectPath = dir;
                    }

                    dsd.SetDsdEntryCollection(collection);
                    dsd.LogFilePath = Path.Combine(dir, "logPlotPdf.log");
                    dsd.SheetType = SheetType.MultiPdf;
                    dsd.IsSheetSet = true;
                    dsd.NoOfCopies = 1;
                    dsd.IsHomogeneous = false;
                    dsd.DestinationName = destFile;
                    dsd.SheetSetName = "PublisherSet";
                    dsd.PromptForDwfName = false;
                    PostProcessDSD(dsd, dsdFile);
                }

                var nbSheets = collection.Count;
                using (var progressDlg = new PlotProgressDialog(false, nbSheets, true))
                {
                    progressDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, title);
                    progressDlg.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Отмена задания");
                    progressDlg.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Отмена листа");
                    progressDlg.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, title);
                    progressDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Печать листа");
                    progressDlg.UpperPlotProgressRange = 100;
                    progressDlg.LowerPlotProgressRange = 0;
                    progressDlg.UpperSheetProgressRange = 100;
                    progressDlg.LowerSheetProgressRange = 0;
                    progressDlg.IsVisible = true;
                    var publisher = Application.Publisher;
                    PlotConfigManager.SetCurrentConfig("clk-PDF.pc3");
                    publisher.PublishDsd(dsdFile, progressDlg);
                    progressDlg.Destroy();
                }

                NetLib.IO.Path.TryDeleteFile(dsdFile);

                // Добавить бланк
                if (Options.BlankOn)
                {
                    var tempPdf = NetLib.IO.Path.GetTempFile(".pdf");
                    try
                    {
                        var blank = IO.Path.GetLocalSettingsFile(@"Support\blank.pdf");
                        new PdfEditor().AddSheet(filePdfOutputName, blank, 1, Options.BlankPageNumber, tempPdf);
                        while (true)
                        {
                            try
                            {
                                File.Copy(tempPdf, filePdfOutputName, true);
                                Process.Start(filePdfOutputName);
                                break;
                            }
                            catch
                            {
                                var res = MessageBox.Show($"Закройте pdf файл - {filePdfOutputName}",
                                    "Ошибка добавление бланка", MessageBoxButtons.OKCancel);
                                if (res != DialogResult.OK)
                                    break;
                            }
                        }
                    }
                    finally
                    {
                        NetLib.IO.Path.TryDeleteFile(tempPdf);
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckFileAccess([NotNull] string destFile)
        {
            var fi = new FileInfo(destFile);
            var countWhile = 0;
            do
            {
                try
                {
                    using (fi.OpenWrite())
                        return;
                }
                catch (Exception ex)
                {
                    var dlgRes = MessageBox.Show($"{ex.Message}\n\rУстраните причину и нажмите продолжить.",
                        "Печать", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
                    if (dlgRes == DialogResult.Cancel)
                        throw new OperationCanceledException();
                }

                countWhile++;
            }
            while (countWhile < 3);
            throw new Exception("Превышено число попыток доступа к файлу. Выход.");
        }

        private bool FilterByName([NotNull] string tabName)
        {
            return Regex.IsMatch(tabName, Options.FilterByNames, RegexOptions.IgnoreCase);
        }

        [NotNull]
        private List<Layout> FilterLayouts([NotNull] List<Layout> layouts, [NotNull] PlotOptions options)
        {
            var resLayouts = new List<Layout>();
            var filterNums = GetFilterNumbers(layouts.Count, options.FilterByNumbers);
            foreach (var layout in layouts)
            {
                // Номер вкладки
                var tabIndex = layout.TabOrder;
                var tabName = layout.LayoutName;
                bool? filtering = null;

                // Фильтр по именам
                if (!string.IsNullOrWhiteSpace(Options.FilterByNames))
                {
                    filtering = FilterByName(tabName);
                }

                // Фильтр по номеру вкладки
                if (!string.IsNullOrWhiteSpace(Options.FilterByNumbers) &&
                    (!filtering.HasValue || !filtering.Value))
                {
                    filtering = filterNums.Contains(tabIndex);
                }

                if (filtering.HasValue && !filtering.Value)
                {
                    // Лист не прошел фильтр
                    continue;
                }

                resLayouts.Add(layout);
            }

            return resLayouts;
        }

        private void PlotFiles()
        {
            using (var dsdCol = new DsdEntryCollection())
            {
                var indexfile = 0;

                title = $"Печать {filesDwg.Length} файлов dwg...";

                foreach (var fileDwg in filesDwg)
                {
                    if (HostApplicationServices.Current.UserBreak())
                        throw new OperationCanceledException();
                    indexfile++;
                    using (var dbTemp = new Database(false, true))
                    {
                        dbTemp.ReadDwgFile(fileDwg, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                        dbTemp.CloseInput(true);
                        using (var t = dbTemp.TransactionManager.StartTransaction())
                        {
                            var layouts = dbTemp.GetLayouts();

                            // Фильтр листов
                            if (Options.FilterState)
                                layouts = FilterLayouts(layouts, Options);

                            var layoutsDsd = new List<Tuple<Layout, DsdEntry>>();
                            foreach (var layout in layouts)
                            {
                                var dsdEntry = new DsdEntry()
                                {
                                    Layout = layout.LayoutName,
                                    DwgName = fileDwg,
                                    NpsSourceDwg = fileDwg,
                                    Title = indexfile + "-" + layout.LayoutName
                                };
                                layoutsDsd.Add(new Tuple<Layout, DsdEntry>(layout, dsdEntry));
                            }

                            if (Options.SortTabOrName)
                            {
                                layoutsDsd.Sort((l1, l2) => l1.Item1.TabOrder.CompareTo(l2.Item1.TabOrder));
                            }
                            else
                            {
                                layoutsDsd.Sort((l1, l2) =>
                                    string.Compare(l1.Item1.LayoutName, l2.Item1.LayoutName, StringComparison.Ordinal));
                            }

                            layoutsDsd.ForEach(l => dsdCol.Add(l.Item2));
                            t.Commit();
                        }
                    }
                }

                PublisherDSD(dsdCol);
            }
        }

        private void PlotFileToPdf(string file)
        {
            dir = Path.GetDirectoryName(file);
            filePdfOutputName = Path.GetFileNameWithoutExtension(file);
            filesDwg = new[] { file };
            PlotFiles();
        }

        private void PostProcessDSD([NotNull] DsdData dsd, [NotNull] string destFile)
        {
            var tmpFile = Path.Combine(dir, "temp.dsd");
            dsd.WriteDsd(tmpFile);
            using (var reader = new StreamReader(tmpFile, Encoding.Default))
            using (var writer = new StreamWriter(destFile, false, Encoding.Default))
            {
                var fileDwg = string.Empty;
                while (!reader.EndOfStream)
                {
                    var str = reader.ReadLine();
                    if (str == null)
                        continue;
                    string newStr;
                    if (str.StartsWith("Has3DDWF="))
                    {
                        newStr = "Has3DDWF=0";
                    }
                    else if (str.StartsWith("DWG=", StringComparison.OrdinalIgnoreCase))
                    {
                        fileDwg = str.Substring(4);
                        newStr = str;
                    }
                    else if (str.StartsWith("OriginalSheetPath="))
                    {
                        newStr = "OriginalSheetPath=" + fileDwg;
                    }
                    else if (str.StartsWith("Type="))
                    {
                        newStr = "Type=6";
                    }
                    else if (str.StartsWith("PromptForDwfName="))
                    {
                        newStr = "PromptForDwfName=FALSE";
                    }
                    else
                    {
                        newStr = str;
                    }

                    writer.WriteLine(newStr);
                }
            }

            File.Delete(tmpFile);
        }

        private void Publisher_BeginSheet(object sender, PublishSheetEventArgs e)
        {
        }
    }
}
