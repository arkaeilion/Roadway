using System;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;
using Roadway.Spline;

namespace Roadway.AI {

    [System.Serializable]
    public class Path {

        public Location start;
        public Location end;

        public List<Junction> goals;

        public List<Junction> path;

        public bool IsComplete {
            get { return path.Count > 0; }
        }

        public Path(RoadwayManager roadway, Vector3 startPoint, Vector3 endPoint, float maxSearchRadius) {
            start = RoadwayAI.FindClosestRoadwayLocation( roadway, startPoint, maxSearchRadius );
            end = RoadwayAI.FindClosestRoadwayLocation( roadway, endPoint, maxSearchRadius );

            if ( start != null && start.roadSection != null && end != null && end.roadSection != null ) {
                goals = new List<Junction> { RoadwayAI.FindConnectedJunction( end ) };
                path = Search.AStarSearch<Junction>( new List<Junction> { RoadwayAI.FindConnectedJunction( start ) },
                                                        goals,
                                                        (junc) => Vector3.Distance( junc.Position, end.Point ),
                                                        (juncA, juncB) => juncA.GetDistance( juncB ),
                                                        (junc) => junc.GetConnected() );
            }
        }

        public Path(Location start, Location end) {
            this.start = start;
            this.end = end;

            goals = new List<Junction> { RoadwayAI.FindConnectedJunction( end ) };
            path = Search.AStarSearch<Junction>( new List<Junction> { RoadwayAI.FindConnectedJunction( start ) },
                                                    goals,
                                                    (junc) => Vector3.Distance( junc.Position, end.Point ),
                                                    (juncA, juncB) => juncA.GetDistance( juncB ),
                                                    (junc) => junc.GetConnected() );
        }

        public Junction GetNextJunction(Junction junc) {
            if ( path.Contains( junc ) ) {
                int i = path.IndexOf( junc ) + 1;
                if ( i < path.Count )
                    return path[ i ];
            }
            return null;
        }

        public Location NextLocation(Location current) {
            int handleIndex = current.lane.flowDirection ? 0 : current.roadSection.Path.NumPoints - 1;
            SplineSnapPoint splineSnapPoint = current.roadSection.SnapOfHandle( handleIndex );
            SnappableObject snapObj = splineSnapPoint.GetOther( current.roadSection );
            if ( snapObj != null ) {

                if ( snapObj is Intersection ) {
                    Intersection inter = (Intersection)snapObj;

                    Junction nextJunc = GetNextJunction( inter );
                    if ( nextJunc == null ) {
                        // could be last intersection 
                        if ( goals.Contains( (Junction)inter ) ) {
                            // this is the last intersection, link to last roadSection
                            (Junction junction, int snapIndex) conJun = RoadwayAI.GetNextJunction( end.roadSection, 0 );
                            int snapIndexOutBound = conJun.snapIndex;
                            if ( (Junction)snapObj != conJun.junction )
                                snapIndexOutBound = RoadwayAI.GetNextJunction( end.roadSection, end.roadSection.Path.NumPoints - 1 ).Item2;

                            int snapIndexInbound = inter.SnapOf( current.roadSection );
                            List<JunctionLaneLink> links = inter.GetLinksBetween( snapIndexInbound, snapIndexOutBound );

                            int i = UnityEngine.Random.Range( 0, links.Count );
                            return new Location( inter.Snap( links[ i ].destinationSnapIndex ).SnappedRoadSection, links[ i ].destination,
                                                                links[ i ].destinationStart ? 0 : links[ i ].destination.points.Count - 1 );


                        }
                        // else something is wrong
                        return null;
                    }
                    Location newLocation = RoadwayAI.GetLocationFromJunctionPair( current, (Intersection)snapObj, nextJunc );
                    return newLocation;

                } else if ( snapObj is RoadSection ) {

                    Location newLocation = RoadwayAI.GetLocationFromRoadSection( current, splineSnapPoint, (RoadSection)snapObj );
                    if ( newLocation != null ) {
                        return newLocation;
                    }
                }
            }
            // else loop back on same roadsection?
            if ( current.roadSection.LeftLanes.Contains( current.lane ) && current.roadSection.numLanesRight > 0 ) {
                // loop back onto right
                current.lane = current.roadSection.RightLanes[ UnityEngine.Random.Range( 0, current.roadSection.numLanesRight ) ];
                if ( current.pointIndex < 0 )
                    current.pointIndex = 0;
                else
                    current.pointIndex = current.lane.points.Count - 1;

                return current;
            } else if ( current.roadSection.RightLanes.Contains( current.lane ) && current.roadSection.numLanesLeft > 0 ) {
                // loop back onto left
                current.lane = current.roadSection.LeftLanes[ UnityEngine.Random.Range( 0, current.roadSection.numLanesLeft ) ];
                if ( current.pointIndex < 0 )
                    current.pointIndex = 0;
                else
                    current.pointIndex = current.lane.points.Count - 1;

                return current;
            }

            return null;
        }


    }

}
