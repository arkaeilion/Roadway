using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.AI;
using Roadway.Roads;
using Roadway.Junctions;
using Roadway.Spline;

namespace Roadway.Testing {

    public class PathViewer : MonoBehaviour {

        RoadwayManager roadway;
        RoadwayManager Roadway {
            get {
                if ( roadway == null )
                    roadway = FindObjectOfType<RoadwayManager>();
                return roadway;
            }
        }

        public Location current;
        public Vector3 target;

        public Path path;
        public Vector3 end;
        public float maxSearchRadius = 5000;

        public List<Vector3> points = new List<Vector3>();

        public void DO() {
            path = new Path( Roadway, transform.position, end, maxSearchRadius );
            if ( !path.IsComplete )
                return;

            current = path.start;

            target = RoadwayAI.NextTarget( transform.position, current, 2 );

            CollectAllPathPoints();
        }

        public void CollectAllPathPoints() {
            points = new List<Vector3>();
            points.Add( transform.position );

            bool fin = false;

            while ( !fin ) {

                // add all points in current lane
                while ( current.pointIndex >= 0 && current.pointIndex < current.lane.points.Count ) {
                    if ( current.roadSection == path.end.roadSection && current.pointIndex == path.end.pointIndex ) {
                        fin = true;
                        break;
                    }
                    points.Add( current.lane.points[ current.pointIndex ] );
                    current.pointIndex += current.lane.flowDirection ? -1 : 1;
                }

                if ( fin )
                    break;

                // get next lane
                Location backup = current;
                current = path.NextLocation( current );
                if ( current == null ) {
                    Junction junc = backup.GetJunction();
                    List<JunctionLaneLink> links = ( (Intersection)junc ).GetLinksFromOrigin( backup.lane );
                    foreach ( JunctionLaneLink link in links ) {
                        Vector3 snapPos = link.DestinationPoint;
                        Path newPath = RoadwayAI.GeneratePath( Roadway, snapPos, end, maxSearchRadius );
                        if ( path != null || path.IsComplete ) {
                            path = newPath;
                            current = new Location( ( (Intersection)junc ).Snap( link.destinationSnapIndex ).SnappedRoadSection, link.destination,
                                                    link.destinationStart ? 0 : link.destination.points.Count - 1 );
                            break;
                        }
                    }
                }

                if ( current == null ) {
                    break;
                }

            }

            points.Add( end );

        }

        void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( end, 1 );
        }


    }

}
