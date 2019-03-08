namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Errors;
    using JetBrains.Annotations;
    using NetLib;

    /// <inheritdoc />
    /// <summary>
    /// Базовое описание блока
    /// </summary>
    [PublicAPI]
    public class BlockBase : IBlock
    {
        private bool _alreadyCalcExtents;
        private Extents3d _extentsToShow;
        private bool _isNullExtents;

        /// <summary>
        /// Блок - по имени и ссылке на вхождение блока
        /// Заполняются параметры блока. и граница Bounds
        /// </summary>
        public BlockBase([NotNull] BlockReference blRef, string blName)
        {
            BlName = blName;

            Update(blRef);
        }

        public bool DontAddErrorsToInspector { get; set; }

        public Database Db { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Имя блока - эффективное
        /// </summary>
        public string BlName { get; set; }

        public string BlLayer { get; set; }

        public ObjectId LayerId { get; set; }

        public bool IsVisible { get; set; } = true;

        public virtual Color Color { get; set; }

        public Point3d Position { get; set; }

        /// <summary>
        /// Границы блока Bounds
        /// </summary>
        public virtual Extents3d? Bounds { get; set; }

        /// <summary>
        /// Id вхождения блока
        /// </summary>
        public ObjectId IdBlRef { get; set; }

        /// <summary>
        /// Id определения блока - BklockTableRecord (для анонимных - DynamicBlockTableRecord).
        /// </summary>
        public ObjectId IdBtr { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Для динамических блоков - анонимное определение блока
        /// </summary>
        [Obsolete("Для дин. блоков определение оригинального дин блока см в IdBtrDyn. Скоро будет удалено.")]
        public ObjectId IdBtrAnonym { get; set; }

        public ObjectId IdBtrDyn { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Пространство в который вставлен этот блок (определение блока)
        /// </summary>
        public ObjectId IdBtrOwner { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Параметры - атрибутв и динамические
        /// </summary>
        public List<Property> Properties { get; set; }

        public Error Error { get; set; }

        public Matrix3d Transform { get; set; }

        public double Rotation { get; set; }

        public Scale3d Scale { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Границы для показа пользователю
        /// </summary>
        public Extents3d ExtentsToShow
        {
            get
            {
                if (!_alreadyCalcExtents)
                {
                    using (var blRef = (BlockReference)IdBlRef.Open(OpenMode.ForRead, false, true))
                    {
                        try
                        {
                            _extentsToShow = blRef.GeometricExtents;
                            _alreadyCalcExtents = true;
                        }
                        catch
                        {
                            _isNullExtents = true;
                            _extentsToShow = new Extents3d(new Point3d(blRef.Position.X - 100, blRef.Position.Y - 100, 0),
                                new Point3d(blRef.Position.X + 100, blRef.Position.Y + 100, 0));
                        }
                    }
                }

                return _extentsToShow;
            }
            set
            {
                _alreadyCalcExtents = true;
                _extentsToShow = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Показ блока (по границе) пользователю с миганием
        /// С проверкой чертежа и блокировкой.
        /// </summary>
        public virtual void Show()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                if (doc.Database != IdBlRef.Database)
                {
                    Application.ShowAlertDialog(
                        $"Переключитесь на чертеж {Path.GetFileNameWithoutExtension(IdBlRef.Database.Filename)}");
                    return;
                }

                using (doc.LockDocument())
                {
                    var ed = doc.Editor;
                    var ext = ExtentsToShow;
                    if (_isNullExtents)
                    {
                        Application.ShowAlertDialog("Границы объекта не определены.");
                    }

                    ed.Zoom(ext);
                    IdBlRef.FlickObjectHighlight(2, 100, 100);
                }
            }
        }

        public void Delete()
        {
            var blRef = IdBlRef.GetObject(OpenMode.ForWrite);
            blRef.Erase();
        }

        [CanBeNull]
        public T GetPropValue<T>(string propMatch, bool isRequired = true, bool exactMatch = true)
        {
            return GetPropValue<T>(propMatch, out var _, isRequired, exactMatch);
        }

        [CanBeNull]
        public T GetPropValue<T>(string propMatch, out bool hasProperty, bool isRequired = true, bool exactMatch = true)
        {
            var resVal = default(T);
            if (exactMatch)
            {
                propMatch = $"^{propMatch}$";
            }

            var prop = GetProperty(propMatch, isRequired);
            if (prop != null)
            {
                hasProperty = true;
                try
                {
                    resVal = prop.Value.GetValue<T>();
                }
                catch (Exception ex)
                {
                    var err = $"Недопустимый тип значения параметра '{propMatch}'= {prop.Value}";
                    if (isRequired)
                        AddError(err);
                    else
                        Logger.Log.Error(ex, err);
                }
            }
            else
            {
                hasProperty = false;
            }

            return resVal;
        }

        /// <summary>
        /// Получение значения свойства (атрибута, динамического свойства)
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="propName">Имя свойства</param>
        /// <param name="defaultValue">Значение поумолчанию</param>
        /// <param name="isrequired">Обязательное свойство</param>
        /// <param name="exactMatch">Точное соответствие имени свойства</param>
        /// <param name="writeDefaultValue">Требуется транзакция! Записывать ли значение поумолчанию в свойство, если оно есть и если его значение является дефолтным для данного типа (например:0 для чисел)</param>
        /// <returns>Значение свойства</returns>
        public T GetPropValue<T>(
            string propName,
            T defaultValue,
            bool isrequired = false,
            bool exactMatch = true,
            bool writeDefaultValue = false)
        {
            var res = GetPropValue<T>(propName, out var hasProp, isrequired, exactMatch);
            if (EqualityComparer<T>.Default.Equals(res, default))
            {
                if (writeDefaultValue && hasProp)
                {
                    try
                    {
                        FillPropValue(propName, defaultValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, $"BlockBase.GetPropValue - FillPropValue - '{propName}', блок {BlName}");
                    }
                }

                return defaultValue;
            }

            return res;
        }

        [CanBeNull]
        public Property GetProperty(string nameMatch, bool isRequired = true)
        {
            var prop = Properties.Find(p => Regex.IsMatch(p.Name, nameMatch, RegexOptions.IgnoreCase));
            if (prop == null && isRequired)
            {
                AddError($"Не определен параметр '{nameMatch}'.");
            }

            return prop;
        }

        public void FillPropValue(string propMatch, object value, bool exactMatch = true, bool isRequired = true)
        {
            if (exactMatch)
            {
                propMatch = $"^{propMatch}$";
            }

            FillProp(GetProperty(propMatch, isRequired), value);
        }

        public void AddError(string msg)
        {
            if (Error == null)
            {
                Error = new Error($"id{IdBlRef}: ", IdBlRef, System.Drawing.SystemIcons.Error)
                {
                    Group = $"Ошибка в блоке '{BlName}'"
                };
                if (!DontAddErrorsToInspector)
                    Inspector.AddError(Error);
            }

            Error.AdditionToMessage(msg);
        }

        /// <summary>
        /// Поиск полилинии в этом блоке на слое
        /// </summary>
        [NotNull]
        public List<Polyline> FindPolylineInLayer(string layer)
        {
            var btr = (BlockTableRecord)IdBtr.GetObject(OpenMode.ForRead);
            var allPls = btr.GetObjects<Polyline>(OpenMode.ForRead);
            var pls = allPls.Where(p => p.Visible && p.Layer.Equals(layer, StringComparison.OrdinalIgnoreCase)).ToList();
            return pls;
        }

        /// <summary>
        /// Копирование объекта из этого блока в модель (btr)
        /// </summary>
        /// <param name="idBtrNew">Куда копировать</param>
        /// <param name="idEnt">Что копировать</param>
        /// <returns>Скопированный объект</returns>
        public ObjectId CopyEntToModel(ObjectId idBtrNew, ObjectId idEnt)
        {
            if (!idEnt.IsNull)
            {
                var idCopy = idEnt.CopyEnt(idBtrNew);
                using (var entCopy = idCopy.GetObject<Entity>(OpenMode.ForWrite))
                {
                    entCopy.TransformBy(Transform);
                    return entCopy.Id;
                }
            }

            return ObjectId.Null;
        }

        public bool Equals(IBlock other)
        {
            // Если все параметры совпадают
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            var res = new HashSet<Property>(Properties).SetEquals(other.Properties);
            return res;
        }

        public override int GetHashCode()
        {
            return BlName.GetHashCode();
        }

        public virtual void Update([NotNull] BlockReference blRef)
        {
            // Считать блок заново
            Db = blRef.Database;
            IdBtrOwner = blRef.OwnerId;
            IdBlRef = blRef.Id;
            IdBtr = blRef.BlockTableRecord;
            if (blRef.IsDynamicBlock)
            {
                IdBtrDyn = blRef.DynamicBlockTableRecord;
                IdBtrAnonym = blRef.AnonymousBlockTableRecord;
            }

            BlLayer = blRef.Layer;
            LayerId = blRef.LayerId;
            Properties = Property.GetAllProperties(blRef);
            Bounds = blRef.Bounds;
            Position = blRef.Position;
            Transform = blRef.BlockTransform;
            Scale = blRef.ScaleFactors;
            Color = GetColor(blRef);
            Rotation = blRef.Rotation;
            if (!blRef.Visible)
            {
                IsVisible = false;
            }
        }

        protected void FillProp([CanBeNull] Property prop, object value)
        {
            if (prop == null)
                return;
            var blRef = IdBlRef.GetObjectT<BlockReference>(OpenMode.ForWrite);
            if (prop.Type == PropertyType.Attribute && !prop.IdAtrRef.IsNull)
            {
                var atr = prop.IdAtrRef.GetObject<AttributeReference>(OpenMode.ForWrite);
                var text = value?.ToString() ?? string.Empty;
                if (atr.IsMTextAttribute)
                {
                    var mt = atr.MTextAttribute;
                    mt.Contents = text;
                    atr.MTextAttribute = mt;
                    atr.UpdateMTextAttribute();
                }
                else
                {
                    atr.TextString = text;
                }

                if (!atr.IsDefaultAlignment)
                    atr.AdjustAlignment(Db);
            }
            else if (prop.Type == PropertyType.Dynamic)
            {
                if (value == null)
                    return;
                var dynProp = blRef.DynamicBlockReferencePropertyCollection.Cast<DynamicBlockReferenceProperty>()
                    .FirstOrDefault(p => p.PropertyName.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                if (dynProp != null)
                {
                    try
                    {
                        dynProp.Value = value;
                    }
                    catch
                    {
                        Inspector.AddError(
                            $"Не удалосось установить динамический параметр {prop.Name} " +
                                           $"со значением {prop.Value} в блок {BlName}",
                            IdBlRef,
                            System.Drawing.SystemIcons.Error);
                    }
                }
            }
        }

        private Color GetColor([NotNull] BlockReference blRef)
        {
            if (blRef.Color.IsByLayer && !blRef.LayerId.IsNull)
            {
                using (var lay = (LayerTableRecord)blRef.LayerId.Open(OpenMode.ForRead))
                {
                    if (lay.IsFrozen || !blRef.Visible)
                    {
                        IsVisible = false;
                    }

                    return lay.Color;
                }
            }

            return blRef.Color;
        }
    }
}
