namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    /// <summary>
    /// Описание атрибута
    /// Для AttributeDefinition или AttributeReference
    /// </summary>
    [PublicAPI]
    public class AttributeInfo
    {
        /// <summary>
        /// DBText - должен быть или AttributeDefinition или AttributeReference
        /// иначе исключение ArgumentException
        /// </summary>
        public AttributeInfo([NotNull] DBText attr)
        {
            if (attr is AttributeDefinition attdef)
            {
                Tag = attdef.Tag;
                IsAtrDefinition = true;
            }
            else
            {
                if (attr is AttributeReference attref)
                {
                    Tag = attref.Tag;
                }
                else
                {
                    throw new ArgumentException("requires an AttributeDefintion or AttributeReference");
                }
            }

            Text = attr.TextString;
            IdAtr = attr.Id;
        }

        public AttributeInfo([NotNull] AttributeReference atrRef)
        {
            Tag = atrRef.Tag;
            Text = atrRef.TextString;
            IdAtr = atrRef.Id;
        }

        public string Tag { get; set; }

        public string Text { get; set; }

        public ObjectId IdAtr { get; set; }

        public bool IsAtrDefinition { get; set; }

        [NotNull]
        public static List<AttributeInfo> GetAttrDefs(ObjectId idBtr)
        {
            var resVal = new List<AttributeInfo>();

            if (!idBtr.IsNull)
            {
                using (var btr = (BlockTableRecord)idBtr.Open(OpenMode.ForRead))
                {
                    foreach (var idEnt in btr)
                    {
                        using (var attrDef = (AttributeDefinition)idEnt.Open(OpenMode.ForRead, false, true))
                        {
                            if (attrDef != null && attrDef.Visible)
                            {
                                var attrDefInfo = new AttributeInfo(attrDef);
                                resVal.Add(attrDefInfo);
                            }
                        }
                    }
                }
            }

            return resVal;
        }

        [NotNull]
        [Obsolete("Use DisposableCol")]
        public static List<AttributeInfo> GetAttrRefs([CanBeNull] BlockReference blRef)
        {
            var resVal = new List<AttributeInfo>();
            if (blRef?.AttributeCollection != null)
            {
                foreach (ObjectId idAttrRef in blRef.AttributeCollection)
                {
                    if (!idAttrRef.IsValidEx())
                        continue;
                    using (var atrRef = (AttributeReference)idAttrRef.Open(OpenMode.ForRead, false, true))
                    {
                        if (atrRef.Visible)
                        {
                            var ai = new AttributeInfo(atrRef);
                            resVal.Add(ai);
                        }
                    }
                }
            }

            return resVal;
        }
    }
}
