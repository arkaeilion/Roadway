using System.Collections.Generic;
using UnityEngine;

namespace Roadway.AI {

    public class RandomFollow : MonoBehaviour {

        RoadwayManager roadway;
        RoadwayManager Roadway {
            get {
                if ( roadway == null )
                    roadway = FindObjectOfType<RoadwayManager>();
                return roadway;
            }
        }

        [SerializeField]
        Location current;

        [SerializeField]
        public Vector3 target;
        [SerializeField]
        public float speed = 10;
        [SerializeField]
        public float targetChangeDistanceSqr = 2;
        public float maxSearchRadius = 5000;
        public bool done = false;

        void Start() {
            current = RoadwayAI.FindClosestRoadwayLocation( roadway, transform.position, maxSearchRadius );
            if ( current.IsSet )
                target = current.lane.points[ current.pointIndex ];
        }

        void FixedUpdate() {
            if ( done || current == null || !current.IsSet )
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
                current = RoadwayAI.NextLocation( current );
                if ( current == null )
                    done = true;
                else
                    target = current.lane.points[ current.pointIndex ];
            }

        }

        void OnDrawGizmos() {

            // Gizmos.color = new Color( 1, 0, 0, .4f );
            // Gizmos.DrawSphere( transform.position, targetChangeDistanceSqr );

            // Gizmos.color = Color.cyan;
            // Gizmos.DrawSphere( target, .5f );

        }

    }

}
