namespace AcadLib.Layers.AutoLayers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using Registry;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    public static class AutoLayersService
    {
        private static Document _doc;
        private static AutoLayer curAutoLayer;
        private static List<ObjectId> idAddedEnts;

        public static List<AutoLayer> AutoLayers { get; set; } = GetAutoLayers();

        public static bool IsStarted { get; private set; }

        /// <summary>
        /// Автослои для всех объектов чертежа
        /// </summary>
        public static void AutoLayersAll()
        {
            var document = Application.DocumentManager.MdiActiveDocument;
            using (var t = document.TransactionManager.StartTransaction())
            {
                var db = document.Database;
                var bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                foreach (var btr in bt.GetObjects<BlockTableRecord>())
                {
                    AutoLayersBtr(btr);
                }

                t.Commit();
            }
        }

        [NotNull]
        public static string GetInfo()
        {
            var info = string.Empty;
            info += IsStarted ? "Автослои включены" : "Автослои выключены";
            if (!IsStarted || AutoLayers == null)
                return info;
            info += Environment.NewLine;
            foreach (var autoLayer in AutoLayers)
            {
                info += autoLayer.GetInfo() + Environment.NewLine;
            }

            return info;
        }

        public static void Init()
        {
            try
            {
                Load();
                if (IsStarted)
                {
                    // Проверка - допустимости автослоев для группы пользователя
                    if (!IsUserGroupAutoLayersAllowed() || string.IsNullOrEmpty(LayerExt.GroupLayerPrefix))
                    {
                        var doc = Application.DocumentManager.MdiActiveDocument;
                        doc?.Editor.WriteMessage(
                            $"\nАвтослои не поддерживаются для текущей группы пользователя - {AutoCAD_PIK_Manager.Settings.PikSettings.UserGroup}");
                        return;
                    }

                    Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "AutoLayersService Init");
            }
        }

        public static void Start()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Application.DocumentManager.DocumentActivated -= DocumentManager_DocumentActivated;
            Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
            SubscribeDocument(doc);
            IsStarted = true;
            Save();
        }

        public static void Stop()
        {
            Application.DocumentManager.DocumentActivated -= DocumentManager_DocumentActivated;
            UnsubscribeDocument(_doc);
            _doc = null;
            curAutoLayer = null;
            idAddedEnts = null;
            IsStarted = false;
            Save();
        }

        private static void AutoLayerEntities([NotNull] AutoLayer autoLayer, [NotNull] IEnumerable<ObjectId> autoLayerEnts)
        {
            var layId = autoLayer.Layer.CheckLayerState();
            foreach (var idEnt in autoLayerEnts)
            {
                var ent = idEnt.GetObject<Entity>(OpenMode.ForWrite);
                if (ent != null && ent.LayerId != layId)
                {
                    ent.LayerId = layId;
                }
            }
        }

        private static void AutoLayersBtr([CanBeNull] BlockTableRecord btr)
        {
            if (btr == null)
                return;
            var idEnts = btr.Cast<ObjectId>().ToList();
            var idBlRefs = idEnts.Where(w => w.ObjectClass == General.ClassBlRef).ToList();
            idEnts = idEnts.Except(idBlRefs).ToList();
            foreach (var idBlRef in idBlRefs)
            {
                var blRef = idBlRef.GetObject<BlockReference>();
                if (blRef == null)
                    continue;
                AutoLayersBtr(blRef.BlockTableRecord.GetObject<BlockTableRecord>());
            }

            foreach (var autoLayer in AutoLayers)
            {
                var autoLayerEnts = autoLayer.GetAutoLayerEnts(idEnts);
                AutoLayerEntities(autoLayer, autoLayerEnts);
                idEnts = idEnts.Except(autoLayerEnts).ToList();
            }
        }

        private static void Database_ObjectAppended(object sender, ObjectEventArgs e)
        {
            // Сбор всех id добавляемых объектов
            if (curAutoLayer != null)
            {
                idAddedEnts?.Add(e.DBObject.Id);
            }
        }

        private static void Doc_CommandCancelled(object sender, [NotNull] CommandEventArgs e)
        {
            EndCommand(sender as Document, e.GlobalCommandName);
        }

        private static void Doc_CommandEnded(object sender, [NotNull] CommandEventArgs e)
        {
            EndCommand(sender as Document, e.GlobalCommandName);
        }

        private static void Doc_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (!(sender is Document document))
                return;

            // Для команд автослоев - подписка на добавление объектов в чертеж
            Debug.WriteLine(e.GlobalCommandName);
            curAutoLayer = GetAutoLayerCommand(e.GlobalCommandName);
            if (curAutoLayer == null)
                return;
            idAddedEnts = new List<ObjectId>();

            // Подписка на события добавления объектов и завершения команды
            document.Database.ObjectAppended -= Database_ObjectAppended;
            document.Database.ObjectAppended += Database_ObjectAppended;
            document.CommandEnded -= Doc_CommandEnded;
            document.CommandEnded += Doc_CommandEnded;

            // При Esc объекты все равно могут быть успешно добавлены в чертеж (если зациклена командв добавления размера, есть такие)
            document.CommandCancelled -= Doc_CommandCancelled;
            document.CommandCancelled += Doc_CommandCancelled;
        }

        private static void DocumentManager_DocumentActivated(object sender, [NotNull] DocumentCollectionEventArgs e)
        {
            SubscribeDocument(e.Document);
        }

        private static void EndCommand([CanBeNull] Document document, string globalCommandName)
        {
            curAutoLayer = GetAutoLayerCommand(globalCommandName);
            if (curAutoLayer == null || document == null)
                return;

            document.Database.ObjectAppended -= Database_ObjectAppended;
            document.CommandEnded -= Doc_CommandEnded;

            // Обработка объектов
            ProcessingAutoLayers(curAutoLayer, idAddedEnts);
            curAutoLayer = null;
        }

        private static AutoLayer GetAutoLayerCommand(string globalCommandName)
        {
            return AutoLayers.Find(f => f.IsAutoLayerCommand(globalCommandName));
        }

        [NotNull]
        private static List<AutoLayer> GetAutoLayers()
        {
            var als = new List<AutoLayer>
            {
                new AutoLayerDim()
            };
            return als;
        }

        [NotNull]
        private static RegExt GetReg()
        {
            return new RegExt("AutoLayers");
        }

        private static bool IsUserGroupAutoLayersAllowed()
        {
            var userGroup = AutoCAD_PIK_Manager.Settings.PikSettings.UserGroupsCombined.First();

            // Кроме ГП и КР-СБ-ГК
            // В ГП - будет настраиваться индивидуально.
            return !userGroup.StartsWith(General.UserGroupGP) && userGroup != General.UserGroupKRSBGK;
        }

        private static void Load()
        {
            using (var reg = GetReg())
            {
                IsStarted = reg.Load("IsStarted", true);
            }
        }

        private static void ProcessingAutoLayers([NotNull] AutoLayer currentAutoLayerAutoLayer, List<ObjectId> idsAddedEnt)
        {
            var autoLayerEnts = currentAutoLayerAutoLayer.GetAutoLayerEnts(idsAddedEnt);
            if (autoLayerEnts == null)
                return;
            using (var t = _doc.TransactionManager.StartTransaction())
            {
                AutoLayerEntities(currentAutoLayerAutoLayer, autoLayerEnts);
                t.Commit();
            }
        }

        private static void Save()
        {
            using (var reg = GetReg())
            {
                reg.Save("IsStarted", IsStarted);
            }
        }

        private static void SubscribeDocument(Document document)
        {
            // Отписка в старом документе
            UnsubscribeDocument(_doc);
            _doc = document;
            if (_doc == null)
                return;

            // Подписка на события команды нового документа
            _doc.CommandWillStart -= Doc_CommandWillStart;
            _doc.CommandWillStart += Doc_CommandWillStart;
        }

        private static void UnsubscribeDocument([CanBeNull] Document document)
        {
            if (document == null)
                return;
            document.CommandWillStart -= Doc_CommandWillStart;
            document.CommandEnded -= Doc_CommandEnded;
            if (document.Database != null)
                document.Database.ObjectAppended -= Database_ObjectAppended;
            document.CommandCancelled -= Doc_CommandCancelled;
        }
    }
}