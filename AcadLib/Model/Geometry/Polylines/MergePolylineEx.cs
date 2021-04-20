namespace AcadLib.Geometry.Polylines
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Comparers;

    public class MergePolylineEx
    {
        public List<Polyline> Merge(
            List<Polyline> pls,
            Point2dEqualityComparer pointComparer,
            IEqualityComparer<Polyline> comparer = null)
        {
            var joinedPls = new List<Polyline>();
            var plsCopy = pls.ToList();
            var joinPl = GetFirstPolyline(plsCopy);
            joinedPls.Add(joinPl);
            while (true)
            {
                if (joinPl == null)
                    break;
                Add(plsCopy, joinPl, joinPl.StartPoint);
            }

            return joinedPls;
        }

        private static Polyline? GetFirstPolyline(List<Polyline> pls)
        {
            if (pls.Any())
            {
                var pl = pls[0];
                pls.Remove(pl);
                return pl;
            }

            return null;
        }

        private void Add(List<Polyline> plsCopy, Polyline joinPl, Point3d ptJoin)
        {
            // TODO Merge Polyline
        }
    }
}