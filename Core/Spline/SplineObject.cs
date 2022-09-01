using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway.Spline {

    public class SplineObject : SnappableObject {

        #region Snaps

        public override bool AddSnapPoint(int snapIndex, SnappableObject snappableObject, int handleIndex) {
            return false;
        }

        public override bool RemoveSnapPoint(SplineSnapPoint splineSnapPoint) {
            return false;
        }

        #endregion

        [SerializeField, HideInInspector]
        protected SplineBezier path;

        public SplineBezier Path {
            get { return path; }
        }

        public int PathNumPoints {
            get { return path.NumPoints; }
        }

        public bool PathLocks(int handleIndex, bool locked) {
            if ( handleIndex < 0 || handleIndex >= PathNumPoints )
                return false;

            Path.PointLock( handleIndex, locked );
            return true;
        }

    }

}
