using AcadLib.Visual;

namespace AcadLib.Errors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;
    using System.Windows;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;
    using NetLib.WPF;
    using OfficeOpenXml;
    using ReactiveUI;
    using ReactiveUI.Legacy;
    using UI;
    using Unit = System.Reactive.Unit;

    public class ErrorsVM : BaseViewModel
    {
        private readonly VisualTransientSimple errorsVisual;

        public ErrorsVM()
        {
        }

        public ErrorsVM([NotNull] List<IError> errors)
        {
            ErrorsOrig = errors;

            // Группировка ошибок
            // "Дублирование блоков"
            Errors = new ReactiveList<ErrorModelBase>(errors.Where(w => !string.IsNullOrEmpty(w.Message)).GroupBy(g => g.Group)
                .Select(s =>
                {
                    ErrorModelBase errModel;
                    if (s.Skip(1).Any())
                    {
                        var errList = new ErrorModelList(s.ToList());
                        errList.SameErrors.Iterate(e => e.SelectionChanged += ErrModel_SelectionChanged);
                        errModel = errList;
                    }
                    else
                    {
                        errModel = new ErrorModelOne(s.First(), null);
                    }
                    errModel.SelectionChanged += ErrModel_SelectionChanged;
                    return errModel;
                }).ToList());
            IsDublicateBlocksEnabled = errors.Any(e => e.Message?.StartsWith("Дублирование блоков") == true ||
                                                       e.Message?.StartsWith("Наложение блоков") == true);
            ErrorsCountInfo = errors.Count;

            var canCollapse = Errors.CountChanged.Select(s => Errors.OfType<ErrorModelList>().Any(a => a.IsExpanded));
            CollapseAll = CreateCommand(CollapseAllExecute, canCollapse);
            var canExpand = Errors.CountChanged.Select(s =>
                Errors.OfType<ErrorModelList>().Any(a => a.SameErrors != null && !a.IsExpanded));
            ExpandeAll = CreateCommand(ExpandedAllExecute, canExpand);
            ExportToExcel = CreateCommand(ExportToExcelExecute);
            ExportToTxt = CreateCommand(ExportToTxtExecute);
            DeleteSelectedDublicateBlocks = CreateCommand(OnDeleteSelectedDublicateBlocksExecute);
            DeleteAllDublicateBlocks = CreateCommand(OnDeleteAllDublicateBlocksExecute);
            DeleteError = CreateCommand<ErrorModelBase>(DeleteErrorExec);
            var visualsEnts = ErrorsOrig.SelectManyNulless(s => s.Visuals).ToList();
            if (visualsEnts.Any())
            {
                errorsVisual = new VisualTransientSimple(visualsEnts) { VisualIsOn = true };
            }
        }

        public ReactiveCommand<Unit, Unit> CollapseAll { get; set; }

        [Reactive]
        public int CountSelectedErrors { get; set; }

        public ReactiveCommand<ErrorModelBase, Unit> DeleteError { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteSelectedDublicateBlocks { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteAllDublicateBlocks { get; set; }

        public new ReactiveList<ErrorModelBase> Errors { get; set; }

        [Reactive]
        public int ErrorsCountInfo { get; set; }

        public List<IError> ErrorsOrig { get; set; }

        public ReactiveCommand<Unit, Unit> ExpandeAll { get; set; }

        public ReactiveCommand<Unit, Unit> ExportToExcel { get; set; }

        public ReactiveCommand<Unit, Unit> ExportToTxt { get; set; }

        public bool IsDialog { get; set; }

        public bool IsDublicateBlocksEnabled { get; set; }

        public override void OnClosed()
        {
            errorsVisual?.Dispose();
            if (ErrorsOrig?.Any() == true)
            {
                foreach (var entity in ErrorsOrig.SelectManyNulless(s=>s.Visuals))
                {
                    entity?.Dispose();
                }
            }
        }

        /// <summary>
        /// Удаление выделенных ошибок
        /// </summary>
        public void DeleteSelectedErrors()
        {
            var selectedErrors = GetSelectedErrors(out var _);
            RemoveErrors(selectedErrors);
        }

        private void CollapseAllExecute()
        {
            foreach (var item in Errors.OfType<ErrorModelList>())
            {
                item.IsExpanded = false;
            }
        }

        private void DeleteErrorExec([NotNull] ErrorModelBase errorBase)
        {
            if (errorBase is ErrorModelOne errOne && errOne.Parent != null)
            {
                errOne.Parent.SameErrors.Remove(errorBase);
            }
            else
            {
                Errors.Remove(errorBase);
            }

            if (errorBase.Error == null)
            {
                throw new ArgumentException("Ошибка не найдена.");
            }

            if (!errorBase.Error.IdEnt.IsValidEx() && errorBase.Error.HasEntity)
            {
                throw new Exception("Элемент ошибки не валидный. Возможно был удален.");
            }

            var doc = AcadHelper.Doc;
            var db = doc.Database;
            if (errorBase.Error.IdEnt.Database != db)
            {
                throw new Exception($"Переключитесь на чертеж '{Path.GetFileName(doc.Name)}'");
            }

            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var ent = errorBase.Error.IdEnt.GetObject<Entity>(OpenMode.ForWrite);
                ent?.Erase();
                if (errorBase is ErrorModelList errList)
                {
                    foreach (var error in errList.SameErrors)
                    {
                        ent = error.Error.IdEnt.GetObject<Entity>(OpenMode.ForWrite);
                        ent?.Erase();
                    }
                }

                t.Commit();
            }
        }

        private void ErrModel_SelectionChanged(object sender, bool e)
        {
            CountSelectedErrors += e ? 1 : -1;
        }

        private void ExpandedAllExecute()
        {
            foreach (var item in Errors.OfType<ErrorModelList>())
            {
                item.IsExpanded = true;
            }
        }

        private void ExportToExcelExecute()
        {
            try
            {
                var tempFile = new FileInfo(NetLib.IO.Path.GetTempFile(".xlsx"));
                using (var excel = new ExcelPackage(tempFile))
                {
                    // Открываем книгу
                    var ws = excel.Workbook.Worksheets.Add("Ошибки");
                    var row = 1;

                    // Название
                    ws.Cells[row, 1].Value = "Список ошибок";
                    row++;
                    foreach (var err in Errors)
                    {
                        if (err is ErrorModelList errlist)
                        {
                            foreach (var item in errlist.SameErrors)
                            {
                                ws.Cells[row, 1].Value = item.Message;
                                row++;
                            }
                        }
                        else
                        {
                            ws.Cells[row, 1].Value = err.Message;
                            row++;
                        }
                    }

                    excel.Save();
                }

                Process.Start(tempFile.FullName);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, @"Ошибка");
                AcadLib.Logger.Log.Error(ex, "Сохранение ошибок в Excel");
            }
        }

        private void ExportToTxtExecute()
        {
            var sbText = new StringBuilder("Список ошибок:").AppendLine();
            foreach (var err in Errors)
            {
                if (err is ErrorModelList errlist)
                {
                    sbText.AppendLine(errlist.Header.Message);
                    foreach (var item in errlist.SameErrors)
                    {
                        sbText.AppendLine(item.Message);
                    }
                }
                else
                {
                    sbText.AppendLine(err.Message);
                }
            }

            var fileTxt = Path.GetTempPath() + Guid.NewGuid() + ".txt";
            File.WriteAllText(fileTxt, sbText.ToString());
            Process.Start(fileTxt);
        }

        [NotNull]
        private List<ErrorModelBase> GetSelectedErrors([NotNull] out List<IError> errors)
        {
            errors = new List<IError>();
            var selectedErrors = new List<ErrorModelBase>();
            foreach (var err in Errors)
            {
                if (err.IsSelected)
                {
                    selectedErrors.Add(err);
                    errors.Add(err.Error);
                }
                else if (err is ErrorModelList errlist)
                {
                    foreach (var innerErr in errlist.SameErrors)
                    {
                        if (!innerErr.IsSelected)
                            continue;
                        selectedErrors.Add(innerErr);
                        errors.Add(innerErr.Error);
                    }
                }
            }

            return selectedErrors;
        }

        private void OnDeleteSelectedDublicateBlocksExecute()
        {
            var selectedErrors = GetSelectedErrors(out _);
            if (selectedErrors.Count == 0)
            {
                MessageBox.Show("Нет выделенных ошибок.");
                return;
            }

            try
            {
                var errors = GetErrorsInners(selectedErrors);
                Blocks.Dublicate.CheckDublicateBlocks.DeleteDublicates(errors);
                RemoveErrors(selectedErrors);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления дубликатов блоков - {ex.Message}");
            }
        }

        private List<IError> GetErrorsInners(IEnumerable<ErrorModelBase> errorsVM)
        {
            var errors = new List<IError>();
            foreach (var errVm in errorsVM)
            {
                switch (errVm)
                {
                    case ErrorModelList errList:
                        errors.AddRange(errList.SameErrors.Select(s => s.Error));
                        break;
                    case ErrorModelOne _:
                        errors.Add(errVm.Error);
                        break;
                }
            }

            return errors;
        }

        private void OnDeleteAllDublicateBlocksExecute()
        {
            try
            {
                var errs = GetErrorsInners(Errors);
                Blocks.Dublicate.CheckDublicateBlocks.DeleteDublicates(errs);
                MessageBox.Show("Дубликаты удалены");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления дубликатов блоков - {ex.Message}");
            }
        }

        private void RemoveErrors([NotNull] List<ErrorModelBase> selectedErrors)
        {
            var countIsSelectedErr = 0;
            foreach (var item in selectedErrors)
            {
                if (item is ErrorModelOne errOne)
                {
                    errOne.Parent.SameErrors.Remove(item);
                }
                else
                {
                    Errors.Remove(item);
                }

                if (item.IsSelected)
                    countIsSelectedErr++;
            }

            ErrorsCountInfo -= selectedErrors.Count;
            CountSelectedErrors -= countIsSelectedErr;
        }
    }
}
