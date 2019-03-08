// Point.java
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
// ReSharper disable once CheckNamespace

namespace RTreeLib
{
    using JetBrains.Annotations;

    /// <summary>
    /// Currently hardcoded to 3 dimensions, but could be extended.
    /// author  aled@sourceforge.net
    /// version 1.0b2p1
    /// </summary>
    [PublicAPI]
    public class Point
    {
        /// <summary>
        /// The (x, y) coordinates of the point.
        /// </summary>
        internal double[] coordinates;

        /// <summary>
        /// Number of dimensions in a point. In theory this
        /// could be exended to three or more dimensions.
        /// </summary>
        private const int DIMENSIONS = 3;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        /// <param name="z">The z coordinate of the point</param>
        public Point(double x, double y, double z)
        {
            coordinates = new double[DIMENSIONS];
            coordinates[0] = x;
            coordinates[1] = y;
            coordinates[2] = z;
        }
    }
}