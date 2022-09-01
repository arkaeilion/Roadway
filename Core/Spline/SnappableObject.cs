using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway.Spline {

    public abstract class SnappableObject : MonoBehaviour {

        protected RoadwayManager roadway;

        protected RoadwayManager Roadway {
            get {
                if ( roadway == null )
                    roadway = FindObjectOfType<RoadwayManager>();
                return roadway;
            }
        }

        protected RoadwaySettings RoadwaySettings {
            get { return Roadway.RoadwaySettings; }
        }

        protected RoadwayGUISettings RoadwayGUISettings {
            get { return Roadway.RoadwayGUISettings; }
        }

        // [SerializeField, HideInInspector]
        [SerializeField, HideInInspector]
        protected List<SplineSnapPoint> snaps = new List<SplineSnapPoint>();

        public SplineSnapPoint Snap(int snapIndex) {
            return snaps[ snapIndex ];
        }

        public void Snap(int snapIndex, SplineSnapPoint newSnap) {
            snaps[ snapIndex ] = newSnap;
        }

        public bool SnapOfHandle(int handleIndex, SplineSnapPoint newSnap) {
            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( Snap( i ).AreSame( this, handleIndex, -1, -1 ) ) {
                    snaps[ i ] = newSnap;
                    return true;
                }
            }
            return false;
        }

        public SplineSnapPoint SnapOfHandle(int handleIndex) {
            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( Snap( i ).AreSame( this, handleIndex, -1, -1 ) ) {
                    return Snap( i );
                }
            }
            return null;
        }

        public int SnapOf(SnappableObject snappableObject) {
            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( Snap( i ).Contains( snappableObject ) ) {
                    return i;
                }
            }
            return -1;
        }

        public bool IsSnapOccupied(int snapIndex) {
            return snaps[ snapIndex ].IsOccupied;
        }

        public Vector3 SnapPoint(int snapIndex) {
            return snaps[ snapIndex ].snapPoint;
        }

        public int NumSnaps {
            get { return snaps.Count; }
        }

        public abstract bool AddSnapPoint(int snapIndex, SnappableObject snappableObject, int handleIndex);
        public abstract bool RemoveSnapPoint(SplineSnapPoint splineSnapPoint);

    }

}
