namespace AcadLib.Dim
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class DimensionExt
    {
        public static void SetDimBlk(string dimSysVar, DimBlkEnum type)
        {
            Application.SetSystemVariable(dimSysVar, GetDimBlkName(type));
        }

        public static ObjectId GetDimBlkObjectId([NotNull] this Database db, DimBlkEnum dimBlk)
        {
            var blkName = GetDimBlkName(dimBlk);
            using (var bt = db.BlockTableId.GetObjectT<BlockTable>())
            {
                if (!bt.Has(blkName))
                {
                    Application.SetSystemVariable("DIMBLK", blkName);
                }

                return bt[blkName];
            }
        }

        [NotNull]
        private static string GetDimBlkName(DimBlkEnum dimBlk)
        {
            switch (dimBlk)
            {
                case DimBlkEnum.FilledClosed: return string.Empty;
                case DimBlkEnum.DOT: return "_DOT";
                case DimBlkEnum.DOTSMALL: return "_DOTSMALL";
                case DimBlkEnum.DOTBLANK: return "_DOTBLANK";
                case DimBlkEnum.ORIGIN: return "_ORIGIN";
                case DimBlkEnum.ORIGIN2: return "_ORIGIN2";
                case DimBlkEnum.OPEN: return "_OPEN";
                case DimBlkEnum.OPEN90: return "_OPEN90";
                case DimBlkEnum.OPEN30: return "_OPEN30";
                case DimBlkEnum.CLOSED: return "_CLOSED";
                case DimBlkEnum.SMALL: return "_SMALL";
                case DimBlkEnum.NONE: return "_NONE";
                case DimBlkEnum.OBLIQUE: return "_OBLIQUE";
                case DimBlkEnum.BOXFILLED: return "_BOXFILLED";
                case DimBlkEnum.BOXBLANK: return "_BOXBLANK";
                case DimBlkEnum.CLOSEDBLANK: return "_CLOSEDBLANK";
                case DimBlkEnum.DATUMFILLED: return "_DATUMFILLED";
                case DimBlkEnum.DATUMBLANK: return "_DATUMBLANK";
                case DimBlkEnum.INTEGRAL: return "_INTEGRAL";
                case DimBlkEnum.ARCHTICK: return "_ARCHTICK";
                default: throw new ArgumentOutOfRangeException(nameof(dimBlk), dimBlk, null);
            }
        }
    }
}