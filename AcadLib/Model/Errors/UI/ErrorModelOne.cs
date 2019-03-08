namespace AcadLib.Errors.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using Layers;
    using NetLib;
    using ReactiveUI;
    using Visibility = System.Windows.Visibility;

    public class ErrorModelOne : ErrorModelBase
    {
        public ErrorModelOne([NotNull] IError err, [CanBeNull] ErrorModelList parent) : base(err)
        {
            VisibilityCount = Visibility.Collapsed;
            Parent = parent;
            if (parent == null)
            {
                if (err.Group.IsNullOrEmpty() || err.Message.StartsWith(err.Group))
                {
                    Message = err.Message;
                }
                else
                {
                    Message = $"{err.Group} {err.Message}";
                }

                MarginHeader = new Thickness(1, 1, 1, 1);
            }
            else
            {
                Message = err.Message.StartsWith(err.Group)
                    ? err.Message.Substring(err.Group.Length)
                    : err.Message;
            }

            AddButtons = err.AddButtons;

            // Добавить кнопку, для отрисовки визуализации на чертежа
            if (HasVisuals)
            {
                if (AddButtons == null)
                {
                    AddButtons = new List<ErrorAddButton>();
                    AddVisualButton();
                }
                else if (!HasVisualButton())
                {
                    AddVisualButton();
                }
            }
        }

        public ErrorModelList Parent { get; set; }

        private bool HasVisualButton()
        {
            return AddButtons.Any(b => b.Name == "Отрисовка");
        }

        private void AddVisualButton()
        {
            var visCommand = CreateCommand(AddVisualsToDrawing, Observable.Start(() => Error?.Visuals?.Any() == true));
            var visButton = new ErrorAddButton
            {
                Name = "Отрисовка",
                Tooltip = "Добавить визуализацию ошибки в чертеж.",
                Click = visCommand
            };
            AddButtons.Add(visButton);
        }

        private void AddVisualsToDrawing()
        {
            try
            {
                var doc = AcadHelper.Doc;
                var db = doc.Database;
                using (doc.LockDocument())
                using (var t = db.TransactionManager.StartTransaction())
                {
                    var layerVisual = LayerExt.CheckLayerState("visuals");
                    var ms = (BlockTableRecord)SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForWrite);
                    var fEnt = Error.Visuals.First();
                    var fEntExt = new Extents3d();
                    var fEntId = ObjectId.Null;
                    foreach (var entity in Error.Visuals)
                    {
                        var entClone = (Entity)entity.Clone();
                        entClone.LayerId = layerVisual;
                        ms.AppendEntity(entClone);
                        t.AddNewlyCreatedDBObject(entClone, true);
                        if (fEnt == entity)
                        {
                            fEntId = entClone.Id;
                            try
                            {
                                fEntExt = entClone.GeometricExtents;
                            }
                            catch
                            {
                                //
                            }
                        }

                        entity.Dispose();
                    }

                    if (!Error.HasEntity && !fEntId.IsNull)
                    {
                        Error.HasEntity = true;
                        Error.IdEnt = fEntId;
                        Error.Extents = fEntExt;
                        HasShow = true;
                    }

                    Error.Visuals = new List<Entity>();
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
