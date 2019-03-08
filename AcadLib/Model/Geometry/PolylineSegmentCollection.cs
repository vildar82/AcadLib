namespace AcadLib.Geometry
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a PolylineSegment collection.
    /// </summary>
    [PublicAPI]
    public class PolylineSegmentCollection : IList<PolylineSegment>
    {
        private List<PolylineSegment> _contents = new List<PolylineSegment>();

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection.
        /// </summary>
        public PolylineSegmentCollection()
        {
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection from a PolylineSegment collection (IEnumerable).
        /// </summary>
        /// <param name="segments">A PolylineSegment collection.</param>
        public PolylineSegmentCollection([NotNull] IEnumerable<PolylineSegment> segments)
        {
            _contents.AddRange(segments);
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection from a PolylineSegment array.
        /// </summary>
        /// <param name="segments">A PolylineSegment array.</param>
        public PolylineSegmentCollection([NotNull] params PolylineSegment[] segments)
        {
            _contents.AddRange(segments);
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection from a Polyline.
        /// </summary>
        /// <param name="pline">A Polyline instance.</param>
        public PolylineSegmentCollection([NotNull] Polyline pline)
        {
            var n = pline.NumberOfVertices - 1;
            for (var i = 0; i < n; i++)
            {
                _contents.Add(new PolylineSegment(
                    pline.GetPoint2dAt(i),
                    pline.GetPoint2dAt(i + 1),
                    pline.GetBulgeAt(i),
                    pline.GetStartWidthAt(i),
                    pline.GetEndWidthAt(i)));
            }

            if (pline.Closed)
            {
                _contents.Add(new PolylineSegment(
                    pline.GetPoint2dAt(n),
                    pline.GetPoint2dAt(0),
                    pline.GetBulgeAt(n),
                    pline.GetStartWidthAt(n),
                    pline.GetEndWidthAt(n)));
            }
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection from a Polyline2d.
        /// </summary>
        /// <param name="pline">A Polyline2d instance.</param>
        public PolylineSegmentCollection([NotNull] Polyline2d pline)
        {
            var vertices = pline.GetVertices().ToArray();
            var n = vertices.Length - 1;
            for (var i = 0; i < n; i++)
            {
                var vertex = vertices[i];
                _contents.Add(new PolylineSegment(
                    vertex.Position.Convert2d(),
                    vertices[i + 1].Position.Convert2d(),
                    vertex.Bulge,
                    vertex.StartWidth,
                    vertex.EndWidth));
            }

            if (pline.Closed)
            {
                var vertex = vertices[n];
                _contents.Add(new PolylineSegment(
                    vertex.Position.Convert2d(),
                    vertices[0].Position.Convert2d(),
                    vertex.Bulge,
                    vertex.StartWidth,
                    vertex.EndWidth));
            }
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection from a Circle.
        /// </summary>
        /// <param name="circle">A Circle instance.</param>
        public PolylineSegmentCollection([NotNull] Circle circle)
        {
            var plane = new Plane(Point3d.Origin, circle.Normal);
            var cen = circle.Center.Convert2d(plane);
            var vec = new Vector2d(circle.Radius, 0.0);
            _contents.Add(new PolylineSegment(cen + vec, cen - vec, 1.0));
            _contents.Add(new PolylineSegment(cen - vec, cen + vec, 1.0));
        }

        /// <summary>
        /// Creates a new instance of PolylineSegmentCollection from an Ellipse.
        /// </summary>
        /// <param name="ellipse">An Ellipse instance.</param>
        public PolylineSegmentCollection([NotNull] Ellipse ellipse)
        {
            // PolylineSegmentCollection figuring the closed ellipse
            const double pi = Math.PI;
            var plane = new Plane(Point3d.Origin, ellipse.Normal);
            var cen3d = ellipse.Center;
            var pt3d0 = cen3d + ellipse.MajorAxis;
            var pt3d4 = cen3d + ellipse.MinorAxis;
            var pt3d2 = ellipse.GetPointAtParameter(pi / 4.0);
            var cen = cen3d.Convert2d(plane);
            var pt0 = pt3d0.Convert2d(plane);
            var pt2 = pt3d2.Convert2d(plane);
            var pt4 = pt3d4.Convert2d(plane);
            var line01 = new Line2d(pt0, (pt4 - cen).GetNormal() + (pt2 - pt0).GetNormal());
            var line21 = new Line2d(pt2, (pt0 - pt4).GetNormal() + (pt0 - pt2).GetNormal());
            var line23 = new Line2d(pt2, (pt4 - pt0).GetNormal() + (pt4 - pt2).GetNormal());
            var line43 = new Line2d(pt4, (pt0 - cen).GetNormal() + (pt2 - pt4).GetNormal());
            var majAx = new Line2d(cen, pt0);
            var minAx = new Line2d(cen, pt4);
            var pt1 = line01.IntersectWith(line21)[0];
            var pt3 = line23.IntersectWith(line43)[0];
            var pt5 = pt3.TransformBy(Matrix2d.Mirroring(minAx));
            var pt6 = pt2.TransformBy(Matrix2d.Mirroring(minAx));
            var pt7 = pt1.TransformBy(Matrix2d.Mirroring(minAx));
            var pt8 = pt0.TransformBy(Matrix2d.Mirroring(minAx));
            var pt9 = pt7.TransformBy(Matrix2d.Mirroring(majAx));
            var pt10 = pt6.TransformBy(Matrix2d.Mirroring(majAx));
            var pt11 = pt5.TransformBy(Matrix2d.Mirroring(majAx));
            var pt12 = pt4.TransformBy(Matrix2d.Mirroring(majAx));
            var pt13 = pt3.TransformBy(Matrix2d.Mirroring(majAx));
            var pt14 = pt2.TransformBy(Matrix2d.Mirroring(majAx));
            var pt15 = pt1.TransformBy(Matrix2d.Mirroring(majAx));
            var bulge1 = Math.Tan((pt4 - cen).GetAngleTo(pt1 - pt0) / 2.0);
            var bulge2 = Math.Tan((pt1 - pt2).GetAngleTo(pt0 - pt4) / 2.0);
            var bulge3 = Math.Tan((pt4 - pt0).GetAngleTo(pt3 - pt2) / 2.0);
            var bulge4 = Math.Tan((pt3 - pt4).GetAngleTo(pt0 - cen) / 2.0);
            _contents.Add(new PolylineSegment(pt0, pt1, bulge1));
            _contents.Add(new PolylineSegment(pt1, pt2, bulge2));
            _contents.Add(new PolylineSegment(pt2, pt3, bulge3));
            _contents.Add(new PolylineSegment(pt3, pt4, bulge4));
            _contents.Add(new PolylineSegment(pt4, pt5, bulge4));
            _contents.Add(new PolylineSegment(pt5, pt6, bulge3));
            _contents.Add(new PolylineSegment(pt6, pt7, bulge2));
            _contents.Add(new PolylineSegment(pt7, pt8, bulge1));
            _contents.Add(new PolylineSegment(pt8, pt9, bulge1));
            _contents.Add(new PolylineSegment(pt9, pt10, bulge2));
            _contents.Add(new PolylineSegment(pt10, pt11, bulge3));
            _contents.Add(new PolylineSegment(pt11, pt12, bulge4));
            _contents.Add(new PolylineSegment(pt12, pt13, bulge4));
            _contents.Add(new PolylineSegment(pt13, pt14, bulge3));
            _contents.Add(new PolylineSegment(pt14, pt15, bulge2));
            _contents.Add(new PolylineSegment(pt15, pt0, bulge1));

            // if the ellipse is an elliptical arc:
            if (!ellipse.Closed)
            {
                double startParam, endParam;
                var startPoint = ellipse.StartPoint.Convert2d(plane);
                var endPoint = ellipse.EndPoint.Convert2d(plane);

                // index of the PolylineSegment closest to the ellipse start point
                var startIndex = GetClosestSegmentIndexTo(startPoint);

                // start point on the PolylineSegment
                var pt = _contents[startIndex].ToCurve2d().GetClosestPointTo(startPoint).Point;

                // if the point is equal to the PolylineSegment end point, jump the next segment in collection
                if (pt.IsEqualTo(_contents[startIndex].EndPoint))
                {
                    if (startIndex == 15)
                        startIndex = 0;
                    else
                        startIndex++;
                    startParam = 0.0;
                }

                // else get the 'parameter' at point on the PolylineSegment
                else
                {
                    startParam = _contents[startIndex].GetParameterOf(pt);
                }

                // index of the PolylineSegment closest to the ellipse end point
                var endIndex = GetClosestSegmentIndexTo(endPoint);

                // end point on the PolylineSegment
                pt = _contents[endIndex].ToCurve2d().GetClosestPointTo(endPoint).Point;

                // if the point is equals to the PolylineSegment startPoint, jump to the previous segment
                if (pt.IsEqualTo(_contents[endIndex].StartPoint))
                {
                    if (endIndex == 0)
                        endIndex = 15;
                    else
                        endIndex--;
                    endParam = 1.0;
                }

                // else get the 'parameter' at point on the PolylineSegment
                else
                {
                    endParam = _contents[endIndex].GetParameterOf(pt);
                }

                // if the parameter at start point is not equal to 0.0, calculate the bulge
                if (Math.Abs(startParam) > 0.0001)
                {
                    _contents[startIndex].StartPoint = startPoint;
                    _contents[startIndex].Bulge = _contents[startIndex].Bulge * (1.0 - startParam);
                }

                // if the parameter at end point is not equal to 1.0, calculate the bulge
                if (Math.Abs(endParam - 1.0) > 0.0001) // (endParam != 0.0)
                {
                    _contents[endIndex].EndPoint = endPoint;
                    _contents[endIndex].Bulge = _contents[endIndex].Bulge * endParam;
                }

                // if both points are on the same segment
                if (startIndex == endIndex)
                {
                    var segment = _contents[startIndex];
                    _contents.Clear();
                    _contents.Add(segment);
                }
                else if (startIndex < endIndex)
                {
                    _contents.RemoveRange(endIndex + 1, 15 - endIndex);
                    _contents.RemoveRange(0, startIndex);
                }
                else
                {
                    _contents.AddRange(_contents.GetRange(0, endIndex + 1));
                    _contents.RemoveRange(0, startIndex);
                }
            }
        }

        /// <summary>
        /// Gets the number of elements actually contained in the collection.
        /// </summary>
        public int Count => _contents.Count;

        /// <summary>
        /// Gets the last PolylineSegment EndPoint
        /// </summary>
        public Point2d EndPoint => _contents[Count - 1].EndPoint;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the first PolylineSegment StartPoint
        /// </summary>
        public Point2d StartPoint => _contents[0].StartPoint;

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public PolylineSegment this[int index]
        {
            get => _contents[index];
            set => _contents[index] = value;
        }

        /// <summary>
        /// Adds a segment to the end of the collection.
        /// </summary>
        /// <param name="item">The segment to be added to the end of the collection.</param>
        public void Add(PolylineSegment item)
        {
            _contents.Add(item);
        }

        /// <summary>
        /// Adds the segments of the specified collection to the end of the collection.
        /// </summary>
        /// <param name="range">The collection whose elements should be added to the end of this collection.</param>
        public void AddRange([NotNull] IEnumerable<PolylineSegment> range)
        {
            _contents.AddRange(range);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            _contents.Clear();
        }

        /// <summary>
        /// Determines whether a segment is in the collection.
        /// </summary>
        /// <param name="item">The segment to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(PolylineSegment item)
        {
            return _contents.Contains(item);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from collection.</param>
        /// <param name="index">The zero-based index in array at which copying begin.s</param>
        public void CopyTo(PolylineSegment[] array, int index)
        {
            _contents.CopyTo(array, index);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, –1.</returns>
        public int FindIndex([NotNull] Predicate<PolylineSegment> match)
        {
            return _contents.FindIndex(match);
        }

        /// <summary>
        /// Returns the zero-based index of the closest segment to the input point.
        /// </summary>
        /// <param name="pt">The Point2d from which the distances to segments are compared.</param>
        /// <returns>The zero-based index of the segment in the PolylineSegmentCollection.</returns>
        public int GetClosestSegmentIndexTo(Point2d pt)
        {
            var result = 0;
            var dist = _contents[0].ToCurve2d().GetDistanceTo(pt);
            for (var i = 1; i < Count; i++)
            {
                var tmpDist = _contents[i].ToCurve2d().GetDistanceTo(pt);
                if (tmpDist < dist)
                {
                    result = i;
                    dist = tmpDist;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerable&lt;PolylineSegment&gt; enumerator for the PolylineSegmentCollection.</returns>
        public IEnumerator<PolylineSegment> GetEnumerator()
        {
            foreach (var seg in _contents)
                yield return seg;
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a value in the collection.
        /// </summary>
        /// <param name="item">The segment to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire List, if found; otherwise, –1.</returns>
        public int IndexOf(PolylineSegment item)
        {
            return _contents.IndexOf(item);
        }

        /// <summary>
        /// Inserts a segment into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The segment to insert.</param>
        public void Insert(int index, PolylineSegment item)
        {
            _contents.Insert(index, item);
        }

        /// <summary>
        /// Inserts a segments collection into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which collection should be inserted</param>
        /// <param name="collection">The collection to insert</param>
        public void InsertRange(int index, [NotNull] IEnumerable<PolylineSegment> collection)
        {
            _contents.InsertRange(index, collection);
        }

        /// <summary>
        /// Joins the contiguous segments into one or more PolylineSegment collections.
        /// Start point and end point of each segment are compared  using the global tolerance.
        /// </summary>
        /// <returns>A List of PolylineSegmentCollection instances.</returns>
        [NotNull]
        public List<PolylineSegmentCollection> Join()
        {
            return Join(Tolerance.Global);
        }

        /// <summary>
        /// Joins the contiguous segments into one or more PolylineSegment collections.
        /// Start point and end point of each segment are compared  using the specified tolerance.
        /// </summary>
        /// <param name="tol">The tolerance to use while comparing segments startand end points</param>
        /// <returns>A List of PolylineSegmentCollection instances.</returns>
        [NotNull]
        public List<PolylineSegmentCollection> Join(Tolerance tol)
        {
            var result = new List<PolylineSegmentCollection>();
            var clone = new PolylineSegmentCollection(_contents);
            while (clone.Count > 0)
            {
                var newCol = new PolylineSegmentCollection();
                var seg = clone[0];
                newCol.Add(seg);
                var start = seg.StartPoint;
                var end = seg.EndPoint;
                clone.RemoveAt(0);
                while (true)
                {
                    // ReSharper disable once AccessToModifiedClosure
                    var i = clone.FindIndex(s => s.StartPoint.IsEqualTo(end, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        newCol.Add(seg);
                        end = seg.EndPoint;
                        clone.RemoveAt(i);
                        continue;
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    i = clone.FindIndex(s => s.EndPoint.IsEqualTo(end, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        seg.Inverse();
                        newCol.Add(seg);
                        end = seg.EndPoint;
                        clone.RemoveAt(i);
                        continue;
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    i = clone.FindIndex(s => s.EndPoint.IsEqualTo(start, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        newCol.Insert(0, seg);
                        start = seg.StartPoint;
                        clone.RemoveAt(i);
                        continue;
                    }

                    // ReSharper disable once AccessToModifiedClosure
                    i = clone.FindIndex(s => s.StartPoint.IsEqualTo(start, tol));
                    if (i >= 0)
                    {
                        seg = clone[i];
                        seg.Inverse();
                        newCol.Insert(0, seg);
                        start = seg.StartPoint;
                        clone.RemoveAt(i);
                        continue;
                    }

                    break;
                }

                result.Add(newCol);
            }

            return result;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The segment to remove.</param>
        /// <returns>The segment to remove from the collection.</returns>
        public bool Remove(PolylineSegment item)
        {
            return _contents.Remove(item);
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            _contents.RemoveAt(index);
        }

        /// <summary>
        /// Removes a range of segments from the collection.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of segments to remove.</param>
        /// <param name="count">The number of segments to remove.</param>
        public void RemoveRange(int index, int count)
        {
            _contents.RemoveRange(index, count);
        }

        /// <summary>
        /// Reverses the collection order and all PolylineSegments
        /// </summary>
        public void Reverse()
        {
            for (var i = 0; i < Count; i++)
            {
                _contents[i].Inverse();
            }

            _contents.Reverse();
        }

        /// <summary>
        /// Creates a new Polyline from the PolylineSegment collection.
        /// </summary>
        /// <returns>A Polyline instance.</returns>
        [NotNull]
        public Polyline ToPolyline()
        {
            var pline = new Polyline();
            for (var i = 0; i < _contents.Count; i++)
            {
                var seg = _contents[i];
                pline.AddVertexAt(i, seg.StartPoint, seg.Bulge, seg.StartWidth, seg.EndWidth);
            }

            var j = _contents.Count;
            pline.AddVertexAt(j, this[j - 1].EndPoint, 0.0, _contents[j - 1].EndWidth, _contents[0].StartWidth);
            if (pline.GetPoint2dAt(0).IsEqualTo(pline.GetPoint2dAt(j)))
            {
                pline.RemoveVertexAt(j);
                pline.Closed = true;
            }

            return pline;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}