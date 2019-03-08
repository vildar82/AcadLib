using System.Threading.Tasks;
using System.Windows.Threading;

namespace AcadLib.Errors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;
    using NetLib.WPF.Data;
    using UI;

    [PublicAPI]
    public class Error : IError
    {
        protected string _msg;
        protected string _shortMsg;
        protected ObjectId _idEnt;
        protected Extents3d _extents;
        protected bool _alreadyCalcExtents;
        protected bool _isNullExtents;
        private static readonly Dictionary<Icon, ErrorStatus> dictErrorIcons = new Dictionary<Icon, ErrorStatus>()
        {
            { SystemIcons.Error, ErrorStatus.Error },
            { SystemIcons.Exclamation, ErrorStatus.Exclamation },
            { SystemIcons.Hand, ErrorStatus.Exclamation },
            { SystemIcons.Information, ErrorStatus.Info },
            { SystemIcons.Warning, ErrorStatus.Error }
        };

        public Error()
        {
        }

        public Error(string message, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            HasEntity = false;
            Icon = icon ?? SystemIcons.Error;

            // Trans = Matrix3d.Identity;
            DefineStatus();
        }

        public Error(string message, [NotNull] Entity ent, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _idEnt = ent.Id;
            HasEntity = true;
            Icon = icon ?? SystemIcons.Error;
            Trans = Matrix3d.Identity;
            CanShow = true;
            DefineStatus();
        }

        public Error(string message, [NotNull] Entity ent, Matrix3d trans, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _idEnt = ent.Id;
            HasEntity = true;
            Icon = icon ?? SystemIcons.Error;
            Trans = trans;
            CanShow = true;
            DefineStatus();
        }

        public Error(string message, Extents3d ext, [NotNull] Entity ent, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _idEnt = ent.Id;
            _extents = ext;
            _alreadyCalcExtents = true;
            HasEntity = true;
            Icon = icon ?? SystemIcons.Error;
            Trans = Matrix3d.Identity;
            CanShow = true;
            DefineStatus();
        }

        public Error(string message, Extents3d ext, ObjectId idEnt, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _idEnt = idEnt;
            _extents = ext;
            _alreadyCalcExtents = true;
            HasEntity = true;
            Icon = icon ?? SystemIcons.Error;
            Trans = Matrix3d.Identity;
            CanShow = true;
            DefineStatus();
        }

        public Error(string message, Extents3d ext, Matrix3d trans, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _extents = ext;
            _extents.TransformBy(trans);
            _alreadyCalcExtents = true;
            HasEntity = false;
            Icon = icon ?? SystemIcons.Error;
            Trans = trans;
            CanShow = true;
            DefineStatus();
        }

        public Error(string message, ObjectId idEnt, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _idEnt = idEnt;
            HasEntity = true;
            Icon = icon ?? SystemIcons.Error;
            Trans = Matrix3d.Identity;
            CanShow = true;
            DefineStatus();
        }

        public Error(string message, ObjectId idEnt, Matrix3d trans, [CanBeNull] Icon icon = null)
        {
            _msg = PrepareMessage(message);
            _shortMsg = GetShortMsg(_msg);
            _idEnt = idEnt;
            HasEntity = true;
            Icon = icon ?? SystemIcons.Error;
            Trans = trans;
            CanShow = true;
            DefineStatus();
        }

        private Error([NotNull] Error err)
        {
            _msg = err._msg;
            Group = err.Group;
            _shortMsg = err._shortMsg;
            _idEnt = err._idEnt;
            _alreadyCalcExtents = err._alreadyCalcExtents;
            _isNullExtents = err._isNullExtents;
            _extents = err._extents;
            HasEntity = err.HasEntity;
            Icon = err.Icon;
            Trans = err.Trans;
            Tag = err.Tag;
            CanShow = err.CanShow;
            Status = err.Status;
        }

        public object Tag { get; set; }

        public Matrix3d Trans { get; set; }

        public string Message => _msg;

        public string Group { get; set; }

        public string ShortMsg => _shortMsg;

        public ObjectId IdEnt
        {
            get => _idEnt;
            set => _idEnt = value;
        }

        public bool HasEntity { get; set; }

        public Icon Icon { get; set; }

        public ErrorStatus Status { get; set; }

        public bool CanShow { get; set; }

        public List<Entity> Visuals { get; set; } = new List<Entity>();

        public List<ErrorAddButton> AddButtons { get; set; } = new List<ErrorAddButton>();

        public Color Background { get; set; }

        public Extents3d Extents
        {
            get
            {
                if (!_alreadyCalcExtents)
                {
                    _alreadyCalcExtents = true;
                    if (HasEntity)
                    {
                        using (var ent = _idEnt.Open(OpenMode.ForRead, false, true) as Entity)
                        {
                            if (ent != null)
                            {
                                try
                                {
                                    _extents = ent.GeometricExtents;
                                    _extents.TransformBy(Trans);
                                    _extents = _extents.Offset();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log.Error(ex, "AcadLib.Error.Extents ent.GeometricExtents;");
                                    _isNullExtents = true;
                                }
                            }
                        }
                    }
                }

                if (_isNullExtents)
                {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowAlertDialog("Границы объекта не определены.");
                }

                return _extents;
            }
            set
            {
                _extents = value;
                _alreadyCalcExtents = true;
            }
        }

        /// <summary>
        /// Сгруппированные ошибки по одинаковым сообщениям.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public static List<IError> GetCollapsedErrors([NotNull] List<IError> errors)
        {
            var errCounts = errors.GroupBy(e => e.Message).Select(g =>
            {
                var e = g.First().GetCopy();
                var err = e as Error;
                err?.SetCount(g.Count());
                return e;
            });
            return errCounts.ToList();
        }

        public virtual void Show()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    var ed = doc.Editor;
                    if (Extents.Diagonal() > 1)
                        ed.Zoom(Extents);
                    if (HasEntity)
                    {
                        // Проверка соответствия документа
                        if (IdEnt.Database == doc.Database)
                        {
                            IdEnt.FlickObjectHighlight(2, 60, 60);
                            Visuals.FlickObjectHighlight();
                            ed.SetImpliedSelection(new[] { IdEnt });
                        }
                        else MessageBox.Show($"Должен быть активен чертеж {IdEnt.Database.Filename}");
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        public int CompareTo([NotNull] IError other)
        {
            var res = Status.CompareTo(other.Status);
            return res != 0 ? res : string.CompareOrdinal(Message, other.Message);
        }

        public bool Equals(IError other)
        {
            return other != null && string.Equals(Message, other.Message);
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }

        [NotNull]
        public IError GetCopy()
        {
            return (IError)MemberwiseClone();
        }

        public void AdditionToMessage(string addMsg)
        {
            _msg += addMsg;
            _shortMsg = GetShortMsg(_msg);
        }

        public void AddButton(string title, string tooltip, [NotNull] Action click)
        {
            AcadHelper.InvokeInMainThread(() =>
            {
                AddButtons.Add(new ErrorAddButton
                {
                    Name = title,
                    Tooltip = tooltip,
                    Click = new RelayCommand(click)
                });                
            });
        }

        public void AddButtonShowEntity(ObjectId entId, string title, string tooltip)
        {
            AcadHelper.InvokeInMainThread(() =>
            {
                AddButtons.Add(new ErrorAddButton
                {
                    Name = title,
                    Tooltip = tooltip,
                    Click = new RelayCommand(() => entId.ShowEnt())
                });                
            });
        }

        [NotNull]
        private static string GetShortMsg([NotNull] string msg)
        {
            var resVal = msg.Length > 200 ? msg.Substring(0, 200) : msg;
            return resVal.Replace("\n", " ");
        }

        private string PrepareMessage(string message)
        {
            Group = message;
            return message; // .ClearString(); // делать очистку в момент создания ошибки при необходимости
        }

        private void SetCount(int v)
        {
            if (v > 1)
            {
                _shortMsg = $"{v}...{_shortMsg}";
            }
        }

        private void DefineStatus()
        {
            if (Icon == null)
                Icon = SystemIcons.Error;
            dictErrorIcons.TryGetValue(Icon, out var status);
            Status = status;
        }
    }
}