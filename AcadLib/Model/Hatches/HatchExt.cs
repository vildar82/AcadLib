namespace AcadLib.Hatches
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Errors;
    using Geometry;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class HatchExt
    {
        /// <summary>
        /// Создание ассоциативной штриховки по полилинии
        /// Полилиния должна быть в базе чертежа
        /// </summary>
        [CanBeNull]
        public static Hatch CreateAssociativeHatch(
            [NotNull] Curve loop,
            [NotNull] BlockTableRecord cs,
            [NotNull] Transaction t,
            string pattern = "SOLID",
            [CanBeNull] string layer = null,
            LineWeight lw = LineWeight.LineWeight015)
        {
            return CreateAssociativeHatch(loop, cs, t, 1, pattern, layer, lw);
        }

        /// <summary>
        /// Создание ассоциативной штриховки по полилинии
        /// Полилиния должна быть в базе чертежа
        /// </summary>
        [CanBeNull]
        public static Hatch CreateAssociativeHatch(
            [NotNull] Curve loop,
            [NotNull] BlockTableRecord cs,
            [NotNull] Transaction t,
            double scale,
            string pattern = "SOLID",
            [CanBeNull] string layer = null,
            LineWeight lw = LineWeight.LineWeight015)
        {
            var h = new Hatch();
            if (layer != null)
            {
                Layers.LayerExt.CheckLayerState(layer);
                h.Layer = layer;
            }

            h.LineWeight = lw;
            h.Linetype = SymbolUtilityServices.LinetypeContinuousName;
            h.PatternScale = scale;
            h.SetHatchPattern(HatchPatternType.PreDefined, pattern);
            cs.AppendEntity(h);
            t.AddNewlyCreatedDBObject(h, true);
            h.Associative = true;
            h.HatchStyle = HatchStyle.Normal;

            // добавление контура полилинии в гштриховку
            var ids = new ObjectIdCollection { loop.Id };
            try
            {
                h.AppendLoop(HatchLoopTypes.Default, ids);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"CreateAssociativeHatch");
                h.Erase();
                return null;
            }

            h.EvaluateHatch(true);
            var orders = (DrawOrderTable)cs.DrawOrderTableId.GetObject(OpenMode.ForWrite);
            orders.MoveToBottom(new ObjectIdCollection(new[] { h.Id }));
            return h;
        }

        [NotNull]
        public static Hatch CreateHatch(this List<Point2d> pts)
        {
            pts = pts.DistinctPoints();
            var ptCol = new Point2dCollection(pts.ToArray()) { pts[0] };
            var dCol = new DoubleCollection(new double[pts.Count]);
            var h = new Hatch();
            h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            h.AppendLoop(HatchLoopTypes.Default, ptCol, dCol);
            h.EvaluateHatch(false);
            return h;
        }

        [CanBeNull]
        public static Hatch CreateHatch([CanBeNull] this List<PolylineVertex> pts)
        {
            if (pts?.Any() != true)
                return null;
            if (!pts[0].Pt.IsEqualTo(pts[pts.Count - 1].Pt))
            {
                pts.Add(pts[0]);
            }

            var ptCol = new Point2dCollection(pts.Select(s => s.Pt).ToArray());
            var dCol = new DoubleCollection(pts.Select(s => s.Bulge).ToArray());
            var h = new Hatch();
            h.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            h.AppendLoop(HatchLoopTypes.Default, ptCol, dCol);
            h.EvaluateHatch(false);
            return h;
        }

        [NotNull]
        public static Hatch CreateHatch([NotNull] this List<Point3d> pts)
        {
            return CreateHatch(pts.ConvertAll(Point3dExtensions.Convert2d));
        }

        /// <summary>
        /// Создание штриховки по точкам полилинии
        /// </summary>
        [CanBeNull]
        public static Hatch CreateHatch([CanBeNull] this Polyline pl)
        {
            if (pl == null)
                return null;
            var vertexes = pl.GetVertexes();
            return CreateHatch(vertexes);
        }

        public static double GetHatchArea([NotNull] this Hatch hatch)
        {
            double area = 0;
            try
            {
                area = hatch.Area;
            }
            catch
            {
                var nLoop = hatch.NumberOfLoops;
                for (var i = 0; i < nLoop; i++)
                {
                    double looparea = 0;
                    var loopType = (int)hatch.LoopTypeAt(i);
                    if ((loopType & (int)HatchLoopTypes.Polyline) > 0)
                    {
                        var hatchLoop = hatch.GetLoopAt(i);
                        var bulgeVertex = hatchLoop.Polyline;
                        using (var pPoly = new Polyline(bulgeVertex.Count))
                        {
                            for (var j = 0; j < bulgeVertex.Count; j++)
                            {
                                pPoly.AddVertexAt(j, bulgeVertex[j].Vertex, bulgeVertex[j].Bulge, 0, 0);
                            }

                            pPoly.Closed = (loopType & (int)HatchLoopTypes.NotClosed) == 0;
                            looparea = pPoly.Area;
                            if ((loopType & (int)HatchLoopTypes.External) > 0)
                                area += Math.Abs(looparea);
                            else
                                area -= Math.Abs(looparea);
                        }
                    }
                    else
                    {
                        var hatchLoop = hatch.GetLoopAt(i);
                        var cur2ds = new Curve2d[hatchLoop.Curves.Count];
                        hatchLoop.Curves.CopyTo(cur2ds, 0);
                        using (var compCurve = new CompositeCurve2d(cur2ds))
                        {
                            var interval = compCurve.GetInterval();
                            double dMin = interval.GetBounds()[0], dMax = interval.GetBounds()[1];
                            if (Math.Abs(dMax - dMin) > 1e-6)
                            {
                                try
                                {
                                    looparea = compCurve.GetArea(dMin, dMax);
                                    if ((loopType & (int)HatchLoopTypes.External) > 0)
                                        area += Math.Abs(looparea);
                                    else
                                        area -= Math.Abs(looparea);
                                }
                                catch
                                {
                                    // Разбиваем кривую на 1000000 точек. Надеюсь, что такой точности
                                    // будет достаточно.
                                    var pts = compCurve.GetSamplePoints(1000);
                                    var np = pts.Length;
                                    for (var j = 0; j < np; j++)
                                    {
                                        looparea += 0.5 * pts[j].X * (pts[(j + 1) % np].Y - pts[(j + np - 1) % np].Y);
                                    }

                                    if ((loopType & (int)HatchLoopTypes.External) > 0)
                                        area += Math.Abs(looparea);
                                    else
                                        area -= Math.Abs(looparea);
                                }
                            }
                        }
                    }
                }
            }

            return Math.Abs(area);
        }

        [CanBeNull]
        public static HatchOptions GetHatchOptions([CanBeNull] this Hatch h)
        {
            return h == null ? null : new HatchOptions(h);
        }

        /// <summary>
        /// Полилинии в штриховке
        /// </summary>
        /// <param name="ht">Штриховка</param>
        /// <param name="loopType">Из каких типов островков</param>
        [NotNull]
        public static DisposableSet<Polyline> GetPolylines(
            [NotNull] this Hatch ht,
            HatchLoopTypes loopType = HatchLoopTypes.External)
        {
            var loops = GetPolylines2(ht, Tolerance.Global, loopType);
            var res = new DisposableSet<Polyline>(loops.Select(s => s.GetPolyline()));
            loops.Clear();
            return res;
        }

        [NotNull]
        public static DisposableSet<HatchLoopPl> GetPolylines2(
            [NotNull] this Hatch ht,
            Tolerance weddingTolerance,
            HatchLoopTypes loopType = (HatchLoopTypes)119,
            bool wedding = false)
        {
            var loops = new DisposableSet<HatchLoopPl>();
            var nloops = ht.NumberOfLoops;
            for (var i = 0; i < nloops; i++)
            {
                var loop = ht.GetLoopAt(i);
                if (loopType.HasAny(loop.LoopType))
                {
                    Debug.WriteLine($"GetPolylines2 HasFlag {loop.LoopType}!");
                    var poly = new Polyline();
                    var vertex = 0;
                    if (loop.IsPolyline)
                    {
                        foreach (BulgeVertex bv in loop.Polyline)
                        {
                            poly.AddVertexAt(vertex++, bv.Vertex, bv.Bulge, 0.0, 0.0);
                        }
                    }
                    else
                    {
                        foreach (Curve2d curve in loop.Curves)
                        {
                            if (curve is LinearEntity2d l)
                            {
                                if (NeedAddVertexToPl(poly, vertex - 1, l.StartPoint, weddingTolerance))
                                {
                                    poly.AddVertexAt(vertex++, l.StartPoint, 0, 0, 0);
                                }

                                poly.AddVertexAt(vertex++, l.EndPoint, 0, 0, 0);
                            }
                            else if (curve is CircularArc2d arc)
                            {
                                if (arc.IsCircle())
                                {
                                    loops.Add(new HatchLoopPl { Loop = arc.CreateCircle(), Types = loop.LoopType });
                                    continue;
                                }

                                var bulge = arc.GetBulge(arc.IsClockWise);
                                if (NeedAddVertexToPl(poly, vertex - 1, arc.StartPoint, weddingTolerance))
                                {
                                    poly.AddVertexAt(vertex++, arc.StartPoint, bulge, 0, 0);
                                }
                                else
                                {
                                    poly.SetBulgeAt(vertex - 1, bulge);
                                }

                                poly.AddVertexAt(vertex++, arc.EndPoint, 0, 0, 0);
                            }
                            else
                            {
                                Inspector.AddError($"Тип сегмента штриховки не поддерживается {curve}", ht);
                            }
                        }
                    }

                    if (poly.NumberOfVertices != 0)
                    {
                        if (wedding)
                        {
                            poly.Wedding(weddingTolerance);
                        }

                        if (!poly.Closed)
                            poly.Closed = true;
                        loops.Add(new HatchLoopPl { Loop = poly, Types = loop.LoopType });
                    }
                }
            }

            return loops;
        }

        public static void SetHatchOptions([CanBeNull] this Hatch h, [CanBeNull] HatchOptions opt)
        {
            if (h == null || opt == null)
                return;
            opt.SetOptions(h);
        }

        private static bool NeedAddVertexToPl(Polyline poly, int prewVertex, Point2d vertex, Tolerance tolerance)
        {
            return prewVertex <= 0 || !poly.GetPoint2dAt(prewVertex - 1).IsEqualTo(vertex, tolerance);
        }
    }
}
