// Rectangle.java
//   Java Spatial Index Library
//   Copyright (C) 2002 Infomatiq Limited
//
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
// Ported to C# By Dror Gluska, April 9th, 2009

namespace AcadLib.RTree.SpatialIndex
{
    using System;
    using System.Text;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    /**
     * Currently hardcoded to 2 dimensions, but could be extended.
     *
     * @author  aled@sourceforge.net
     * @version 1.0b2p1
     */
    [PublicAPI]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Rectangle
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /**
         * Number of dimensions in a rectangle. In theory this
         * could be exended to three or more dimensions.
         */
        public double[] _max;
        public double[] _min;
        internal const int DIMENSIONS = 3;

        /**
         * array containing the minimum value for each dimension; ie { min(x), min(y) }
         */
        /**
         * array containing the maximum value for each dimension; ie { max(x), max(y) }
         */
        /**
         * Constructor.
         *
         * @param x1 coordinate of any corner of the rectangle
         * @param y1 (see x1)
         * @param x2 coordinate of the opposite corner
         * @param y2 (see x2)
         */
        public Rectangle(double x1, double y1, double x2, double y2, double z1, double z2)
        {
            _min = new double[DIMENSIONS];
            _max = new double[DIMENSIONS];
            Set(x1, y1, x2, y2, z1, z2);
        }

        /**
         * Constructor.
         *
         * @param min array containing the minimum value for each dimension; ie { min(x), min(y) }
         * @param max array containing the maximum value for each dimension; ie { max(x), max(y) }
         */
        public Rectangle([NotNull] double[] min, [NotNull] double[] max)
        {
            if (min.Length != DIMENSIONS || max.Length != DIMENSIONS)
            {
                throw new Exception("Error in Rectangle constructor: " +
                                    "min and max arrays must be of length " + DIMENSIONS);
            }

            _min = new double[DIMENSIONS];
            _max = new double[DIMENSIONS];
            Set(min, max);
        }

        public Rectangle(Extents3d extents) : this(extents.MinPoint.X, extents.MinPoint.Y,
            extents.MaxPoint.X, extents.MaxPoint.Y, 0, 0)
        {
        }

        public Rectangle(Extents2d extents) : this(extents.MinPoint.X, extents.MinPoint.Y,
            extents.MaxPoint.X, extents.MaxPoint.Y, 0, 0)
        {
        }

        /**
          * Sets the size of the rectangle.
          *
          * @param x1 coordinate of any corner of the rectangle
          * @param y1 (see x1)
          * @param x2 coordinate of the opposite corner
          * @param y2 (see x2)
          */
#pragma warning disable 659

        public override bool Equals(object obj)
#pragma warning restore 659
        {
            var equals = false;
            if (obj?.GetType() == typeof(Rectangle))
            {
                var r = (Rectangle)obj;

                // #warning possible didn't convert properly
                if (CompareArrays(r._min, _min) && CompareArrays(r._max, _max))
                {
                    equals = true;
                }
            }

            return equals;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            // min coordinates
            sb.Append('(');
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(_min[i]);
            }

            sb.Append("), (");

            // max coordinates
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(_max[i]);
            }

            sb.Append(')');
            return sb.ToString();
        }

        internal void Add(Rectangle r)
        {
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (r._min[i] < _min[i])
                {
                    _min[i] = r._min[i];
                }

                if (r._max[i] > _max[i])
                {
                    _max[i] = r._max[i];
                }
            }
        }

        internal double Area()
        {
            return (_max[0] - _min[0]) * (_max[1] - _min[1]);
        }

        internal bool ContainedBy(Rectangle r)
        {
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (_max[i] > r._max[i] || _min[i] < r._min[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal bool Contains(Rectangle r)
        {
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (_max[i] < r._max[i] || _min[i] > r._min[i])
                {
                    return false;
                }
            }

            return true;
        }

        [NotNull]
        internal Rectangle Copy()
        {
            return new Rectangle(_min, _max);
        }

        internal double Distance(Point p)
        {
            double distanceSquared = 0;
            for (var i = 0; i < DIMENSIONS; i++)
            {
                var greatestMin = Math.Max(_min[i], p.coordinates[i]);
                var leastMax = Math.Min(_max[i], p.coordinates[i]);
                if (greatestMin > leastMax)
                {
                    distanceSquared += (greatestMin - leastMax) * (greatestMin - leastMax);
                }
            }

            return Math.Sqrt(distanceSquared);
        }

        internal double Distance(Rectangle r)
        {
            double distanceSquared = 0;
            for (var i = 0; i < DIMENSIONS; i++)
            {
                var greatestMin = Math.Max(_min[i], r._min[i]);
                var leastMax = Math.Min(_max[i], r._max[i]);
                if (greatestMin > leastMax)
                {
                    distanceSquared += (greatestMin - leastMax) * (greatestMin - leastMax);
                }
            }

            return Math.Sqrt(distanceSquared);
        }

        internal double DistanceSquared(int dimension, double point)
        {
            double distanceSquared = 0;
            var tempDistance = point - _max[dimension];
            for (var i = 0; i < 2; i++)
            {
                if (tempDistance > 0)
                {
                    distanceSquared = tempDistance * tempDistance;
                    break;
                }

                tempDistance = _min[dimension] - point;
            }

            return distanceSquared;
        }

        internal bool EdgeOverlaps(Rectangle r)
        {
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (Math.Abs(_min[i] - r._min[i]) < 0.0001 || Math.Abs(_max[i] - r._max[i]) < 0.0001)
                {
                    return true;
                }
            }

            return false;
        }

        internal double Enlargement([NotNull] Rectangle r)
        {
            var enlargedArea = (Math.Max(_max[0], r._max[0]) - Math.Min(_min[0], r._min[0])) *
                               (Math.Max(_max[1], r._max[1]) - Math.Min(_min[1], r._min[1]));

            return enlargedArea - Area();
        }

        internal double FurthestDistance(Rectangle r)
        {
            double distanceSquared = 0;

            for (var i = 0; i < DIMENSIONS; i++)
            {
                distanceSquared += Math.Max(r._min[i], r._max[i]);

                // #warning possible didn't convert properly
                // distanceSquared += Math.Max(distanceSquared(i, r.min[i]), distanceSquared(i, r.max[i]));
            }

            return Math.Sqrt(distanceSquared);
        }

        internal bool Intersects(Rectangle r)
        {
            // Every dimension must intersect. If any dimension
            // does not intersect, return false immediately.
            for (var i = 0; i < DIMENSIONS; i++)
            {
                if (_max[i] < r._min[i] || _min[i] > r._max[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal bool SameObject(object o)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return base.Equals(o);
        }

        internal void Set(double x1, double y1, double x2, double y2, double z1, double z2)
        {
            _min[0] = Math.Min(x1, x2);
            _min[1] = Math.Min(y1, y2);
            _min[2] = Math.Min(z1, z2);
            _max[0] = Math.Max(x1, x2);
            _max[1] = Math.Max(y1, y2);
            _max[2] = Math.Max(z1, z2);
        }

        internal void Set([NotNull] double[] min, [NotNull] double[] max)
        {
            Array.Copy(min, 0, _min, 0, DIMENSIONS);
            Array.Copy(max, 0, _max, 0, DIMENSIONS);
        }

        [NotNull]
        internal Rectangle Union(Rectangle r)
        {
            var union = Copy();
            union.Add(r);
            return union;
        }

        private static bool CompareArrays([CanBeNull] double[] a1, [CanBeNull] double[] a2)
        {
            if (a1 == null || a2 == null)
                return false;
            if (a1.Length != a2.Length)
                return false;

            for (var i = 0; i < a1.Length; i++)
                if (Math.Abs(a1[i] - a2[i]) > 0.0001)
                    return false;
            return true;
        }
    }
}