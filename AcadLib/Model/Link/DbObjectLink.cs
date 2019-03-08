namespace AcadLib.Link
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public enum LinkCode
    {
        SoftPointer,
        HardPointer
    }

    [PublicAPI]
    public static class DbObjectLink
    {
        private const string HardPointerRecordName = "HardPointer";
        private const string SoftPointerRecordName = "SoftPointer";

        public static void WriteLink(this ObjectId objId, ObjectId linkId, LinkCode code)
        {
            WriteLinks(objId, new List<ObjectId> { linkId }, code);
        }

        /// <summary>
        /// Записать связь в объект objId с объектом writedId
        /// </summary>
        /// <param name="objId">Объект</param>
        /// <param name="linkIds">Связываемые объекты</param>
        /// <param name="code">Тип связи</param>
        /// /// <param name="replace">Перезаписать существующий словарь LinkCode - true, или добавить если уже существует - false</param>
        public static void WriteLinks(this ObjectId objId, List<ObjectId> linkIds, LinkCode code, bool replace = true)
        {
            var dictId = GetExtDict(objId);
            var existLinks = ReadLinks(objId, code);
            var addLinkIds = linkIds?.Where(w => !w.IsNull).Except(existLinks).ToList() ?? new List<ObjectId>();
            var dxfCode = GetDxfCode(code);
            using (var dict = dictId.Open(OpenMode.ForWrite) as DBDictionary)
            {
                var entryName = GetLinkRecordName(code);
                if (dict.Contains(entryName) && replace)
                {
                    dict.Remove(entryName);
                }

                if (!addLinkIds.Any())
                    return;
                using (var xrec = new Xrecord())
                using (var resBuff = new ResultBuffer())
                {
                    foreach (var addLinkId in addLinkIds)
                    {
                        resBuff.Add(new TypedValue((int) dxfCode, addLinkId));
                    }

                    xrec.Data = resBuff;
                    dict.SetAt(entryName, xrec);
                }
            }
        }

        [NotNull]
        public static List<ObjectId> ReadLinks(this ObjectId objId, LinkCode code)
        {
            var dictId = ObjectId.Null;
            using (var obj = objId.Open(OpenMode.ForRead, true, true))
            {
                dictId = obj.ExtensionDictionary;
            }

            if (!dictId.IsValid)
                return new List<ObjectId>();
            var entryName = GetLinkRecordName(code);

            var xrecId = ObjectId.Null;
            using (var dict = dictId.Open(OpenMode.ForRead) as DBDictionary)
            {
                if (dict.Contains(entryName))
                {
                    xrecId = dict.GetAt(entryName);
                }
            }

            if (!xrecId.IsValid)
                return new List<ObjectId>();

            using (var xrec = xrecId.Open(OpenMode.ForRead) as Xrecord)
            using (var resBuf = xrec.Data)
            {
                TypedValue[] tVals;
                if (resBuf != null && (tVals = resBuf.AsArray()).Length > 0)
                {
                    var dxfCode = GetDxfCode(code);
                    var vals = tVals.Where(item => item.TypeCode == (int)dxfCode).Select(s => s.Value).OfType<ObjectId>().ToList();
                    return vals;
                }
            }

            return new List<ObjectId>();
        }

        private static DxfCode GetDxfCode(LinkCode code)
        {
            switch (code)
            {
                case LinkCode.SoftPointer:
                    return DxfCode.SoftPointerId;
                case LinkCode.HardPointer:
                    return DxfCode.HardPointerId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }

        private static string GetLinkRecordName(LinkCode code)
        {
            switch (code)
            {
                case LinkCode.HardPointer:
                    return HardPointerRecordName;
                case LinkCode.SoftPointer:
                    return SoftPointerRecordName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }

        private static ObjectId GetExtDict(ObjectId objId)
        {
            using (var obj = objId.Open(OpenMode.ForRead))
            {
                var dictId = obj.ExtensionDictionary;
                if (!dictId.IsValid)
                {
                    obj.UpgradeOpen();
                    obj.CreateExtensionDictionary();
                    dictId = obj.ExtensionDictionary;
                }

                return dictId;
            }
        }
    }
}
