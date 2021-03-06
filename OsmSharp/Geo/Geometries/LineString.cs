﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Math.Geo;

namespace OsmSharp.Geo.Geometries
{
    /// <summary>
    /// Represents a linestring, a simple sequence of line-segments.
    /// </summary>
    public class LineString : Geometry
    {
        /// <summary>
        /// Creates a new linestring.
        /// </summary>
        public LineString()
        {
            this.Coordinates = new List<GeoCoordinate>();
        }

        /// <summary>
        /// Creates a new linestring.
        /// </summary>
        /// <param name="coordinates">The coordinates in this linestring.</param>
        public LineString(IEnumerable<GeoCoordinate> coordinates)
        {
            this.Coordinates = new List<GeoCoordinate>(coordinates);
        }

        /// <summary>
        /// Creates a new linestring.
        /// </summary>
        /// <param name="coordinates">The coordinates in this linestring.</param>
        public LineString(params GeoCoordinate[] coordinates)
        {
            this.Coordinates = new List<GeoCoordinate>(coordinates);
        }

        /// <summary>
        /// Returns the list of coordinates.
        /// </summary>
        public List<GeoCoordinate> Coordinates { get; private set; }

        /// <summary>
        /// Returns the smallest possible bounding box containing this geometry.
        /// </summary>
        public override GeoCoordinateBox Box
        {
            get { return new GeoCoordinateBox(this.Coordinates.ToArray()); }
        }

        /// <summary>
        /// Returns true if this linestring is inside the given bounding box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override bool IsInside(GeoCoordinateBox box)
        {
            for (int idx = 0; idx < this.Coordinates.Count - 1; idx++)
            {
                if (box.IntersectsPotentially(this.Coordinates[idx], this.Coordinates[idx + 1]))
                {
                    if (box.Intersects(this.Coordinates[idx], this.Coordinates[idx + 1]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}