namespace AcadLib
{
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public static class CleanExt
    {
        public static int CleanZombieBlock([NotNull] this Database db)
        {
            var countZombie = 0;
            using (var t = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                foreach (var idBtr in bt)
                {
                    var btr = (BlockTableRecord)idBtr.GetObject(OpenMode.ForRead);
                    if (!btr.IsLayout && btr.IsAnonymous && !btr.IsDynamicBlock && btr.Name.StartsWith("*U"))
                    {
                        var idBlRefs = btr.GetBlockReferenceIds(true, false);
                        if (idBlRefs.Count == 0)
                            continue;
                        var isZombie = true;
                        foreach (ObjectId idBlRef in idBlRefs)
                        {
                            var blRef = (BlockReference)idBlRef.GetObject(OpenMode.ForWrite, false, true);
                            if (!blRef.AnonymousBlockTableRecord.IsNull)
                            {
                                isZombie = false;
                                break;
                            }

                            blRef.Erase();
                            countZombie++;
                        }

                        if (isZombie)
                        {
                            btr = btr.Id.GetObject<BlockTableRecord>(OpenMode.ForWrite);
                            if (btr != null)
                                btr.Erase();
                        }
                    }
                }

                t.Commit();
            }

            return countZombie;
        }
    }
}