// ReSharper disable once CheckNamespace
namespace AcadLib.Geometry
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public class PolylineVertex
    {
        public PolylineVertex(string name, int index, Point2d pt)
        {
            Name = name;
            Index = index;
            Pt = pt;
        }

        public double Bulge { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public Point2d Pt { get; set; }

        [NotNull]
        public static List<PolylineVertex> GetVertexes([NotNull] Polyline pl, string name)
        {
            var res = new List<PolylineVertex>();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                var pt = pl.GetPoint2dAt(i);
                var bulge = pl.GetBulgeAt(i);
                res.Add(new PolylineVertex(name, i, pt) { Bulge = bulge });
            }

            return res;
        }
    }
}