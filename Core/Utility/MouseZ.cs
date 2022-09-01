using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway.Spline;

namespace RoadwayEditor {

    public static class MouseZ {

        public static Vector3 MousePosition(float maxRayDepth, float backUpDepth, bool ignoreSnappableObjects = true) {
            Vector2 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay( mousePosition );

            if ( !ignoreSnappableObjects ) {
                // return first hit thing point
                RaycastHit hit;
                bool didHit = Physics.Raycast( ray, out hit, maxRayDepth );
                if ( didHit )
                    return hit.point;
            } else {
                // return first hit thing point that isn't a SnappableObject
                RaycastHit[] hits = Physics.RaycastAll( ray, maxRayDepth );
                for ( int i = 0; i < hits.Length; i++ ) {
                    if ( hits[ i ].transform.gameObject.GetComponent<SnappableObject>() == null )
                        return hits[ i ].point;
                }
            }

            // else else, return a point infrom of camera
            return HandleUtility.GUIPointToWorldRay( mousePosition ).GetPoint( backUpDepth );
        }
    }

}
