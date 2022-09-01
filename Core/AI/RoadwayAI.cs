using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;
using Roadway.Spline;

namespace Roadway.AI {

    public static class RoadwayAI {

        public static Path GeneratePath(RoadwayManager roadway, Vector3 startPoint, Vector3 endPoint, float maxSearchRadius) {
            Location start = FindClosestRoadwayLocation( roadway, startPoint, maxSearchRadius );
            Location end = FindClosestRoadwayLocation( roadway, endPoint, maxSearchRadius );

            if ( start == null || start.roadSection == null || end == null || end.roadSection == null )
                return null;

            Path path = new Path( start, end );
            return path;
        }

        public static Junction FindConnectedJunction(Location loc) {
            (Junction junction, int snapIndex) conJun = GetNextJunction( loc.roadSection, loc.lane.flowDirection ? 0 : loc.roadSection.Path.NumPoints - 1 );
            if ( conJun.junction == null )
                conJun = GetNextJunction( loc.roadSection, loc.lane.flowDirection ? loc.roadSection.Path.NumPoints - 1 : 0 );
            return conJun.junction;
        }

        public static (Junction, int) GetNextJunction(RoadSection roadSection, int handleToSearchFrom) {
            while ( roadSection != null ) {
                // roadSection will become null or we find another Junction
                // Get the SplineSnapPoint for the end of the roadSection
                SplineSnapPoint splineSnapPoint = roadSection.SnapOfHandle( handleToSearchFrom );
                // can be null if nothing connected
                if ( splineSnapPoint != null ) {
                    // next SnappableObject, could be junction or RoadSection
                    SnappableObject nextSnappableObject = splineSnapPoint.GetOther( roadSection );
                    // could also be null
                    if ( nextSnappableObject != null ) {
                        // something is connected, could be junction or RoadSection
                        if ( nextSnappableObject is Junction ) {
                            // if it is a Junction we return and end the search
                            return ((Junction)nextSnappableObject, splineSnapPoint.anchorHandleIndex);
                        } else if ( nextSnappableObject is RoadSection ) {
                            // if it is a RoadSection we can store and continue the search
                            roadSection = (RoadSection)nextSnappableObject;
                            handleToSearchFrom = splineSnapPoint.GetHandleIndex( roadSection );
                        }

                    } else {
                        // if nextSnappableObject was null, return null to end the while loop search
                        return (null, -1);
                    }
                } else {
                    // if nextSnappableObject was null, return null to end the while loop search
                    return (null, -1);
                }
            }
            return (null, -1);
        }

        public static Vector3 NextTarget(Vector3 curPos, Location currentLocation, float targetChangeDistanceSqr) {
            Vector3 target = currentLocation.lane.points[ currentLocation.pointIndex ];

            Vector3 directionToTarget = target - curPos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if ( dSqrToTarget < targetChangeDistanceSqr ) {
                currentLocation.pointIndex += currentLocation.lane.flowDirection ? -1 : 1;

                if ( currentLocation.pointIndex >= 0 && currentLocation.pointIndex < currentLocation.lane.points.Count ) {
                    return currentLocation.lane.points[ currentLocation.pointIndex ];
                }

                // else lane has ended, need to find next lane
                return Vector3.negativeInfinity;
            }
            // else keep same target
            return target;
        }

        public static Location NextLocation(Location currentLocation) {
            int handleIndex = currentLocation.lane.flowDirection ? 0 : currentLocation.roadSection.Path.NumPoints - 1;
            SplineSnapPoint splineSnapPoint = currentLocation.roadSection.SnapOfHandle( handleIndex );
            SnappableObject snapObj = splineSnapPoint.GetOther( currentLocation.roadSection );
            if ( snapObj != null ) {

                if ( snapObj is Intersection ) {

                    List<Location> options = GetLocationsFromIntersetion( currentLocation, (Intersection)snapObj );
                    if ( options.Count > 0 ) {
                        return options[ UnityEngine.Random.Range( 0, options.Count ) ];
                    }

                } else if ( snapObj is RoadSection ) {

                    Location newLocation = GetLocationFromRoadSection( currentLocation, splineSnapPoint, (RoadSection)snapObj );
                    if ( newLocation != null ) {
                        return newLocation;
                    }
                }
            }
            // else loop back on same roadsection?
            if ( currentLocation.roadSection.LeftLanes.Contains( currentLocation.lane ) && currentLocation.roadSection.numLanesRight > 0 ) {
                // loop back onto right
                currentLocation.lane = currentLocation.roadSection.RightLanes[ UnityEngine.Random.Range( 0, currentLocation.roadSection.numLanesRight ) ];
                if ( currentLocation.pointIndex < 0 )
                    currentLocation.pointIndex = 0;
                else
                    currentLocation.pointIndex = currentLocation.lane.points.Count - 1;

                return currentLocation;
            } else if ( currentLocation.roadSection.RightLanes.Contains( currentLocation.lane ) && currentLocation.roadSection.numLanesLeft > 0 ) {
                // loop back onto left
                currentLocation.lane = currentLocation.roadSection.LeftLanes[ UnityEngine.Random.Range( 0, currentLocation.roadSection.numLanesLeft ) ];
                if ( currentLocation.pointIndex < 0 )
                    currentLocation.pointIndex = 0;
                else
                    currentLocation.pointIndex = currentLocation.lane.points.Count - 1;

                return currentLocation;
            }

            return null;
        }

        public static Location GetLocationFromRoadSection(Location currentLocation, SplineSnapPoint splineSnapPoint, RoadSection otherRoadSection) {
            // roadSection given is the result of splineSnapPoint.GetOther();
            // index to use, GetHandleIndex returns the handle index but we can use this to use either the first point or the last
            int newLanePointIndex = splineSnapPoint.GetHandleIndex( otherRoadSection );
            if ( newLanePointIndex == -1 ) // -1 returned if 'otherRoadSection' not in snapObj. shouldn't happen, but we check
                return null;

            // match the lane
            // left or right lane?
            bool currentLaneIsLeft = currentLocation.roadSection.LeftLanes.Contains( currentLocation.lane );
            // number of lanes
            int currentNumLanes = currentLaneIsLeft ? currentLocation.roadSection.LeftLanes.Count :
                                                        currentLocation.roadSection.RightLanes.Count;
            // index of lane?
            int currentLaneIndex = currentLaneIsLeft ? currentLocation.roadSection.LeftLanes.IndexOf( currentLocation.lane ) :
                                                        currentLocation.roadSection.RightLanes.IndexOf( currentLocation.lane );
            // lanes to check
            bool otherLaneIsLeft = false;
            // are we connecting to end?
            if ( splineSnapPoint.anchorHandleIndex == 0 && splineSnapPoint.snappedHandleIndex == 0 ) {
                // start to start
                otherLaneIsLeft = currentLaneIsLeft ? false : true;
            } else if ( splineSnapPoint.anchorHandleIndex == 0 && splineSnapPoint.snappedHandleIndex != 0 ) {
                // start to end
                otherLaneIsLeft = currentLaneIsLeft ? true : false;
            } else if ( splineSnapPoint.anchorHandleIndex != 0 && splineSnapPoint.snappedHandleIndex == 0 ) {
                // end to start
                otherLaneIsLeft = currentLaneIsLeft ? true : false;
            } else if ( splineSnapPoint.anchorHandleIndex != 0 && splineSnapPoint.snappedHandleIndex != 0 ) {
                // end to end
                otherLaneIsLeft = currentLaneIsLeft ? false : true;
            }

            int otherNumLanes = otherLaneIsLeft ? otherRoadSection.numLanesLeft : otherRoadSection.numLanesRight;

            // if same number of lanes, use lane at same index
            if ( otherNumLanes == currentNumLanes ) {
                Lane newLane = otherLaneIsLeft ? otherRoadSection.LeftLane( currentLaneIndex ) : otherRoadSection.RightLane( currentLaneIndex );
                newLanePointIndex = newLanePointIndex == 0 ? 0 : newLane.points.Count - 1;
                return new Location( otherRoadSection, newLane, newLanePointIndex );
            }

            // different number of lanes
            // workout how to match
            int currentNumInnerLanes = currentLaneIsLeft ? currentLocation.roadSection.numLanesLeftInner : currentLocation.roadSection.numLanesRightInner;
            int otherNumInnerLanes = otherLaneIsLeft ? otherRoadSection.numLanesLeftInner : otherRoadSection.numLanesRightInner;

            int currentZeroLane = currentNumLanes - currentNumInnerLanes;
            int otherZeroLane = otherNumLanes - otherNumInnerLanes;
            int innerLanechange = currentNumInnerLanes - otherNumInnerLanes;

            if ( Mathf.Abs( innerLanechange ) == 1 ) {
                int laneIndexToUse = currentLaneIndex - innerLanechange;
                if ( laneIndexToUse < otherNumLanes ) {
                    Lane newLane = otherLaneIsLeft ? otherRoadSection.LeftLane( laneIndexToUse ) : otherRoadSection.RightLane( laneIndexToUse );
                    newLanePointIndex = newLanePointIndex == 0 ? 0 : newLane.points.Count - 1;
                    return new Location( otherRoadSection, newLane, newLanePointIndex );
                } else {
                    // else 
                    Lane newLane = otherLaneIsLeft ? otherRoadSection.LeftLane( otherNumLanes - 1 ) : otherRoadSection.RightLane( otherNumLanes - 1 );
                    newLanePointIndex = newLanePointIndex == 0 ? 0 : newLane.points.Count - 1;
                    return new Location( otherRoadSection, newLane, newLanePointIndex );
                }
            }
            // else 
            if ( currentLaneIndex < otherNumLanes ) {
                Lane newLane = otherLaneIsLeft ? otherRoadSection.LeftLane( currentLaneIndex ) : otherRoadSection.RightLane( currentLaneIndex );
                newLanePointIndex = newLanePointIndex == 0 ? 0 : newLane.points.Count - 1;
                return new Location( otherRoadSection, newLane, newLanePointIndex );
            }
            // else else
            Lane newLaneFinal = otherLaneIsLeft ? otherRoadSection.LeftLane( otherNumLanes - 1 ) : otherRoadSection.RightLane( otherNumLanes - 1 );
            newLanePointIndex = newLanePointIndex == 0 ? 0 : newLaneFinal.points.Count - 1;
            return new Location( otherRoadSection, newLaneFinal, newLanePointIndex );
        }

        public static List<Location> GetLocationsFromIntersetion(Location curLoc, Intersection inter) {
            List<Location> locs = new List<Location>();
            List<JunctionLaneLink> links = inter.GetLinksFromOrigin( curLoc.lane );
            for ( int i = 0; i < links.Count; i++ ) {
                Location newLoc = new Location( inter.Snap( links[ i ].destinationSnapIndex ).SnappedRoadSection, links[ i ].destination,
                                                    links[ i ].destinationStart ? 0 : links[ i ].destination.points.Count - 1 );
                locs.Add( newLoc );
            }
            return locs;
        }

        public static Location GetLocationFromJunctionPair(Location curLoc, Junction juncA, Junction juncB) {
            JunctionConnection jc = juncA.GetJunctionConnection( juncB );
            if ( jc == null )
                return null;

            Intersection inter = (Intersection)juncA;
            int snapIndexOutbound = jc.GetSnapIndexForJunction( juncA );
            // int snapIndexInbound1 = inter.SnapOf( curLoc.roadSection );
            int snapIndexInbound = curLoc.roadSection.SnapOfHandle( curLoc.lane.flowDirection ? 0 : curLoc.roadSection.Path.NumPoints - 1 ).anchorHandleIndex;
            List<JunctionLaneLink> links = inter.GetLinksBetween( snapIndexInbound, snapIndexOutbound );

            if ( links == null || links.Count < 1  )
                return null;

            int i = UnityEngine.Random.Range( 0, links.Count );
            return new Location( inter.Snap( links[ i ].destinationSnapIndex ).SnappedRoadSection, links[ i ].destination,
                                                links[ i ].destinationStart ? 0 : links[ i ].destination.points.Count - 1 );

        }


        public static Location FindClosestRoadwayLocation(RoadwayManager roadway, Vector3 pos, float maxSearchRadius) {
            List<RoadSection> roadSections = new List<RoadSection>();
            float radius = 1;
            do {
                Collider[] colliders = Physics.OverlapSphere( pos, radius );
                radius += radius;

                foreach ( Collider col in colliders ) {
                    if ( col.gameObject.GetComponent<RoadSection>() )
                        roadSections.Add( col.gameObject.GetComponent<RoadSection>() );
                }
            } while ( roadSections.Count < 1 && radius < maxSearchRadius );

            RoadSection closestRoadSection = null;
            Lane closestLane = null;
            int closestPointIndex = -1;
            float closestDistanceSqr = Mathf.Infinity;

            foreach ( RoadSection roadSection in roadSections ) {
                foreach ( Lane lane in roadSection.LeftLanes ) {
                    for ( int p = 0; p < lane.points.Count; p++ ) {
                        Vector3 directionToTarget = lane.points[ p ] - pos;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if ( dSqrToTarget < closestDistanceSqr ) {
                            closestDistanceSqr = dSqrToTarget;
                            closestPointIndex = p;
                            closestLane = lane;
                            closestRoadSection = roadSection;
                        }
                    }
                }
                foreach ( Lane lane in roadSection.RightLanes ) {
                    for ( int p = 0; p < lane.points.Count; p++ ) {
                        Vector3 directionToTarget = lane.points[ p ] - pos;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if ( dSqrToTarget < closestDistanceSqr ) {
                            closestDistanceSqr = dSqrToTarget;
                            closestPointIndex = p;
                            closestLane = lane;
                            closestRoadSection = roadSection;
                        }
                    }
                }
            }

            return new Location( closestRoadSection, closestLane, closestPointIndex );
        }

    }

}
