namespace AcadLib.Blocks.Dublicate
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Errors;
    using Tree;

    /// <summary>
    /// Проверка наложения блоков в пространстве модели
    /// </summary>
    public static class CheckDublicateBlocks
    {
        public static int DEPTH = 5;
        private static List<IError> _errors = new List<IError>();
        private static HashSet<string>? _ignoreBlocks;
        private static List<BlockRefDublicateInfo> allDublicBlRefInfos = new List<BlockRefDublicateInfo>();
        private static HashSet<ObjectId> attemptedBlocks = new HashSet<ObjectId>();
        private static int curDepth;
        private static Dictionary<string, Dictionary<PointTree, List<BlockRefDublicateInfo>>> dictBlRefInfos = new Dictionary<string, Dictionary<PointTree, List<BlockRefDublicateInfo>>>();

        public static Tolerance Tolerance { get; set; } = new Tolerance(0.2, 10);

        public static void Check()
        {
            Check(null, null);
        }

        public static void Check(HashSet<string> ignoreBlocks)
        {
            Check(null, ignoreBlocks);
        }

        public static void Check(IEnumerable idsBlRefs)
        {
            Check(idsBlRefs, null);
        }

        public static void Check(IEnumerable? idsBlRefs, HashSet<string>? ignoreBlocks)
        {
            curDepth = 0;
            _ignoreBlocks = ignoreBlocks;
            var db = HostApplicationServices.WorkingDatabase;
            _errors = new List<IError>();
            attemptedBlocks = new HashSet<ObjectId>();
            allDublicBlRefInfos = new List<BlockRefDublicateInfo>();
            dictBlRefInfos = new Dictionary<string, Dictionary<PointTree, List<BlockRefDublicateInfo>>>();
            try
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    if (idsBlRefs == null)
                    {
                        var ms = (BlockTableRecord)SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject(OpenMode.ForRead);
                        idsBlRefs = ms;
                    }

                    GetDuplicateBlocks(idsBlRefs, Matrix3d.Identity, 0);
                    t.Commit();
                }

                // дублирующиеся блоки
                allDublicBlRefInfos = dictBlRefInfos.SelectMany(s => s.Value.Values).Where(w => w.Count > 1)
                    .SelectMany(s => s.GroupBy(g => g).Where(w => w.Skip(1).Any()))
                    .Select(s =>
                    {
                        var bi = s.First();
                        bi.CountDublic = s.Count();
                        bi.Dublicates = s.Skip(1).ToList();
                        return bi;
                    }).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"CheckDublicateBlocks - {db.Filename}. {ex.StackTrace}");
                return;
            }

            if (allDublicBlRefInfos.Count == 0)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nДубликаты блоков не найдены.");
            }
            else
            {
                foreach (var dublBlRefInfo in allDublicBlRefInfos)
                {
                    var err = new Error($"Дублирование блоков '{dublBlRefInfo.Name}' - " +
                                        $"{dublBlRefInfo.CountDublic} шт. в точке {dublBlRefInfo.Position.ToString()}",
                        dublBlRefInfo.IdBlRef,
                        dublBlRefInfo.TransformToModel,
                        System.Drawing.SystemIcons.Error)
                    {
                        Tag = dublBlRefInfo
                    };

                    _errors.Add(err);
                }
            }

            if (_errors.Count > 0)
            {
                if (Inspector.ShowDialog(_errors) != true)
                {
                    Inspector.Show(_errors);
                    throw new OperationCanceledException();
                }
            }
        }

        public static void DeleteDublicates(List<IError>? errors)
        {
            if (errors?.Any() != true)
                return;

            var blDuplicatesToDel = errors.Where(e => e.Tag is BlockRefDublicateInfo)
                .SelectMany(e => ((BlockRefDublicateInfo)e.Tag).Dublicates).ToList();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var countErased = 0;
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                if (t == null)
                    return;
                foreach (var dublBl in blDuplicatesToDel)
                {
                    var blTodel = dublBl.IdBlRef.GetObject<BlockReference>(OpenMode.ForWrite);
                    if (blTodel == null) continue;
                    blTodel.Erase();
                    countErased++;
                }

                $"Удалено {countErased} наложенных элементов.".WriteToCommandLine();
                t.Commit();
            }
        }

        private static void GetDuplicateBlocks(IEnumerable ids, Matrix3d transToModel, double rotate)
        {
            var idsBtrNext = new List<Tuple<ObjectId, Matrix3d, double>>();

            var isFirstDbo = true;

            foreach (var item in ids)
            {
                if (!(item is ObjectId))
                    continue;
                var idEnt = (ObjectId)item;
                if (!idEnt.IsValidEx())
                    continue;
                var dbo = idEnt.GetObject(OpenMode.ForRead, false, true);

                // Проверялся ли уже такое определение блока
                if (isFirstDbo)
                {
                    isFirstDbo = false;
                    if (!attemptedBlocks.Add(dbo.OwnerId))
                    {
                        continue;
                    }
                }

                var blRef = dbo as BlockReference;
                if (blRef == null || !blRef.Visible)
                    continue;
                var blRefInfo = new BlockRefDublicateInfo(blRef, transToModel, rotate);

                if (_ignoreBlocks?.Contains(blRefInfo.Name, StringComparer.OrdinalIgnoreCase) == true)
                {
                    continue;
                }

                var ptTree = new PointTree(blRefInfo.Position.X, blRefInfo.Position.Y);

                if (!dictBlRefInfos.TryGetValue(blRefInfo.Name, out var dictPointsBlInfos))
                {
                    dictPointsBlInfos = new Dictionary<PointTree, List<BlockRefDublicateInfo>>();
                    dictBlRefInfos.Add(blRefInfo.Name, dictPointsBlInfos);
                }

                if (!dictPointsBlInfos.TryGetValue(ptTree, out var listBiAtPoint))
                {
                    listBiAtPoint = new List<BlockRefDublicateInfo>();
                    dictPointsBlInfos.Add(ptTree, listBiAtPoint);
                }

                listBiAtPoint.Add(blRefInfo);

                idsBtrNext.Add(new Tuple<ObjectId, Matrix3d, double>(
                    blRef.BlockTableRecord, blRef.BlockTransform * transToModel, blRef.Rotation + rotate));
            }

            // Нырок глубже
            if (curDepth < DEPTH)
            {
                curDepth++;
                foreach (var btrNext in idsBtrNext)
                {
                    var btr = (BlockTableRecord)btrNext.Item1.GetObject(OpenMode.ForRead);
                    GetDuplicateBlocks(btr, btrNext.Item2, btrNext.Item3);
                }
            }
        }
    }
}
