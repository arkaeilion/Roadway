using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway.Spline {

    [System.Serializable]
    public struct ArrowVector {
        public Vector3 head;
        public Vector3 armLeft;
        public Vector3 armRight;

        public ArrowVector(Vector3 a, Vector3 b, float t, bool reverse, float size) {
            head = MathZ.PointBetweenTwoVectors( a, b, t );
            Vector3 forward = ( b - a ).normalized;
            if ( reverse )
                forward = -forward;
            Vector3 left = new Vector3( -forward.z, forward.y, forward.x );
            Vector3 right = new Vector3( forward.z, forward.y, -forward.x );
            armLeft = head + ( -forward + left ) * size;
            armRight = head + ( -forward + right ) * size;
        }


    }

}
