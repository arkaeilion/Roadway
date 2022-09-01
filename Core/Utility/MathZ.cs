using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway {

    public static class MathZ {

        public static Vector3 MiddlePointOfTwoVectors(Vector3 a, Vector3 b) {
            return PointBetweenTwoVectors( a, b, .5f );
        }
        public static Vector3 PointBetweenTwoVectors(Vector3 a, Vector3 b, float t) {
            return a + t * ( b - a );
        }

        public static float Sigmoid(float value) {
            return 1.0f / ( 1.0f + (float)Mathf.Exp( -value ) );
        }

        /// <summary> is a in front of b </summary>
        public static bool IsInFront(Vector3 a, Vector3 b, Vector3 forward) {
            double product = ( a.x - b.x ) * forward.x
                            + ( a.y - b.y ) * forward.y
                            + ( a.z - b.z ) * forward.z;
            return ( product > 0.0 );
        }

        /// <summary> Angle between a and b </summary>
        public static float Angle(Vector3 a, Vector3 b, Vector3 forward) {
            return ( a.x - b.x ) * forward.x
                    + ( a.y - b.y ) * forward.y
                    + ( a.z - b.z ) * forward.z;
        }

        /// <summary> Map a value in one range to another</summary>
        public static float Remap(float value, float from1, float to1, float from2, float to2) {
            return ( value - from1 ) / ( to1 - from1 ) * ( to2 - from2 ) + from2;
        }
    }

}
