namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using Layers;

    [PublicAPI]
    public static class BlockInsert
    {
        // Файл шаблонов блоков
        internal static readonly string fileCommonBlocks = Path.Combine(
            AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder,
            @"Blocks\Блоки-оформления.dwg");

        /// <summary>
        /// Добавление атрибутов к вставке блока
        /// </summary>
        public static void AddAttributes(BlockReference blRef, [NotNull] BlockTableRecord btrBl, Transaction t)
        {
            foreach (var idEnt in btrBl)
            {
                if (idEnt.ObjectClass.Name == "AcDbAttributeDefinition")
                {
                    var atrDef = (AttributeDefinition)t.GetObject(idEnt, OpenMode.ForRead);
                    if (!atrDef.Constant)
                    {
                        using (var atrRef = new AttributeReference())
                        {
                            atrRef.SetAttributeFromBlock(atrDef, blRef.BlockTransform);
                            blRef.AttributeCollection.AppendAttribute(atrRef);
                            t.AddNewlyCreatedDBObject(atrRef, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Вставка блока в чертеж - интерактивная (BlockInsertJig)
        /// </summary>
        public static ObjectId Insert(string blName, LayerInfo layer, List<Property> props, bool explode = false)
        {
            ObjectId idBlRefInsert;
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return ObjectId.Null;
            var db = doc.Database;
            var ed = doc.Editor;
            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)t.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (!bt.Has(blName))
                {
                    throw new Exception("Блок не определен в чертеже " + blName);
                }

                var idBlBtr = bt[blName];
                var pt = Point3d.Origin;
                var br = new BlockReference(pt, idBlBtr);
                br.SetDatabaseDefaults();

                if (layer != null)
                {
                    layer.CheckLayerState();
                    br.Layer = layer.Name;
                }

                var spaceBtr = (BlockTableRecord)t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                idBlRefInsert = spaceBtr.AppendEntity(br);
                t.AddNewlyCreatedDBObject(br, true);

                if (props != null && br.IsDynamicBlock)
                {
                    foreach (DynamicBlockReferenceProperty item in br.DynamicBlockReferencePropertyCollection)
                    {
                        var prop = props.FirstOrDefault(p =>
                            p.Name.Equals(item.PropertyName, StringComparison.OrdinalIgnoreCase));
                        if (prop != null)
                        {
                            try
                            {
                                item.Value = prop.Value;
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Error(ex,
                                    msg: $"Ошибка типа значения для дин параметра '{item.PropertyName}' " +
                                         $"при вставке блока '{blName}': тип устанавливаемого значение '{prop.Value.GetType()}', " +
                                         $"а должен быть тип '{item.UnitsType}'");
                            }
                        }
                    }
                }

                // jig
                var entJig = new Jigs.BlockInsertJig(br);
                var pr = ed.Drag(entJig);
                if (pr.Status == PromptStatus.OK)
                {
                    var btrBl = (BlockTableRecord)t.GetObject(idBlBtr, OpenMode.ForRead);
                    if (btrBl.HasAttributeDefinitions)
                        AddAttributes(br, btrBl, t);
                    if (explode)
                    {
                        var owner = br.BlockId.GetObject<BlockTableRecord>(OpenMode.ForWrite);
                        using (var explodes = new DBObjectCollection())
                        {
                            br.Explode(explodes);
                            foreach (Entity ent in explodes)
                            {
                                owner.AppendEntity(ent);
                                t.AddNewlyCreatedDBObject(ent, true);
                                ent.Layer = br.Layer;
                            }

                            br.Erase();
                        }
                    }
                }
                else
                {
                    br.Erase();
                    idBlRefInsert = ObjectId.Null;
                }

                t.Commit();
            }

            return idBlRefInsert;
        }

        /// <summary>
        /// Вставка блока в чертеж - интерактивная (BlockInsertJig)
        /// </summary>
        public static ObjectId Insert(string blName, LayerInfo layer, bool explode = false)
        {
            return Insert(blName, layer, null, explode);
        }

        /// <summary>
        /// Вставка блока в чертеж - интерактивная (BlockInsertJig)
        /// </summary>
        public static ObjectId Insert(string blName, string layer)
        {
            var layerInfo = new LayerInfo(layer);
            return Insert(blName, layerInfo);
        }

        /// <summary>
        /// Вставка блока в чертеж - интерактивная (BlockInsertJig)
        /// </summary>
        public static ObjectId Insert(string blName)
        {
            return Insert(blName, (LayerInfo)null);
        }

        /// <summary>
        /// Вставка вхождения блока
        /// </summary>
        /// <param name="blName">Имя блока</param>
        /// <param name="pt">Точка вставки</param>
        /// <param name="owner">Контейнер</param>
        /// <param name="t"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        [NotNull]
        public static BlockReference InsertBlockRef(
            string blName,
            Point3d pt,
            [NotNull] BlockTableRecord owner,
            [NotNull] Transaction t,
            double scale = 1)
        {
            var db = owner.Database;
            var bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
            var btr = (BlockTableRecord)bt[blName].GetObject(OpenMode.ForRead);
            return InsertBlockRef(btr, pt, owner, t, scale);
        }

        /// <summary>
        /// Вставка вхождения блока
        /// </summary>
        public static BlockReference InsertBlockRef(
            BlockTableRecord btr,
            Point3d pt,
            [NotNull] BlockTableRecord owner,
            [NotNull] Transaction t,
            double scale = 1)
        {
            return InsertBlockRef(btr.Id, pt, owner, t, scale);
        }

        /// <summary>
        /// Вставка вхождения блока
        /// </summary>
        public static BlockReference InsertBlockRef(
            ObjectId btrId,
            Point3d pt,
            [NotNull] BlockTableRecord owner,
            [NotNull] Transaction t,
            double scale = 1)
        {
            var db = owner.Database;
            var blRef = new BlockReference(pt, btrId)
            {
                Position = pt
            };
            if (blRef.Annotative == AnnotativeStates.True)
            {
                // Установка аннотативного масштаба
                blRef.AddContext(db.Cannoscale);
            }
            else if (Math.Abs(scale - 1) > 0.001)
            {
                blRef.TransformBy(Matrix3d.Scaling(scale, pt));
            }

            owner.AppendEntity(blRef);
            t.AddNewlyCreatedDBObject(blRef, true);
            AddAttributes(blRef, btrId.GetObjectT<BlockTableRecord>(), t);
            return blRef;
        }

        /// <summary>
        /// Вставка общего блока из файла Блоки-Оформления.
        /// Визуальная вставка с помошью Jig
        /// </summary>
        public static ObjectId InsertCommonBlock(string blName, Database db)
        {
            // Выбор и вставка блока
            Block.CopyBlockFromExternalDrawing(blName, fileCommonBlocks, db);
            return Insert(blName);
        }
    }
}