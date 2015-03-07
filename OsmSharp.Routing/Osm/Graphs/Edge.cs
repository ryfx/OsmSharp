// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using OsmSharp.Collections.Arrays;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Graph;

namespace OsmSharp.Routing.Osm.Graphs
{
    /// <summary>
    /// A simple edge containing the orignal OSM-tags and a flag indicating the direction of this edge relative to the 
    /// OSM-direction.
    /// </summary>
    public struct Edge : IGraphEdgeData
    {
        /// <summary>
        /// Contains a value that represents tagsId and forward flag [forwardFlag (true when zero)][tagsIdx].
        /// </summary>
        private uint _value;

        /// <summary>
        /// Gets/sets the value.
        /// </summary>
        internal uint Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Flag indicating if this is a forward or backward edge relative to the tag descriptions.
        /// </summary>
        public bool Forward
        {
            get
            { // true when first bit is 0.
                return _value % 2 == 0;
            }
            set
            {
                if (_value % 2 == 0)
                { // true already.
                    if (!value) { _value = _value + 1; }
                }
                else
                { // false already.
                    if (value) { _value = _value - 1; }
                }
            }
        }

        /// <summary>
        /// The properties of this edge.
        /// </summary>
        public uint Tags
        {
            get
            {
                return _value / 2;
            }
            set
            {
                if (_value % 2 == 0)
                { // true already.
                    _value = value * 2;
                }
                else
                { // false already.
                    _value = (value * 2) + 1;
                }
            }
        }

        /// <summary>
        /// Gets/or sets the total distance of this edge.
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Returns true if this edge represents a neighbour-relation.
        /// </summary>
        public bool RepresentsNeighbourRelations
        {
            get { return true; }
        }

        /// <summary>
        /// Creates the exact reverse of this edge.
        /// </summary>
        /// <returns></returns>
        public IGraphEdgeData Reverse()
        {
            return new Edge()
            {
                Distance = this.Distance,
                Forward = !this.Forward,
                Tags = this.Tags
            };
        }

        /// <summary>
        /// Returns true if the other edge represents the same information than this edge.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IGraphEdgeData other)
        {
            if (other is Edge)
            { // ok, type is the same.
                var otherEdge = (Edge)other;
                if (otherEdge._value != this._value)
                { // basic info different.
                    return false;
                }
                return otherEdge.Distance == this.Distance;
            }
            return false;
        }

        /// <summary>
        /// Returns a description of this edge.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", this.Tags, this.Distance);
        }

        /// <summary>
        /// Holds the size this edge has when converted to uints.
        /// </summary>
        public static int SizeUints = 3;

        /// <summary>
        /// A delegate to map an edge onto uints.
        /// </summary>
        public static MappedHugeArray<Edge, uint>.MapFrom MapFromDelegate = (array, idx) =>
        {
            return new Edge()
                {
                    Value = array[idx],
                    Tags = array[idx + 1],
                    Distance = System.BitConverter.ToSingle(System.BitConverter.GetBytes(array[idx + 2]), 0)
                };
        };

        /// <summary>
        /// A delegate to map an edge onto uints.
        /// </summary>
        public static MappedHugeArray<Edge, uint>.MapTo MapToDelegate = (array, idx, value) =>
        {
                array[idx] = value.Value;
                array[idx + 1] = value.Tags;
                array[idx + 2] = System.BitConverter.ToUInt32(System.BitConverter.GetBytes(value.Distance), 0);
        };
    }
}