namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class AttSyncExt
    {
        public static void SynchronizeAttributes(this ObjectId btrId)
        {
            using var t = btrId.Database.TransactionManager.StartTransaction();
            var btr = btrId.GetObject<BlockTableRecord>();
            btr.SynchronizeAttributes();
            t.Commit();
        }

        /// <summary>
        /// Синхронизация атрибутов блока. Требуется запущенная транзакция
        /// </summary>
        public static void SynchronizeAttributes([NotNull] this BlockTableRecord target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            var tr = target.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.NoActiveTransactions);
            var attDefs = target.GetAttributes();
            foreach (ObjectId id in target.GetBlockReferenceIds(true, false))
            {
                var br = id.GetObjectT<BlockReference>();
                br.ResetAttributes(attDefs, tr);
            }

            if (target.IsDynamicBlock)
            {
                target.UpdateAnonymousBlocks();
                foreach (ObjectId id in target.GetAnonymousBlockIds())
                {
                    var btr = id.GetObjectT<BlockTableRecord>();
                    attDefs = btr.GetAttributes();
                    foreach (ObjectId brId in btr.GetBlockReferenceIds(true, false))
                    {
                        var br = brId.GetObject<BlockReference>(OpenMode.ForWrite);
                        br?.ResetAttributes(attDefs, tr);
                    }
                }
            }
        }

        [NotNull]
        private static List<AttributeDefinition> GetAttributes([NotNull] this BlockTableRecord target)
        {
            var attDefs = new List<AttributeDefinition>();
            foreach (var id in target)
            {
                if (id.ObjectClass == General.ClassAttDef)
                {
                    var attDef = id.GetObject<AttributeDefinition>();
                    attDefs.Add(attDef);
                }
            }

            return attDefs;
        }

        private static void ResetAttributes(
            [NotNull] this BlockReference br,
            [NotNull] List<AttributeDefinition> attDefs,
            Transaction tr)
        {
            var attValues = new Dictionary<string, string>();
            foreach (ObjectId id in br.AttributeCollection)
            {
                if (!id.IsErased)
                {
                    var attRef = id.GetObject<AttributeReference>(OpenMode.ForWrite);
                    if (attRef == null) continue;
                    attValues.Add(attRef.Tag,
                        attRef.IsMTextAttribute ? attRef.MTextAttribute.Contents : attRef.TextString);
                    attRef.Erase();
                }
            }

            foreach (var attDef in attDefs)
            {
                var attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                if (attDef.Constant)
                {
                    attRef.TextString = attDef.IsMTextAttributeDefinition
                        ? attDef.MTextAttributeDefinition.Contents
                        : attDef.TextString;
                }
                else if (attValues.ContainsKey(attRef.Tag))
                {
                    attRef.TextString = attValues[attRef.Tag];
                }

                br.AttributeCollection.AppendAttribute(attRef);
                tr.AddNewlyCreatedDBObject(attRef, true);
            }
        }
    }
}
