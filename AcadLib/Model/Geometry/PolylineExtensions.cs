namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Exceptions;
    using JetBrains.Annotations;
    using NetLib;
    using Scale;

    /// <summary>
    /// Enumeration of offset side options
    /// </summary>
    public enum OffsetSide
    {
        In,
        Out,
        Left,
        Right,
        Both
    }

    /// <summary>
    /// Provides extension methods for the Polyline type.
    /// </summary>
    [PublicAPI]
    public static class PolylineExtensions
    {
        public enum PolygonValidateResult
        {
            OK = 0,
            NotClosed = 1,
            DuplicateVertices = 2,
            SelfIntersection = 4,
        }

        public static Point3d GetClosestVertex([NotNull] this Polyline pl, Point3d pt)
        {
            var closest = pl.GetClosestPointTo(pt, Vector3d.ZAxis, false);
            var parameter = pl.GetParameterAtPoint(closest);
            var index = (int)NetLib.DoubleExt.Round(parameter, 0);
            return pl.GetPoint3dAt(index);
        }

        /// <summary>
        /// Breaks the polyline at specified point.
        /// </summary>
        /// <param name="pline">The polyline this method applies to.</param>
        /// <param name="brkPt">The point where to break the polyline.</param>
        /// <returns>An array of the two resullting polylines.</returns>
        [NotNull]
        public static Polyline[] BreakAtPoint([NotNull] this Polyline pline, Point3d brkPt)
        {
            brkPt = pline.GetClosestPointTo(brkPt, false);

            // le point spécifié est sur le point de départ de la polyligne
            if (brkPt.IsEqualTo(pline.StartPoint))
                return new[] { null, (Polyline)pline.Clone() };

            // le point spécifié est sur le point de fin de la polyligne
            if (brkPt.IsEqualTo(pline.EndPoint))
                return new[] { (Polyline)pline.Clone(), null };

            var param = pline.GetParameterAtPoint(brkPt);
            var index = (int)param;
            var num = pline.NumberOfVertices;
            var pl1 = (Polyline)pline.Clone();
            if (pline.Closed)
            {
                pl1.AddVertexAt(
                    pline.NumberOfVertices,
                    pline.GetPoint2dAt(0),
                    pline.GetStartWidthAt(num - 1),
                    pline.GetEndWidthAt(num - 1),
                    pline.GetBulgeAt(num - 1));
                pl1.Closed = false;
            }

            var pl2 = (Polyline)pl1.Clone();

            // le point spécifié est sur un sommet de la polyligne
            if (Math.Abs(NetLib.DoubleExt.Round(param, 6) - index) < 0.0001)
            {
                for (var i = pl1.NumberOfVertices - 1; i > index; i--)
                {
                    pl1.RemoveVertexAt(i);
                }

                for (var i = 0; i < index; i++)
                {
                    pl2.RemoveVertexAt(0);
                }

                return new[] { pl1, pl2 };
            }

            // le point spécifié est sur un segment
            var pt = brkPt.Convert2d(new Plane(Point3d.Origin, pline.Normal));
            for (var i = pl1.NumberOfVertices - 1; i > index + 1; i--)
            {
                pl1.RemoveVertexAt(i);
            }

            pl1.SetPointAt(index + 1, pt);
            for (var i = 0; i < index; i++)
            {
                pl2.RemoveVertexAt(0);
            }

            pl2.SetPointAt(0, pt);
            if (Math.Abs(pline.GetBulgeAt(index)) > 0.0001)
            {
                var bulge = pline.GetBulgeAt(index);
                pl1.SetBulgeAt(index, MultiplyBulge(bulge, param - index));
                pl2.SetBulgeAt(0, MultiplyBulge(bulge, index + 1 - param));
            }

            return new[] { pl1, pl2 };
        }

        /// <summary>
        /// Gets the centroid of the polyline.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <returns>The centroid of the polyline (WCS coordinates).</returns>
        public static Point3d Centroid([NotNull] this Polyline pl)
        {
            return pl.Centroid2d().Convert3d(pl.Normal, pl.Elevation);
        }

        /// <summary>
        /// Gets the centroid of the polyline.
        /// </summary>
        /// <param name="pl">The instance to which the method applies.</param>
        /// <returns>The centroid of the polyline (OCS coordinates).</returns>
        public static Point2d Centroid2d([NotNull] this Polyline pl)
        {
            var cen = new Point2d();
            var tri = new Triangle2d();
            CircularArc2d arc;
            double tmpArea;
            var area = 0.0;
            var last = pl.NumberOfVertices - 1;
            var p0 = pl.GetPoint2dAt(0);
            var bulge = pl.GetBulgeAt(0);
            var pts = new List<Point2d> { p0 };

            if (Math.Abs(bulge) > 0.0001)
            {
                arc = pl.GetArcSegment2dAt(0);
                area = arc.AlgebricArea();
                cen = arc.Centroid() * area;
            }

            for (var i = 1; i < last; i++)
            {
                var pi = pl.GetPoint2dAt(i);
                pi.AddTo(pts);
                tri.Set(p0, pi, pl.GetPoint2dAt(i + 1));
                tmpArea = tri.AlgebricArea;
                cen += (tri.Centroid * tmpArea).GetAsVector();
                area += tmpArea;
                bulge = pl.GetBulgeAt(i);
                if (Math.Abs(bulge) > 0.0001)
                {
                    arc = pl.GetArcSegment2dAt(i);
                    tmpArea = arc.AlgebricArea();
                    area += tmpArea;
                    cen += (arc.Centroid() * tmpArea).GetAsVector();
                }
            }

            bulge = pl.GetBulgeAt(last);
            if (Math.Abs(bulge) > 0.0001 && pl.Closed)
            {
                arc = pl.GetArcSegment2dAt(last);
                tmpArea = arc.AlgebricArea();
                area += tmpArea;
                cen += (arc.Centroid() * tmpArea).GetAsVector();
            }

            if (Math.Abs(area) < 0.0001)
            {
                // Средняя точка из всех точек полилинии
                return new Point2d(pts.Average(a => a.X), pts.Average(a => a.Y));
            }

            return cen.DivideBy(area);
        }

        /// <summary>
        /// Проверка наличия самопересечений в полилинии
        /// True - нет самопересечений
        /// </summary>
        public static bool CheckCross(this Polyline pline)
        {
            using (var mpoly = new MPolygon())
            {
                var isValidBoundary = false;
                try
                {
                    mpoly.AppendLoopFromBoundary(pline, true, Tolerance.Global.EqualPoint);
                    if (mpoly.NumMPolygonLoops != 0)
                    {
                        isValidBoundary = true;
                    }
                }
                catch
                {
                    // Самопересечение
                }

                return isValidBoundary;
            }
        }

        [NotNull]
        public static Polyline CreatePolyline(this List<Point2d> pts)
        {
            pts = pts.DistinctPoints();
            return CreatePolyline(pts, true);
        }

        [NotNull]
        public static Polyline CreatePolyline(this List<Point3d> pts, bool closed = true, bool distinct = false)
        {
            var pts2D = pts.Select(s => s.Convert2d()).ToList();
            if (distinct) pts2D = pts2D.DistinctPoints();
            return CreatePolyline(pts2D, closed);
        }

        [NotNull]
        public static Polyline CreatePolyline(this List<Point2d> pts, bool closed)
        {
            var pl = new Polyline();
            for (var i = 0; i < pts.Count; i++)
            {
                pl.AddVertexAt(i, pts[i], 0, 0, 0);
            }

            if (closed)
                pl.Closed = true;
            return pl;
        }

        public static void Dispose([NotNull] this IEnumerable<Polyline> plines)
        {
            foreach (var pline in plines)
            {
                pline.Dispose();
            }
        }

        public static IEnumerable<Point2d> EnumeratePoints([NotNull] this Polyline pl)
        {
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                yield return pl.GetPoint2dAt(i);
            }
        }

        /// <summary>
        /// Adds an arc (fillet), if able, at each polyline vertex.
        /// </summary>
        /// <param name="pline">The instance to which the method applies.</param>
        /// <param name="radius">The arc radius.</param>
        public static void FilletAll([NotNull] this Polyline pline, double radius)
        {
            var n = pline.Closed ? 0 : 1;
            for (var i = n; i < pline.NumberOfVertices - n; i += 1 + pline.FilletAt(i, radius))
            {
            }
        }

        /// <summary>
        /// Adds an arc (fillet) at the specified vertex.
        /// </summary>
        /// <param name="pline">The instance to which the method applies.</param>
        /// <param name="index">The index of the vertex.</param>
        /// <param name="radius">The arc radius.</param>
        /// <returns>1 if the operation succeeded, 0 if it failed.</returns>
        public static int FilletAt([NotNull] this Polyline pline, int index, double radius)
        {
            var prev = index == 0 && pline.Closed ? pline.NumberOfVertices - 1 : index - 1;
            if (pline.GetSegmentType(prev) != SegmentType.Line ||
                pline.GetSegmentType(index) != SegmentType.Line)
            {
                return 0;
            }

            var seg1 = pline.GetLineSegment2dAt(prev);
            var seg2 = pline.GetLineSegment2dAt(index);
            var vec1 = seg1.StartPoint - seg1.EndPoint;
            var vec2 = seg2.EndPoint - seg2.StartPoint;
            var angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            var dist = radius * Math.Tan(angle);
            if (Math.Abs(dist) < 0.0001 || dist > seg1.Length || dist > seg2.Length)
            {
                return 0;
            }

            var pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            var pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            var bulge = Math.Tan(angle / 2.0);
            if (Clockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint))
            {
                bulge = -bulge;
            }

            pline.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            pline.SetPointAt(index + 1, pt2);
            return 1;
        }

        [NotNull]
        public static List<Point2d> GetApproximatePoints([NotNull] this Polyline pl, int arcDivisionCount)
        {
            if (!pl.HasBulges)
            {
                return pl.GetPoints();
            }

            var points = new List<Point2d>();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                points.Add(pl.GetPoint2dAt(i));
                var segType = pl.GetSegmentType(i);
                if (segType != SegmentType.Arc)
                    continue;
                var seg = pl.GetArcSegment2dAt(i);
                var arcPts = seg.GetSamplePoints(arcDivisionCount).ToList();
                arcPts = arcPts.Take(arcPts.Count - 1).Skip(1).ToList();
                points.AddRange(arcPts);
            }

            return points;
        }

        /// <summary>
        /// Апроксимация полилинии по высоте отступа сегментов от дуги
        /// </summary>
        /// <param name="pl">Полилиния</param>
        /// <param name="chordHeight">Высота отклонения от дуги</param>
        [NotNull]
        public static List<Point2d> GetApproximatePoints([NotNull] this Polyline pl, double chordHeight)
        {
            if (!pl.HasBulges)
            {
                return pl.GetPoints();
            }

            var points = new List<Point2d>();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                points.Add(pl.GetPoint2dAt(i));
                var segType = pl.GetSegmentType(i);
                if (segType != SegmentType.Arc)
                    continue;
                var seg = pl.GetArcSegment2dAt(i);
                var chordLength = GetChordLength(seg.Radius, chordHeight);
                var segLength = seg.GetLength(seg.GetParameterOf(seg.StartPoint), seg.GetParameterOf(seg.EndPoint));
                var numSeg = Convert.ToInt32(segLength / chordLength);
                var arcPts = seg.GetSamplePoints(numSeg).ToList();
                arcPts = arcPts.Take(arcPts.Count - 1).Skip(1).ToList();
                points.AddRange(arcPts);
            }

            return points;
        }

        /// <summary>
        /// Минимальное расстояние от точки до полилинии
        /// </summary>
        public static double GetClosesLengthToPoint([NotNull] this Polyline pl, Point3d pt)
        {
            var ptClosest = pl.GetClosestPointTo(pt, Vector3d.ZAxis, false);
            return (pt - ptClosest).Length;
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the curve along the given plane.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <returns>The projected polyline</returns>
        [CanBeNull]
        public static Polyline GetOrthoProjectedPolyline(this Polyline pline, [NotNull] Plane plane)
        {
            return pline.GetProjectedPolyline(plane, plane.Normal);
        }

        /// <summary>
        /// GetParameterAtPoint - или попытка корректировки точки с помощью GetClosestPointTo и вызов для скорректированной точки GetParameterAtPoint
        /// </summary>
        /// <param name="pl">Полилиния</param>
        /// <param name="pt">Точка</param>
        /// <param name="extend">Input whether or not to extend curve in search for nearest point.</param>
        /// <returns></returns>
        [Obsolete("Используй GetParameterAtPointTry с допуском.")]
        public static double GetParameterAtPointTry([NotNull] this Polyline pl, Point3d pt, bool extend = false)
        {
            try
            {
                return pl.GetParameterAtPoint(pt);
            }
            catch
            {
                var ptCorrect = pl.GetClosestPointTo(pt, Vector3d.ZAxis, extend);
                return pl.GetParameterAtPoint(ptCorrect);
            }
        }

        /// <summary>
        /// GetParameterAtPoint - или попытка корректировки точки с помощью GetClosestPointTo и вызов для скорректированной точки GetParameterAtPoint
        /// </summary>
        /// <param name="pl">Полилиния</param>
        /// <param name="pt">Точка</param>
        /// <param name="delta">Предельное отклонения</param>
        /// <param name="extend">Extend</param>
        /// <returns></returns>
        public static double? GetParameterAtPointTry([NotNull] this Polyline pl, Point3d pt, double delta, bool extend = false)
        {
            try
            {
                return pl.GetParameterAtPoint(pt);
            }
            catch
            {
                var ptCorrect = pl.GetClosestPointTo(pt, Vector3d.ZAxis, extend);
                if ((ptCorrect - pt).Length <= delta)
                    return pl.GetParameterAtPoint(ptCorrect);
            }

            return null;
        }

        [NotNull]
        public static List<Point2d> GetPoints([NotNull] this Polyline pl)
        {
            var points = new List<Point2d>();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                points.Add(pl.GetPoint2dAt(i));
            }

            return points;
        }

        /// <summary>
        /// Creates a new Polyline that is the result of projecting the Polyline parallel to 'direction' onto 'plane' and returns it.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection.</param>
        /// <returns>The projected Polyline.</returns>
        [CanBeNull]
        public static Polyline GetProjectedPolyline(this Polyline pline, [NotNull] Plane plane, Vector3d direction)
        {
            var tol = new Tolerance(1e-9, 1e-9);
            if (plane.Normal.IsPerpendicularTo(direction, tol))
                return null;

            if (pline.Normal.IsPerpendicularTo(direction, tol))
            {
                var dirPlane = new Plane(Point3d.Origin, direction);
                if (!pline.IsWriteEnabled)
                    pline = pline.UpgradeOpenTr();
                pline.TransformBy(Matrix3d.WorldToPlane(dirPlane));
                var extents = pline.GeometricExtents;
                pline.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
                return GeomExt.ProjectExtents(extents, plane, direction, dirPlane);
            }

            return GeomExt.ProjectPolyline(pline, plane, direction);
        }

        [NotNull]
        public static List<PolylineVertex> GetVertexes([NotNull] this Polyline pl)
        {
            return PolylineVertex.GetVertexes(pl, string.Empty);
        }

        /// <summary>
        /// Пересечение полилиний (штатный IntersectWith)
        /// </summary>
        [NotNull]
        public static List<Point3d> Intersects([NotNull] this Polyline pl1, Polyline pl2)
        {
            var pts = new Point3dCollection();
            pl1.IntersectWith(pl2, Intersect.OnBothOperands, new Plane(), pts, IntPtr.Zero, IntPtr.Zero);
            return pts.Cast<Point3d>().ToList();
        }

        /// <summary>
        /// Попадает ли точка внутрь полигона (все линейные сегменты)
        /// Но, работает быстрее, чем IsPointInsidePolyline. Примерно в 10раз.
        /// </summary>
        [Obsolete("Используй IsPointInsidePolyline(), подходящий для дуговых полилиний.")]
        public static bool IsPointInsidePolygon([NotNull] this Polyline polygon, Point3d pt)
        {
            var n = polygon.NumberOfVertices;
            double angle = 0;
            Point pt1, pt2;
            for (var i = 0; i < n; i++)
            {
                pt1.X = polygon.GetPoint2dAt(i).X - pt.X;
                pt1.Y = polygon.GetPoint2dAt(i).Y - pt.Y;
                pt2.X = polygon.GetPoint2dAt((i + 1) % n).X - pt.X;
                pt2.Y = polygon.GetPoint2dAt((i + 1) % n).Y - pt.Y;
                angle += Angle2D(pt1.X, pt1.Y, pt2.X, pt2.Y);
            }

            return !(Math.Abs(angle) < Math.PI);
        }

        /// <summary>
        /// Попадает ли точка внутрь полилинии.
        /// Предполагается, что полилиния имеет замкнутый контур.
        /// Подходит для полилиний с дуговыми сегментами.
        /// Работает медленнее чем IsPointInsidePolygon(), примерно в 10 раз.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="onIsInside">Если исходная точка лежит на полилинии, то считать, что она внутри или нет: True - внутри, False - снаружи.</param>
        /// <param name="pl"></param>
        /// <exception cref="Exceptions.ErrorException">Не удалось определить за несколько попыток.</exception>
        public static bool IsPointInsidePolyline([NotNull] this Polyline pl, Point3d pt, bool onIsInside = false)
        {
            using (var ray = new Ray())
            {
                ray.BasePoint = pt;
                var vec = new Vector3d(0, 1, pt.Z);
                ray.SecondPoint = pt + vec;
                using (var ptsIntersects = new Point3dCollection())
                {
                    bool isContinue;
                    var isPtOnPolyline = false;
                    var countWhile = 0;
                    do
                    {
                        using (var plane = new Plane())
                        {
                            pl.IntersectWith(ray, Intersect.OnBothOperands, plane, ptsIntersects, IntPtr.Zero, IntPtr.Zero);
                        }

                        isContinue = ptsIntersects.Cast<Point3d>().Any(p =>
                        {
                            if (pt.IsEqualTo(p))
                            {
                                isPtOnPolyline = true;
                                return true;
                            }
                            var param = pl.GetParameterAtPointTry(p);
                            return Math.Abs(param % 1) < 0.0001;
                        });

                        if (isPtOnPolyline)
                        {
                            return onIsInside;
                        }

                        if (isContinue)
                        {
                            vec = vec.RotateBy(0.01, Vector3d.ZAxis);
                            ray.SecondPoint = pt + vec;
                            ptsIntersects.Clear();
                            countWhile++;
                            if (countWhile > 3)
                            {
                                throw new ErrorException(new Errors.Error(
                                    "Не определено попадает ли точка внутрь полилинии.",
                                    pt.GetRectangleFromCenter(3), Matrix3d.Identity,
                                    System.Drawing.SystemIcons.Error));
                            }
                        }
                    } while (isContinue);
                    return NetLib.MathExt.IsOdd(ptsIntersects.Count);
                }
            }
        }

        /// <summary>
        /// Проверка попадания точки в полилинию, с допуском
        /// Попвдвет - если точка внутри или на полилинии.
        /// Способ - по нечетному кол-ву точек пересечения луча из точки с полилинией.
        /// </summary>
        public static bool IsPointInsidePolylineByRay(this Polyline pl, Point3d pt, Tolerance tolerance)
        {
            using (var ray = new Ray { BasePoint = pt, UnitDir = Vector3d.YAxis })
            {
                return CurveExt.IsPointInsidePolylineByRay(ray, pt, pl, tolerance);
            }
        }

        [Obsolete("Используй новую перегрузку с параметром допуска, она работает быстрее.")]
        public static bool IsPointOnPolyline([NotNull] this Polyline pl, Point3d pt)
        {
            var isOn = false;
            var ptZeroZ = new Point3d(pt.X, pt.Y, pl.Elevation);
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                Curve3d seg = null;

                var segType = pl.GetSegmentType(i);
                if (segType == SegmentType.Arc)
                    seg = pl.GetArcSegmentAt(i);
                else if (segType == SegmentType.Line)
                    seg = pl.GetLineSegmentAt(i);

                if (seg != null)
                {
                    isOn = seg.IsOn(ptZeroZ);
                    if (isOn)
                        break;
                }
            }

            return isOn;
        }

        /// <summary>
        /// Лежит ли точка на полилинии.
        /// Через GetClosestPointTo().
        /// </summary>
        /// <param name="pl">Полилиния</param>
        /// <param name="pt">Точка</param>
        /// <param name="gap">Допуск. Если 0, то используется Tolerance.Global</param>
        public static bool IsPointOnPolyline([NotNull] this Polyline pl, Point3d pt, double gap)
        {
            var tolerance = Math.Abs(gap) < 0.0001 ? Tolerance.Global : new Tolerance(gap, gap);
            return IsPointOnPolyline(pl, pt, tolerance);
        }

        public static bool IsPointOnPolyline([NotNull] this Polyline pl, Point3d pt, Tolerance tolerance)
        {
            var ptPl = pl.GetClosestPointTo(pt, Vector3d.ZAxis, false);
            return pt.IsEqualTo(ptPl, tolerance);
        }

        /// <summary>
        /// Проверка самопересечения полилинии
        /// </summary>
        public static PolygonValidateResult IsValidPolygon(
            [NotNull] this Polyline pline,
            [NotNull] out List<Point3d> intersections,
            double tolerance = 0.01)
        {
            intersections = new List<Point3d>();
            var result = PolygonValidateResult.OK;
            if (!pline.Closed)
            {
                result += 1;
            }

            var t = new Tolerance(tolerance, tolerance);
            using (var curve1 = pline.GetGeCurve(t))
            using (var curve2 = pline.GetGeCurve(t))
            using (var curveInter = new CurveCurveIntersector3d(curve1, curve2, pline.Normal, t))
            {
                var interCount = curveInter.NumberOfIntersectionPoints;
                var overlaps = curveInter.OverlapCount();
                if (!pline.Closed)
                    overlaps += 1;
                if (overlaps < pline.NumberOfVertices)
                    result += 2;
                if (interCount > overlaps)
                {
                    result += 4;
                    var plPts = pline.GetPoints().DistinctPoints().Select(s => s.Convert3d()).ToList();
                    for (var i = 0; i < interCount; i++)
                        intersections.Add(curveInter.GetIntersectionPoint(i));
                    var onlyIntersects = intersections.Except(plPts).ToList();
                    if (onlyIntersects.Any())
                    {
                        intersections = onlyIntersects;
                    }
                }
            }

            return result;
        }

        public static bool IsVertex([NotNull] this Polyline pl, Point3d pt, double tolerance = 0.0001)
        {
            return NetLib.MathExt.IsWholeNumber(pl.GetParameterAtPointTry(pt), tolerance);
        }

        /// <summary>
        /// Applies a factor to a polyline bulge.
        /// </summary>
        /// <param name="bulge">The bulge this method applies to.</param>
        /// <param name="factor">the factor to apply to the bulge.</param>
        /// <returns>The new bulge.</returns>
        public static double MultiplyBulge(double bulge, double factor)
        {
            return Math.Tan(Math.Atan(bulge) * factor);
        }

        /// <summary>
        /// Следующий индекс полилинии
        /// </summary>
        /// <param name="pl">Полилиния</param>
        /// <param name="index">Текущий индек</param>
        /// <param name="dir">Величина сдвига индекса. 1 - на ед вверх, -1 на ед. вниз, и т.д.</param>
        /// <returns>Текущий индекс + сдвиг. С проверкой попадания индекса в кол-во вершин полилинии.</returns>
        public static int NextVertexIndex([NotNull] this Polyline pl, int index, int dir = 1)
        {
            var res = index + dir;
            if (res < 0)
            {
                res = pl.NumberOfVertices - 1;
            }
            else if (res >= pl.NumberOfVertices)
            {
                res = 0;
            }

            return res;
        }

        /// <summary>
        /// Offset the source polyline to specified side(s).
        /// </summary>
        /// <param name="source">The polyline to be offseted.</param>
        /// <param name="offsetDist">The offset distance.</param>
        /// <param name="side">The offset side(s).</param>
        /// <returns>A polyline sequence resulting from the offset of the source polyline.</returns>
        [CanBeNull]
        public static IEnumerable<Polyline> Offset([NotNull] this Polyline source, double offsetDist, OffsetSide side)
        {
            offsetDist = Math.Abs(offsetDist);
            var offsetRight = source.GetOffsetCurves(offsetDist).Cast<Polyline>().ToList();
            var areaRight = offsetRight.Select(pline => pline.Area).Sum();
            var offsetLeft = source.GetOffsetCurves(-offsetDist).Cast<Polyline>().ToList();
            var areaLeft = offsetLeft.Select(pline => pline.Area).Sum();
            switch (side)
            {
                case OffsetSide.In:
                    if (areaRight < areaLeft)
                    {
                        offsetLeft.Dispose();
                        return offsetRight;
                    }
                    else
                    {
                        offsetRight.Dispose();
                        return offsetLeft;
                    }

                case OffsetSide.Out:
                    if (areaRight < areaLeft)
                    {
                        offsetRight.Dispose();
                        return offsetLeft;
                    }
                    else
                    {
                        offsetLeft.Dispose();
                        return offsetRight;
                    }

                case OffsetSide.Left:
                    offsetRight.Dispose();
                    return offsetLeft;

                case OffsetSide.Right:
                    offsetLeft.Dispose();
                    return offsetRight;

                case OffsetSide.Both:
                    return offsetRight.Concat(offsetLeft);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Подписывание вершин полилинии
        /// </summary>
        public static void TestDrawVertexNumbers([NotNull] this Polyline pl, Color color)
        {
            var scale = HostApplicationServices.WorkingDatabase.GetCurrentAnnoScale();
            var texts = new List<Entity>();
            for (var i = 0; i < pl.NumberOfVertices; i++)
            {
                var text = new DBText
                {
                    TextString = i.ToString(),
                    Position = pl.GetPoint2dAt(i).Convert3d(),
                    Height = 2.5 * scale,
                    Color = color
                };
                texts.Add(text);
            }

            texts.AddEntityToCurrentSpace();
        }

        public static void Wedding([NotNull] this Polyline pl, Tolerance tolerance)
        {
            Wedding(pl, tolerance, false);
        }

        public static void Wedding([NotNull] this Polyline pl, Tolerance tolerance, bool close = true, bool onSomeLine = false)
        {
            var prew = pl.GetPoint2dAt(0);
            for (var i = 1; i < pl.NumberOfVertices; i++)
            {
                var cur = pl.GetPoint2dAt(i);
                if (prew.IsEqualTo(cur, tolerance))
                {
                    if (pl.HasBulges)
                    {
                        var bulge = pl.GetBulgeAt(i);
                        pl.SetBulgeAt(pl.NextVertexIndex(i, -1), bulge);
                    }

                    pl.RemoveVertexAt(i--);
                }
                else if (onSomeLine && pl.NumberOfVertices > 2)
                {
                    // Проверка если точки на одной прямой
                    var iNext = pl.NextVertexIndex(i);
                    var next = pl.GetPoint2dAt(iNext);
                    if (!next.IsEqualTo(prew, tolerance) && IsPointsOnSomeLine(prew, cur, next, tolerance))
                    {
                        pl.RemoveVertexAt(i--);
                    }
                    else
                    {
                        prew = cur;
                    }
                }
                else
                {
                    prew = cur;
                }
            }

            if (close && !pl.Closed)
                pl.Closed = true;

            if (pl.Closed && onSomeLine && pl.NumberOfVertices >= 3)
            {
                // Проверить что первая точка лежит на одной прямой со 2 и последней точками
                prew = pl.GetPoint2dAt(pl.NumberOfVertices - 1);
                var cur = pl.GetPoint2dAt(0);
                var next = pl.GetPoint2dAt(1);
                if (IsPointsOnSomeLine(prew, cur, next, tolerance))
                {
                    pl.RemoveVertexAt(0);
                }
            }
        }

        private static double Angle2D(double x1, double y1, double x2, double y2)
        {
            var theta1 = Math.Atan2(y1, x1);
            var theta2 = Math.Atan2(y2, x2);
            var dtheta = theta2 - theta1;
            while (dtheta > Math.PI)
                dtheta -= Math.PI * 2;
            while (dtheta < -Math.PI)
                dtheta += Math.PI * 2;
            return dtheta;
        }

        /// <summary>
        /// Evaluates if the points are clockwise.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        /// <returns>True if points are clockwise, False otherwise.</returns>
        public static bool Clockwise(Point2d p1, Point2d p2, Point2d p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X) < 1e-8;
        }

        private static double GetChordLength(double r, double h)
        {
            return Math.Sqrt(r * r - (r - h) * (r - h)) * 2;
        }

        private static bool IsPointsOnSomeLine(Point2d pt1, Point2d pt2, Point2d pt3, Tolerance tolerance)
        {
            var vec1 = pt2 - pt1;
            var vec2 = pt3 - pt1;
            return vec1.IsParallelTo(vec2, tolerance);
        }

        private struct Point
        {
            public double X, Y;
        }
    }
}
