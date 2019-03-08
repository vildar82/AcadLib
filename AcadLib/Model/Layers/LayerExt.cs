namespace AcadLib.Layers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class LayerExt
    {
        private static string groupLayerPrefix;

        /// <summary>
        /// Префикс слоев - группа пользователя
        /// </summary>
        [NotNull]
        public static string GroupLayerPrefix => groupLayerPrefix ?? (groupLayerPrefix = GetGroupLayerPrefix());

        /// <summary>
        /// Все слои чертежа
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        [NotNull]
        public static List<LayerInfo> Layers([NotNull] this Database db)
        {
            List<LayerInfo> layers;
            using (var t = db.TransactionManager.StartTransaction())
            {
                var lt = db.LayerTableId.GetObjectT<LayerTable>();
                layers = lt.Cast<ObjectId>().LayersFromLTR();
                t.Commit();
            }

            return layers;
        }

        /// <summary>
        /// Список слоев из объектов чертежа
        /// </summary>
        /// <param name="entIds"></param>
        /// <returns></returns>
        [NotNull]
        public static List<LayerInfo> Layers([NotNull] this IEnumerable<ObjectId> entIds)
        {
            return entIds.GetObjects<Entity>().GroupBy(g => g.LayerId).Select(s => new LayerInfo(s.Key)).ToList();
        }

        /// <summary>
        /// Проверка блокировки слоя IsOff IsLocked IsFrozen.
        /// Если заблокировано - то разблокируется.
        /// Если слоя нет - то он создается.
        /// </summary>
        /// <param name="layers">Список слоев для проверкм в текущей рабочей базе</param>
        /// <param name="checkProps"></param>
        [NotNull]
        public static Dictionary<string, ObjectId> CheckLayerState([NotNull] this List<LayerInfo> layers, bool checkProps)
        {
            var resVal = new Dictionary<string, ObjectId>();
            var db = HostApplicationServices.WorkingDatabase;
            using (var lt = (LayerTable)db.LayerTableId.Open(OpenMode.ForRead))
            {
                foreach (var layer in layers.Where(w => w != null))
                {
                    ObjectId layId;
                    var layName = layer.Name;
                    if (layName.IsNullOrEmpty())
                    {
                        layId = db.Clayer;

                        // Берем текущиий
                        CheckLayerState(layId, out layName);
                        layer.Name = layName;
                    }
                    else if (lt.Has(layer.Name))
                    {
                        layId = lt[layer.Name];
                        CheckLayerState(layId, out _);
                        if (checkProps)
                        {
                            using (var lay = (LayerTableRecord)layId.Open(OpenMode.ForWrite))
                            {
                                layer.SetProp(lay, db);
                            }
                        }
                    }
                    else
                    {
                        layId = CreateLayer(layer, lt);
                    }

                    layer.LayerId = layId;
                    resVal.Add(layName, layId);
                }
            }

            return resVal;
        }

        /// <summary>
        /// Проверка блокировки слоя IsOff IsLocked IsFrozen.
        /// Если заблокировано - то разблокируется.
        /// Если слоя нет - то он создается.
        /// </summary>
        /// <param name="layers">Список слоев для проверкм в текущей рабочей базе</param>
        [NotNull]
        public static Dictionary<string, ObjectId> CheckLayerState([NotNull] this List<LayerInfo> layers)
        {
            return CheckLayerState(layers, false);
        }

        public static ObjectId CheckLayerState(this LayerInfo layer, bool checkProps)
        {
            var layers = new List<LayerInfo> { layer };
            var dictLays = CheckLayerState(layers, checkProps);
            return dictLays.First().Value;
        }

        public static ObjectId CheckLayerState(this LayerInfo layer)
        {
            return CheckLayerState(layer, false);
        }

        public static ObjectId CheckLayerState([NotNull] string layer)
        {
            var li = new LayerInfo(layer);
            var layersInfo = new List<LayerInfo> { li };
            var dictLays = CheckLayerState(layersInfo);
            dictLays.TryGetValue(layer, out var res);
            return res;
        }

        [NotNull]
        public static Dictionary<string, ObjectId> CheckLayerState([NotNull] string[] layers)
        {
            var layersInfo = new List<LayerInfo>();
            foreach (var item in layers)
            {
                var li = new LayerInfo(item);
                layersInfo.Add(li);
            }

            return CheckLayerState(layersInfo);
        }

        /// <summary>
        /// Создание слоя.
        /// Слоя не должно быть в таблице слоев.
        /// </summary>
        /// <param name="layerInfo">параметры слоя</param>
        /// <param name="lt">таблица слоев открытая для чтения. Выполняется UpgradeOpen и DowngradeOpen</param>
        [Obsolete("Use CheckLayerState")]
        public static ObjectId CreateLayer([NotNull] this LayerInfo layerInfo, [NotNull] LayerTable lt)
        {
            if (layerInfo.Name.IsNullOrEmpty())
                return lt.Database.Clayer;
            ObjectId idLayer;

            // Если слоя нет, то он создается.
            using (var newLayer = new LayerTableRecord())
            {
                layerInfo.SetProp(newLayer, lt.Database);
                lt.UpgradeOpen();
                idLayer = lt.Add(newLayer);
                lt.DowngradeOpen();
            }

            return idLayer;
        }

        /// <summary>
        /// Получение слоя.
        /// Если его нет в базе, то создается.
        /// </summary>
        /// <param name="layerInfo">параметры слоя</param>
        /// <returns></returns>
        [Obsolete("Use ChackLayerState")]
        public static ObjectId GetLayerOrCreateNew([NotNull] this LayerInfo layerInfo)
        {
            ObjectId idLayer;
            var db = HostApplicationServices.WorkingDatabase;

            // Если уже был создан слой, то возвращаем его. Опасно, т.к. перед повторным запуском команды покраски, могут удалить/переименовать слой марок.
            using (var lt = (LayerTable)db.LayerTableId.Open(OpenMode.ForRead))
            {
                idLayer = lt.Has(layerInfo.Name) ? lt[layerInfo.Name] : CreateLayer(layerInfo, lt);
            }

            return idLayer;
        }

        [NotNull]
        private static List<LayerInfo> LayersFromLTR([NotNull] this IEnumerable<ObjectId> ltrIds)
        {
            return ltrIds.Select(s => new LayerInfo(s)).OrderBy(o => o.Name).ToList();
        }

        private static void CheckLayerState(ObjectId layerId, out string layerName)
        {
            layerName = null;
            if (!layerId.IsValidEx())
                return;
            using (var lay = (LayerTableRecord)layerId.Open(OpenMode.ForRead))
            {
                layerName = lay.Name;
                if (lay.IsLocked || lay.IsOff || lay.IsFrozen)
                {
                    lay.UpgradeOpen();
                    if (lay.IsOff)
                    {
                        lay.IsOff = false;
                    }

                    if (lay.IsLocked)
                    {
                        lay.IsLocked = false;
                    }

                    if (lay.IsFrozen)
                    {
                        lay.IsFrozen = false;
                    }
                }
            }
        }

        [NotNull]
        private static string GetGroupLayerPrefix()
        {
            var usergroup = AutoCAD_PIK_Manager.Settings.PikSettings.UserGroup;
            switch (usergroup)
            {
                case "КР-МН":
                    return "КР";

                case "КР-МН_Тест":
                    return "КР";

                case "КР-СБ":
                    return "СБ";

                case "КР-СБ-ГК":
                    return string.Empty;
            }

            if (usergroup.StartsWith("ГП"))
            {
                return string.Empty;
            }

            if (usergroup.Contains(","))
            {
                return string.Empty;
            }

            return usergroup == "ЖБК-ТО" ? "ОЗЖБК" : usergroup;
        }
    }
}
