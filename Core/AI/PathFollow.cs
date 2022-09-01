using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;

namespace Roadway.AI {

    public class PathFollow : MonoBehaviour {

        RoadwayManager roadway;
        RoadwayManager Roadway {
            get {
                if ( roadway == null )
                    roadway = FindObjectOfType<RoadwayManager>();
                return roadway;
            }
        }

        public Vector3 start;
        public Vector3 end;
        public float speed = 10;
        public float targetChangeDistanceSqr = 2;
        public float maxSearchRadius = 5000;
        public bool done = false;

        public Path path;
        public Location current;
        public Vector3 target;

        void Start() {
            start = transform.position;
            path = new Path( Roadway, start, end, maxSearchRadius );
            current = path.start;
        }

        void FixedUpdate() {
            if ( !path.IsComplete )
                return;

            Next();

            if ( !done ) {
                transform.LookAt( target );
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards( transform.position, target, step );
            }

        }

        public void Next() {
            target = RoadwayAI.NextTarget( transform.position, current, targetChangeDistanceSqr );

            if ( target.Equals( Vector3.negativeInfinity ) ) {

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
                    done = true;
                } else {
                    target = current.lane.points[ current.pointIndex ];
                }

            }

        }

        void OnDrawGizmos() {

            Gizmos.color = Color.green;
            Gizmos.DrawSphere( start, 1 );

            Gizmos.color = Color.red;
            Gizmos.DrawSphere( end, 1 );

        }

    }

}
