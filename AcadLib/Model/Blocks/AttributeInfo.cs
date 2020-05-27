using System.Linq;

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
            if (attr is AttributeDefinition attDef)
            {
                Tag = attDef.Tag;
                IsAtrDefinition = true;
            }
            else
            {
                if (attr is AttributeReference attRef)
                {
                    Tag = attRef.Tag;
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

        public AttributeInfo([NotNull] AttributeDefinition atrDef)
        {
            Tag = atrDef.Tag;
            Text = atrDef.TextString;
            IdAtr = atrDef.Id;
            IsAtrDefinition = true;
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
#pragma warning disable 618
                using var btr = (BlockTableRecord)idBtr.Open(OpenMode.ForRead);
#pragma warning restore 618
                foreach (var idEnt in btr)
                {
#pragma warning disable 618
                    using var attrDef = (AttributeDefinition)idEnt.Open(OpenMode.ForRead, false, true);
#pragma warning restore 618
                    if (attrDef != null && attrDef.Visible)
                    {
                        var attrDefInfo = new AttributeInfo(attrDef);
                        resVal.Add(attrDefInfo);
                    }
                }
            }

            return resVal;
        }

        [NotNull]
        public static List<AttributeInfo> GetAttrRefs([CanBeNull] BlockReference blRef)
        {
            var resVal = new List<AttributeInfo>();
            if (blRef?.AttributeCollection != null)
            {
                foreach (ObjectId idAttrRef in blRef.AttributeCollection)
                {
                    if (!idAttrRef.IsValidEx())
                        continue;
#pragma warning disable 618
                    using var atrRef = (AttributeReference)idAttrRef.Open(OpenMode.ForRead, false, true);
#pragma warning restore 618
                    if (atrRef.Visible)
                    {
                        var ai = new AttributeInfo(atrRef);
                        resVal.Add(ai);
                    }
                }

#pragma warning disable 618
                using var btr = (BlockTableRecord)blRef.BlockTableRecord.Open(OpenMode.ForRead);
#pragma warning restore 618
                if (btr.HasAttributeDefinitions)
                {
                    foreach (var id in btr.Cast<ObjectId>().Where(i => i.ObjectClass == General.ClassAttDef))
                    {
#pragma warning disable 618
                        using var atrDef = (AttributeDefinition) id.Open(OpenMode.ForRead, false, true);
#pragma warning restore 618
                        if (!atrDef.Constant || !atrDef.Visible)
                            continue;
                        var ai = new AttributeInfo(atrDef);
                        resVal.Add(ai);
                    }
                }
            }

            return resVal;
        }
    }
}
