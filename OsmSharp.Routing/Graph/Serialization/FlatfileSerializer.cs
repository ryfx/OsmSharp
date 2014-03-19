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

using OsmSharp.Collections.Tags.Index;
using OsmSharp.Collections.Tags.Serializer;
using OsmSharp.IO;
using OsmSharp.Routing.Graph.Router;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Graph.Serialization
{
    /// <summary>
    /// An abstract serializer to serialize/deserialize a routing data source to a flat-file.
    /// </summary>
    public abstract class FlatfileSerializer<TEdgeData> : RoutingDataSourceSerializer<TEdgeData>
        where TEdgeData : IDynamicGraphEdgeData
    {
        /// <summary>
        /// Does the v1 serialization.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        protected override void DoSerialize(LimitedStream stream,
            DynamicGraphRouterDataSource<TEdgeData> graph)
        {
            // LAYOUT:
            // [SIZE_OF_PROFILES(4bytes)][PROFILES][SIZE_OF_VERTICES(8bytes)][VERTICES][SIZE_OF_EDGES(8bytes)][EDGES][SIZE_OF_TAGS(8bytes][TAGS]

            // serialize supported profiles.
            stream.Seek(4, System.IO.SeekOrigin.Current);
            long position = stream.Position;
            this.SerializeProfiles(stream, graph.GetSupportedProfiles());
            long size = stream.Position - position;
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            byte[] sizeBytes = BitConverter.GetBytes((int)size);
            stream.Write(sizeBytes, 0, 4);
            stream.Seek(size, System.IO.SeekOrigin.Current);

            // serialize coordinates.
            stream.Seek(8, System.IO.SeekOrigin.Current);
            position = stream.Position;
            this.SerializeVertices(stream, graph);
            size = stream.Position - position;
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            sizeBytes = BitConverter.GetBytes(size);
            stream.Write(sizeBytes, 0, 8);
            stream.Seek(size, System.IO.SeekOrigin.Current);

            // serialize edges.
            stream.Seek(8, System.IO.SeekOrigin.Current);
            position = stream.Position;
            this.SerializeEdges(stream, graph);
            size = stream.Position - position;
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            sizeBytes = BitConverter.GetBytes(size);
            stream.Write(sizeBytes, 0, 8);
            stream.Seek(size, System.IO.SeekOrigin.Current);

            // serialize tags.
            stream.Seek(8, System.IO.SeekOrigin.Current);
            position = stream.Position;
            this.SerializeTags(stream, graph.TagsIndex);
            size = stream.Position - position;
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            sizeBytes = BitConverter.GetBytes(size);
            stream.Write(sizeBytes, 0, 8);
            stream.Seek(size, System.IO.SeekOrigin.Current);
        }

        /// <summary>
        /// Does the v1 deserialization.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lazy"></param>
        /// <param name="vehicles"></param>
        /// <returns></returns>
        protected override IBasicRouterDataSource<TEdgeData> DoDeserialize(
            LimitedStream stream, bool lazy, IEnumerable<string> vehicles)
        {
            ITagsCollectionIndex tagsCollectionIndex = null;
            DynamicGraphRouterDataSource<TEdgeData> graph = null;

            // deserialize vehicle profiles.
            var sizeBytes = new byte[4];
            stream.Read(sizeBytes, 0, 4);
            long position = stream.Position;
            long size = BitConverter.ToInt32(sizeBytes, 0);
            this.DeserializeProfiles(stream, size, graph);
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // deserialize vertices.
            sizeBytes = new byte[8];
            stream.Read(sizeBytes, 0, 8);
            position = stream.Position;
            size = BitConverter.ToInt32(sizeBytes, 0);
            this.DeserializeVertices(stream, size, graph);
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // deserialize edges.
            stream.Read(sizeBytes, 0, 8);
            position = stream.Position;
            size = BitConverter.ToInt32(sizeBytes, 0);
            this.DeserializeEdges(stream, size, graph);
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // deserialize tags.
            stream.Read(sizeBytes, 0, 8);
            position = stream.Position;
            size = BitConverter.ToInt32(sizeBytes, 0);
            this.DeserializeTags(stream, size, tagsCollectionIndex);
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            return graph;
        }

        /// <summary>
        /// Serializes the supported vehicle profiles.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="profiles"></param>
        protected virtual void SerializeProfiles(LimitedStream stream, IEnumerable<Vehicle> profiles)
        {
            string[] profileIds = profiles.Select(x => x.UniqueName).ToArray();

            var typeModel = RuntimeTypeModel.Create();
            typeModel.SerializeWithSize(stream, profileIds);
        }

        /// <summary>
        /// Serializes the vertices
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        protected virtual void SerializeVertices(LimitedStream stream, DynamicGraphRouterDataSource<TEdgeData> graph)
        {
            RuntimeTypeModel typeModel = RuntimeTypeModel.Create();
            typeModel.Add(typeof(SerializableVertex), true);

            int blockSize = 10;
            var vertices = new SerializableVertex[blockSize];
            uint vertex = 0;
            float latitude, longitude;
            while(vertex <= graph.VertexCount)
            {
                // build block.
                for(uint idx = 0; idx < blockSize; idx++)
                {
                    uint current = vertex + idx;
                    if (vertex <= graph.VertexCount && graph.GetVertex(current, out latitude, out longitude))
                    { // vertex in the graph.
                        if(vertices[idx] == null)
                        { // make sure there is a vertex.
                            vertices[idx] = new SerializableVertex();
                        }
                        vertices[idx].Latitude = latitude;
                        vertices[idx].Longitude = longitude;
                        vertices[idx].Id = vertex + idx;
                    }
                    else
                    { // vertex not in the graph.
                        vertices[idx] = null;
                    }
                }

                // serialize.
                typeModel.SerializeWithSize(stream, vertices);

                // move to the next vertex.
                vertex = (uint)(vertex + blockSize);
            }
        }

        /// <summary>
        /// Serializes the edges.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        protected abstract void SerializeEdges(LimitedStream stream, DynamicGraphRouterDataSource<TEdgeData> graph);

        /// <summary>
        /// Serializes the meta-data.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tagsCollectionIndex"></param>
        protected virtual void SerializeTags(LimitedStream stream, ITagsCollectionIndexReadonly tagsCollectionIndex)
        {
            // write tags collection-count.
            var countBytes = BitConverter.GetBytes(tagsCollectionIndex.Max);
            stream.Write(countBytes, 0, 4);

            // serialize tags collections one-by-one.
            var serializer = new TagsCollectionSerializer();
            for (uint idx = 0; idx < tagsCollectionIndex.Max; idx++)
            { // serialize objects one-by-one.
                serializer.SerializeWithSize(tagsCollectionIndex.Get(idx), stream);
            }
        }

        /// <summary>
        /// Deserializes the supported vehicle profiles.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="profiles"></param>
        /// <param name="size"></param>
        protected virtual void DeserializeProfiles(LimitedStream stream, long size, DynamicGraphRouterDataSource<TEdgeData> graph)
        {
            var typeModel = RuntimeTypeModel.Create();
            string[] profileIds = typeModel.DeserializeWithSize(stream, null, typeof(string[])) as string[];

            foreach(string profileId in profileIds)
            {
                graph.AddSupportedProfile(Vehicle.GetByUniqueName(profileId));
            }
        }

        /// <summary>
        /// Deserializes the vertices
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        /// <param name="size"></param>
        protected virtual void DeserializeVertices(LimitedStream stream, long size, DynamicGraphRouterDataSource<TEdgeData> graph)
        {
            RuntimeTypeModel typeModel = RuntimeTypeModel.Create();
            typeModel.Add(typeof(SerializableVertex), true);

            var vertices = new SerializableVertex[10];
            long position = stream.Position;
            uint vertex = 0;
            while (stream.Position - position < size)
            { // keep reading vertices until the appriated number of bytes have been read.
                typeModel.DeserializeWithSize(stream, vertices, typeof(SerializableVertex));
                if (vertices != null)
                { // there are a vertices.
                    for (int idx = 0; idx < 10; idx++)
                    {
                        if(vertices[idx] != null)
                        { // there is a vertex.
                            graph.AddVertex(vertices[idx].Latitude, vertices[idx].Longitude);
                        }
                        vertex++;
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes the edges.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <param name="graph"></param>
        protected abstract void DeserializeEdges(LimitedStream stream, long size, DynamicGraphRouterDataSource<TEdgeData> graph);

        /// <summary>
        /// Deserializes the meta-data.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="tagsCollectionIndex"></param>
        /// <param name="size"></param>
        protected virtual void DeserializeTags(LimitedStream stream, long size, ITagsCollectionIndex tagsCollectionIndex)
        {
            if(tagsCollectionIndex.Max != null)
            { // cannot deserialize tags to a non-empty tags index.
                throw new Exception("Cannot deserialize to a non-empty tags index.");
            }
            
            // read tags collection-count.
            var countBytes = new byte[4];
            stream.Read(countBytes, 0, 4);
            int max = BitConverter.ToInt32(countBytes, 0);

            // serialize tags collections one-by-one.
            var serializer = new TagsCollectionSerializer();
            for (uint idx = 0; idx < max; idx++)
            { // serialize objects one-by-one.
                tagsCollectionIndex.Add(serializer.DeserializeWithSize(stream));
            }
        }

        #region Serializable Classes

        /// <summary>
        /// Serializable coordinate.
        /// </summary>
        [ProtoContract]
        internal class SerializableVertex
        {
            /// <summary>
            /// Gets/sets the latitude.
            /// </summary>
            [ProtoMember(1)]
            public float Latitude { get; set; }

            /// <summary>
            /// Gets/sets the longitude.
            /// </summary>
            [ProtoMember(2)]
            public float Longitude { get; set; }

            /// <summary>
            /// Gets/sets the vertex id.
            /// </summary>
            [ProtoMember(3)]
            public uint Id { get; set; }
        }

        #endregion
    }
}