using System.Collections.Generic;

namespace AcadLib
{
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class DrawOrderExt
    {
        public static void DrawOrder([NotNull] this BlockTableRecord btr, ObjectId top, ObjectId bot)
        {             
            var drawOrder = btr.DrawOrderTableId.GetObject(OpenMode.ForWrite) as DrawOrderTable;
            if (drawOrder == null)
                return;
            var idsAbove = new ObjectIdCollection { top };
            drawOrder.MoveAbove(idsAbove, bot);
        }

        public static void MoveTop(this ObjectId entId)
        {
            using (var ent = entId.Open(OpenMode.ForRead, false, true))
            using (var btr = ent.OwnerId.Open(OpenMode.ForRead, false, true) as BlockTableRecord)
            using (var order = btr.DrawOrderTableId.Open(OpenMode.ForWrite, false, true) as DrawOrderTable)
            {
                order.MoveToTop(new ObjectIdCollection(new[] {entId}));
            }
        }
        
        public static void MoveTop(this List<ObjectId> ids)
        {
            using (var ent = ids[0].Open(OpenMode.ForRead, false, true))
            using (var btr = ent.OwnerId.Open(OpenMode.ForRead, false, true) as BlockTableRecord)
            using (var order = btr.DrawOrderTableId.Open(OpenMode.ForWrite, false, true) as DrawOrderTable)
            {
                order.MoveToTop(new ObjectIdCollection(ids.ToArray()));
            }
        }
    }
}