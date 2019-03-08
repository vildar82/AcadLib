using System;

namespace AcadLib.XData
{
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public static class DicNODExt
    {
        [CanBeNull]
        public static DicED LoadDicNOD(this Database db, string dicName)
        {
            var nod = new DictNOD(dicName, db);
            return nod.LoadED();
        }

        [Obsolete("Что то напутано!!!")]
        public static void SaveDicNOD(this Database db, [NotNull] DicED dic)
        {
            var nod = new DictNOD(dic.Name, db);
            nod.Save(dic);
        }

        public static void SaveDicNOD2(this Database db, [NotNull] DicED dic)
        {
            var nod = new DictNOD(db);
            nod.Save(dic);
        }
    }
}
