﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Tools.Math.Geo;
using OsmSharp.Tools.Math;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Graph.DynamicGraph;
using OsmSharp.Routing.Graph.DynamicGraph.SimpleWeighed;
using OsmSharp.Routing;

namespace OsmSharp.Routing.Osm.Data.Processing
{
    /// <summary>
    /// A data processing target accepting raw OSM data and converting it into routable data.
    /// </summary>
    public class SimpleWeighedDataGraphProcessingTarget : DynamicGraphDataProcessorTarget<SimpleWeighedEdge>
    {
        /// <summary>
        /// Holds the vehicle profile this pre-processing target is for.
        /// </summary>
        private VehicleEnum _vehicle;

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        /// <param name="dynamic_graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="tags_index"></param>
        /// <param name="id_transformations"></param>
        public SimpleWeighedDataGraphProcessingTarget(IDynamicGraph<SimpleWeighedEdge> dynamic_graph,
            IRoutingInterpreter interpreter, ITagsIndex tags_index, VehicleEnum vehicle)
            : this(dynamic_graph, interpreter, tags_index, vehicle, new Dictionary<long, uint>())
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        /// <param name="dynamic_graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="tags_index"></param>
        /// <param name="id_transformations"></param>
        public SimpleWeighedDataGraphProcessingTarget(IDynamicGraph<SimpleWeighedEdge> dynamic_graph,
            IRoutingInterpreter interpreter, ITagsIndex tags_index, VehicleEnum vehicle, IDictionary<long, uint> id_transformations)
            : this(dynamic_graph, interpreter, tags_index, vehicle, id_transformations, null)
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        /// <param name="dynamic_graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="tags_index"></param>
        /// <param name="id_transformations"></param>
        public SimpleWeighedDataGraphProcessingTarget(IDynamicGraph<SimpleWeighedEdge> dynamic_graph,
            IRoutingInterpreter interpreter, ITagsIndex tags_index, VehicleEnum vehicle, GeoCoordinateBox box)
            : this(dynamic_graph, interpreter, tags_index, vehicle, new Dictionary<long, uint>(), box)
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        /// <param name="dynamic_graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="tags_index"></param>
        /// <param name="id_transformations"></param>
        public SimpleWeighedDataGraphProcessingTarget(IDynamicGraph<SimpleWeighedEdge> dynamic_graph,
            IRoutingInterpreter interpreter, ITagsIndex tags_index, VehicleEnum vehicle, IDictionary<long, uint> id_transformations, GeoCoordinateBox box)
            : base(dynamic_graph, interpreter, null, tags_index, id_transformations, box)
        {
            _vehicle = vehicle;
        }

        /// <summary>
        /// Calculates edge data.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="direction_forward"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected override SimpleWeighedEdge CalculateEdgeData(IEdgeInterpreter edge_interpreter, ITagsIndex tags_index, IDictionary<string, string> tags,
            bool direction_forward, GeoCoordinate from, GeoCoordinate to)
        {
            // use the distance as weight.
            double distance = edge_interpreter.Weight(tags, _vehicle, from, to);

            if (tags_index == null)
            {
                return new SimpleWeighedEdge()
                {
                    IsForward = direction_forward,
                    //Tags = tags_index.Add(tags),
                    Weight = distance
                };
            }
            return new SimpleWeighedEdge()
            {
                IsForward = direction_forward,
                Tags = tags_index.Add(tags),
                Weight = distance
            };
        }
    }
}