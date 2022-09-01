using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.AI;
using Roadway.Roads;
using Roadway.Junctions;
using Roadway.Spline;

namespace Roadway.Testing {

    public class Test : MonoBehaviour {

        RoadwayManager roadway;
        RoadwayManager Roadway {
            get {
                if ( roadway == null )
                    roadway = FindObjectOfType<RoadwayManager>();
                return roadway;
            }
        }

        public Path path;
        public Vector3 start;
        public Vector3 end;
        public float maxSearchRadius = 5000;

        void OnDrawGizmos() {

            Gizmos.color = Color.green;
            Gizmos.DrawSphere( start, 1 );

            Gizmos.color = Color.red;
            Gizmos.DrawSphere( end, 1 );

            if ( path == null || !path.IsComplete )
                return;

            Gizmos.color = Color.red;
            for ( int i = 0; i < path.path.Count - 1; i++ ) {
                Gizmos.DrawLine( path.path[ i ].Position, path.path[ i + 1 ].Position );
            }

        }

        public void DO() {
            path = RoadwayAI.GeneratePath( Roadway, start, end, maxSearchRadius );
        }

    }

}
