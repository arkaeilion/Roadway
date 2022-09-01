using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor.Events;

using Roadway.Roads;
using Roadway.Spline;

namespace Roadway.Junctions {

    public class Roundabout : Junction {

        #region Modify Event 

        // Called when the Roundabout is modified
        public override void NotifyModified(bool suppressOnModify = false) {
            // CreateMesh();
            // UpdateSnapPoints();
            UpdateBoundingBox();
            if ( !suppressOnModify && OnModified != null ) {
                OnModified.Invoke();
            }
        }

        # endregion

        public override bool AddSnapPoint(int snapIndex, SnappableObject snappableObject, int handleIndex) {
            if ( snappableObject is RoadSection ) {
                RoadSection roadSec = (RoadSection)snappableObject;
                SplineSnapPoint splineSnapPoint = Snap( snapIndex );
                splineSnapPoint.SetSnapped( roadSec, handleIndex );
                roadSec.SnapOfHandle( handleIndex, splineSnapPoint );
                SnapOfHandle( snapIndex, splineSnapPoint );
                splineSnapPoint.IsBlocked = true;

                NotifyModified();
                return true;
            }
            return false;
        }

        public override bool RemoveSnapPoint(SplineSnapPoint splineSnapPoint) {
            if ( snaps.Contains( splineSnapPoint ) ) {
                if ( splineSnapPoint.SnappedRoadSection != null )
                    splineSnapPoint.IsBlocked = false;
                splineSnapPoint.SetSnapped( null, -1 );
                NotifyModified();
                return true;
            }
            return false;
        }

    }

}