using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NetLib;

namespace AcadLib.Blocks
{
    /// <summary>
    /// Вставка кучи блоков с дин параметрами - кэширование блоков с одинаковыми параметрами
    /// </summary>
    public class InsertManyDynBlocks : IDisposable
    {
        private readonly Dictionary<InsertData, BlockReference> _templates = new Dictionary<InsertData, BlockReference>(new InsertDataComparer());

        public BlockReference Insert(InsertData blData, Action<Exception, DynProp, BlockReference>? setDynPropException = null)
        {
            if (!_templates.TryGetValue(blData, out var template))
            {
                template = InsertTemplate(blData, setDynPropException);
                _templates.Add(blData, template);
            }

            return (BlockReference)template.Clone();
        }

        private BlockReference InsertTemplate(InsertData blData, Action<Exception, DynProp, BlockReference>? setDynPropException)
        {
            var blRef = BlockInsert.InsertBlockRef(blData.Btr, Point3d.Origin, blData.Owner, blData.Transaction, blData.Scale);
            var blBase = new BlockBase(blRef, blData.Btr.Name);

            foreach (var dynProp in blData.DynProps)
            {
                try
                {
                    blBase.FillPropValue(dynProp.Name, dynProp.Value, dynProp.ExactMatch, dynProp.IsRequired);
                }
                catch (Exception ex)
                {
                    setDynPropException?.Invoke(ex, dynProp, blRef);
                    ex.LogError();
                }
            }

            return blRef;
        }

        public void Dispose()
        {
            foreach (var item in _templates)
            {
                item.Value.Erase();
            }
        }
    }

    public class InsertDataComparer : IEqualityComparer<InsertData>
    {
        public bool Equals(InsertData i1, InsertData i2)
        {
            return i1.Btr.Id == i2.Btr.Id &&
                   i1.Owner.Id == i2.Owner.Id &&
                   i1.DynProps.EqualLists(i2.DynProps, new DynPropComparer());
        }

        public int GetHashCode(InsertData i)
        {
            return i.Btr.Id.GetHashCode();
        }
    }

    public class DynPropComparer : IEqualityComparer<DynProp>
    {
        public bool Equals(DynProp p1, DynProp p2)
        {
            return p1.Name.EqualsIgnoreCase(p2.Name) &&
                   Equals(p1.Value, p2.Value);
        }

        public int GetHashCode(DynProp prop)
        {
            return prop.Name.GetHashCode();
        }
    }

    public class InsertData
    {
        public BlockTableRecord Btr { get; set; }
        public Point3d Point { get; set; }
        public BlockTableRecord Owner { get; set; }
        public Transaction Transaction { get; set; }
        public double Scale { get; set; }
        public List<DynProp> DynProps { get; set; }
    }

    public class DynProp
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool ExactMatch { get; set; } = true;
        public bool IsRequired { get; set; } = false;
    }
}