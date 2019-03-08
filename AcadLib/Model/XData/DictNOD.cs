namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using XData;

    /// <summary>
    /// Сохранение и извлечение значений из словаря чертежа
    /// Работает с HostApplicationServices.WorkingDatabase при каждом вызове метода сохранения или считывания значения.
    /// Или нужно задать Database в поле Db.
    /// </summary>
    [PublicAPI]
    public class DictNOD
    {
        private readonly string dictInnerName;
        private readonly string dictName;

        [Obsolete("Используй `innerDict` конструктор.")]
        public DictNOD(string dictName)
        {
            this.dictName = dictName;
            Db = HostApplicationServices.WorkingDatabase;
        }

        public DictNOD(string innerDict, bool hasInnerDict)
        {
            dictName = ExtDicHelper.PikApp;
            dictInnerName = innerDict;
            Db = HostApplicationServices.WorkingDatabase;
        }

        public DictNOD(string innerDict, Database db)
        {
            dictName = ExtDicHelper.PikApp;
            dictInnerName = innerDict;
            Db = db;
        }

        public DictNOD(Database db)
        {
            dictName = ExtDicHelper.PikApp;
            Db = db;
        }

        public Database Db { get; set; }

        public void SaveJson(string json)
        {
            var dic = new DicED("json");
            var tvk = new TypedValueExtKit();
            tvk.Add("json", json);
            dic.AddRec("json", tvk.Values);
            Save(dic);
        }

        public string LoadJson()
        {
            var dic = LoadED("json");
            var dv = dic?.GetRec("json")?.Values.ToDictionary();
            return dv.GetTypedValue<string>("json");
        }

        /// <summary>
        /// Чтение списка записей для заданной XRecord по имени
        /// </summary>
        /// <param name="recName">Имя XRecord в словаре</param>
        /// <returns>Список значений в XRecord или null</returns>
        [CanBeNull]
        [Obsolete("Используй `DicED`")]
        public List<TypedValue> Load([NotNull] string recName)
        {
            List<TypedValue> values;
            var recId = GetRec(recName, false);
            if (recId.IsNull)
                return null;
            using (var xRec = recId.Open(OpenMode.ForRead) as Xrecord)
            {
                if (xRec == null)
                    return null;
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return null;
                    values = data.AsArray().ToList();
                }
            }

            return values;
        }

        /// <summary>
        /// Считывание из словаря, текущей рабочей бд, булевого значения из первой записи TypedValue в xRecord заданного имени.
        /// </summary>
        /// <param name="recName">Имя XRecord записи в словаре</param>
        /// <param name="defValue">Возвращаемое значение если такой записи нет в словаре.</param>
        /// <returns></returns>
        [Obsolete("Используй `DicED`")]
        public bool Load([NotNull] string recName, bool defValue)
        {
            var res = defValue; // default
            var idRec = GetRec(recName, false);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                if (xRec == null)
                    return res;
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    var values = data.AsArray();
                    if (values.Length <= 0)
                        return res;
                    try
                    {
                        return Convert.ToBoolean(values[0].Value);
                    }
                    catch
                    {
                        Logger.Log.Error(
                            $"Ошибка определения параметра '{recName}'='{values[0].Value}'. Взято значение по умолчанию '{defValue}'");
                        xRec.Close();
                        Save(defValue, recName);
                        res = defValue;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Считывание из словаря, текущей рабочей бд, целого значения из первой записи TypedValue в xRecord заданного имени.
        /// </summary>
        /// <param name="recName">Имя XRecord записи в словаре</param>
        /// <param name="defaultValue">Возвращаемое значение если такой записи нет в словаре.</param>
        /// <returns></returns>
        [Obsolete("Используй `DicED`")]
        public int Load([NotNull] string recName, int defaultValue)
        {
            var res = defaultValue;
            var idRec = GetRec(recName, false);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                if (xRec == null)
                    return res;
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    var values = data.AsArray();
                    if (!values.Any())
                        return res;
                    try
                    {
                        return (int)values[0].Value;
                    }
                    catch
                    {
                        Logger.Log.Error(
                            $"Ошибка определения параметра '{recName}'='{values[0].Value}'. Взято значение по умолчанию '{defaultValue}'");
                        xRec.Close();
                        Save(defaultValue, recName);
                        res = defaultValue;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Считывание из словаря, текущей рабочей бд, десятичного значения из первой записи TypedValue в xRecord заданного имени.
        /// </summary>
        /// <param name="recName">Имя XRecord записи в словаре</param>
        /// <param name="defaultValue">Возвращаемое значение если такой записи нет в словаре.</param>
        /// <returns></returns>
        [Obsolete("Используй `DicED`")]
        public double Load([NotNull] string recName, double defaultValue)
        {
            var res = defaultValue;
            var idRec = GetRec(recName, false);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                if (xRec == null)
                    return res;
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    var values = data.AsArray();
                    if (!values.Any())
                        return res;
                    try
                    {
                        return (double)values[0].Value;
                    }
                    catch
                    {
                        Logger.Log.Error(
                            $"Ошибка определения параметра '{recName}'='{values[0].Value}'. Взято значение по умолчанию '{defaultValue}'");
                        xRec.Close();
                        Save(defaultValue, recName);
                        res = defaultValue;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Считывание из словаря, текущей рабочей бд, строки значения из первой записи TypedValue в xRecord заданного имени.
        /// </summary>
        /// <param name="recName">Имя XRecord записи в словаре</param>
        /// <param name="defaultValue">Возвращаемое значение если такой записи нет в словаре.</param>
        ///<returns></returns>
        [Obsolete("Используй `DicED`")]
        public string Load([NotNull] string recName, string defaultValue)
        {
            var res = defaultValue;
            var idRec = GetRec(recName, false);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                if (xRec == null)
                    return res;
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    var values = data.AsArray();
                    if (values.Any())
                    {
                        return values[0].Value.ToString();
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Чтение вложенного словаря плагина - по имени вложенного словаря
        /// </summary>
        /// <param name="dicName">Имя словаря</param>
        /// <returns>Словарь по ключу `dicName` если он есть.</returns>
        [CanBeNull]
        public DicED LoadED(string dicName)
        {
            var dicPluginId = GetDicPlugin(false);
            var dicResId = ExtDicHelper.GetDic(dicPluginId, dicName, false, false);
            var res = ExtDicHelper.GetDicEd(dicResId);
            if (res != null)
            {
                res.Name = dicName;
            }

            return res;
        }

        /// <summary>
        /// Чтение всего словаря плагина
        /// </summary>
        [CanBeNull]
        public DicED LoadED()
        {
            var dicPluginId = GetDicPlugin(false);
            var res = ExtDicHelper.GetDicEd(dicPluginId);
            if (res != null)
            {
                res.Name = dictInnerName;
            }

            return res;
        }

        /// <summary>
        /// Сохранение словаря
        /// </summary>
        /// <param name="dicEd">Словарь для сохранения</param>
        public void Save([CanBeNull] DicED dicEd)
        {
            if (string.IsNullOrEmpty(dicEd?.Name)) return;
            var dicId = GetDicPlugin(true);
            ExtDicHelper.SetDicED(dicId, dicEd);
        }

        /// <summary>
        /// Сохранение булевого значения в словарь
        /// </summary>
        /// <param name="value">Сохраняемое значение</param>
        /// <param name="key">Имя записи XRecord с одним TypedValue</param>
        [Obsolete("Используй `DicED`")]
        public void Save(bool value, [NotNull] string key)
        {
            var idRec = GetRec(key, true);
            if (idRec.IsNull)
                return;

            // ReSharper disable once IdOpenMode
            using (var xRec = (Xrecord)idRec.Open(OpenMode.ForWrite))
            {
                using (var rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Bool, value));
                    if (xRec != null)
                        xRec.Data = rb;
                }
            }
        }

        /// <summary>
        /// Сохранение целого значения в словарь
        /// </summary>
        /// <param name="number">Значение</param>
        /// <param name="keyName">Имя записи XRecord с одним TypedValue</param>
        [Obsolete("Используй `DicED`")]
        public void Save(int number, [NotNull] string keyName)
        {
            var idRec = GetRec(keyName, true);
            if (idRec.IsNull)
                return;

            // ReSharper disable once IdOpenMode
            using (var xRec = (Xrecord)idRec.Open(OpenMode.ForWrite))
            {
                using (var rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Int32, number));
                    if (xRec != null)
                        xRec.Data = rb;
                }
            }
        }

        /// <summary>
        /// Сохранение десятичного значения в словарь
        /// </summary>
        /// <param name="number">Значение</param>
        /// <param name="keyName">Имя записи XRecord с одним TypedValue</param>
        [Obsolete("Используй `DicED`")]
        public void Save(double number, [NotNull] string keyName)
        {
            var idRec = GetRec(keyName, true);
            if (idRec.IsNull)
                return;

            // ReSharper disable once IdOpenMode
            using (var xRec = (Xrecord)idRec.Open(OpenMode.ForWrite))
            {
                using (var rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Real, number));
                    if (xRec != null)
                        xRec.Data = rb;
                }
            }
        }

        /// <summary>
        /// Сохранение строки значения в словарь
        /// </summary>
        /// <param name="text">Значение</param>
        /// <param name="key">Имя записи XRecord с одним TypedValue</param>
        [Obsolete("Используй `DicED`")]
        public void Save(string text, [NotNull] string key)
        {
            var idRec = GetRec(key, true);
            if (idRec.IsNull)
                return;

            // ReSharper disable once IdOpenMode
            using (var xRec = (Xrecord)idRec.Open(OpenMode.ForWrite))
            {
                using (var rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Text, text));
                    if (xRec != null)
                        xRec.Data = rb;
                }
            }
        }

        [Obsolete("Используй `DicED`")]
        public void Save([CanBeNull] List<TypedValue> values, string key)
        {
            if (values == null || values.Count == 0)
                return;
            var idRec = GetRec(key, true);
            if (idRec.IsNull)
                return;

            // ReSharper disable once IdOpenMode
            using (var xRec = (Xrecord)idRec.Open(OpenMode.ForWrite))
            {
                using (var rb = new ResultBuffer(values.ToArray()))
                {
                    if (xRec != null)
                        xRec.Data = rb;
                }
            }
        }

        private ObjectId GetDicPlugin(bool create)
        {
            ObjectId res;
            var dicNodId = Db.NamedObjectsDictionaryId;
            var dicPikId = ExtDicHelper.GetDic(dicNodId, dictName, create, false);
            if (string.IsNullOrEmpty(dictInnerName))
            {
                res = dicPikId;
            }
            else
            {
                var dicPluginId = ExtDicHelper.GetDic(dicPikId, dictInnerName, create, false);
                res = dicPluginId;
            }

            return res;
        }

        [Obsolete("Используй `DicED`")]
        private ObjectId GetRec([NotNull] string key, bool create)
        {
            var dicId = GetDicPlugin(create);
            return ExtDicHelper.GetRec(dicId, key, create, false);
        }
    }
}
