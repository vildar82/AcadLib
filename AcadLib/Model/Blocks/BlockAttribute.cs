namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    public class BlockAttribute
    {
        // Constructors
        public BlockAttribute(BlockReference br)
        {
            SetProperties(br);
        }

        public BlockAttribute(ObjectId idBlRef)
        {
            var db = idBlRef.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                SetProperties((BlockReference)tr.GetObject(idBlRef, OpenMode.ForRead));
            }
        }

        // Public read only properties
        public string Name { get; private set; }

        public Dictionary<string, string> Attributes { get; private set; }

        public string this[string key] => Attributes[key.ToUpper()];

        // Public method
        public new string ToString()
        {
            if (Attributes != null && Attributes.Count > 0)
                return $"{Name}: {Attributes.Select(a => $"{a.Key}={a.Value}").Aggregate((a, b) => $"{a}; {b}")}";
            return Name;
        }

        // Private method
        private void SetProperties(BlockReference br)
        {
            Name = br.GetEffectiveName();
            Attributes = new Dictionary<string, string>();
            br.AttributeCollection
                .GetObjects<AttributeReference>()
                .Iterate(att => Attributes.Add(att.Tag.ToUpper(), att.TextString));
        }
    }

    public class BlockAttributeEqualityComparer : IEqualityComparer<BlockAttribute>
    {
        public bool Equals(BlockAttribute x, BlockAttribute y)
        {
            if (x == null || y == null)
                return false;
            return x.Name.Equals(y.Name, StringComparison.CurrentCultureIgnoreCase) &&
                   x.Attributes.SequenceEqual(y.Attributes);
        }

        public int GetHashCode(BlockAttribute obj)
        {
            return base.GetHashCode();
        }
    }
}