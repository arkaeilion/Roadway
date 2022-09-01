using UnityEngine;

using Roadway.Roads;

namespace Roadway.Spline {

    [System.Serializable]
    public struct SplinePointDirectionalVectors {
        public Vector3 forwardVector;
        public Vector3 leftVector;
        public Vector3 rightVector;

        public SplinePointDirectionalVectors(Vector3[] points, int i) {
            forwardVector = Vector3.zero;
            if ( i < points.Length - 1 ) {
                forwardVector += points[ ( i + 1 ) % points.Length ] - points[ i ];
            }
            if ( i > 0 ) {
                forwardVector += points[ i ] - points[ ( i - 1 + points.Length ) % points.Length ];
            }
            forwardVector.Normalize();
            leftVector = new Vector3( -forwardVector.z, forwardVector.y, forwardVector.x );
            rightVector = new Vector3( forwardVector.z, forwardVector.y, -forwardVector.x );
        }

        public SplinePointDirectionalVectors(Vector3[] points, int i, Vector3 nextPoint, Vector3 previousPoint) {
            forwardVector = Vector3.zero;
            if ( i < points.Length - 1 ) {
                forwardVector += points[ ( i + 1 ) % points.Length ] - points[ i ];
            } else if ( !nextPoint.Equals( Vector3.negativeInfinity ) ) {
                forwardVector += nextPoint - points[ i ];
            }

            if ( i > 0 ) {
                forwardVector += points[ i ] - points[ ( i - 1 + points.Length ) % points.Length ];
            } else if ( !previousPoint.Equals( Vector3.negativeInfinity ) ) {
                forwardVector += points[ i ] - previousPoint;
            }

            forwardVector.Normalize();
            leftVector = new Vector3( -forwardVector.z, forwardVector.y, forwardVector.x );
            rightVector = new Vector3( forwardVector.z, forwardVector.y, -forwardVector.x );
        }
    }

}