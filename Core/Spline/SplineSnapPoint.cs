using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;

namespace Roadway.Spline {

    [System.Serializable]
    public class SplineSnapPoint : ScriptableObject {

        public static SplineSnapPoint NewSplineSnapPoint(SnappableObject anchorObject = null,
                                                            int anchorHandleIndex = -1,
                                                            SnappableObject snappedObject = null,
                                                            int snappedHandleIndex = -1) {
            SplineSnapPoint splineSnapPoint = CreateInstance<SplineSnapPoint>();
            splineSnapPoint.Init( anchorObject, anchorHandleIndex, snappedObject, snappedHandleIndex );
            return splineSnapPoint;
        }

        // public static SplineSnapPoint[] NewSplineSnapPointArrayNull(int arrayLength) {
        //     SplineSnapPoint[] splineSnapPoints = new SplineSnapPoint[ arrayLength ];
        //     for ( int i = 0; i < arrayLength; i++ )
        //         splineSnapPoints[ i ] = null;
        //     return splineSnapPoints;
        // }

        // snappedObject is connedted to anchorObject

        [SerializeField]
        public SnappableObject anchorObject;
        [SerializeField]
        public int anchorHandleIndex;

        [SerializeField]
        public SnappableObject snappedObject;
        [SerializeField]
        public int snappedHandleIndex;


        // extra data for when used outside of the spline
        [SerializeField]
        public Vector3 snapPoint;
        [SerializeField]
        public int leftLaneIndex = -1; // set to the index of the lane if this snap is for that lane
        [SerializeField]
        public int rightLaneIndex = -1; // set to the index of the lane if this snap is for that lane
        [SerializeField]
        public bool used = true; // used to check if should be removed

        // use this to specify if something else is blocking the snap point
        // right now this would happen to the Spline snap points when the roadsection is connected to an intersection
        // this might be independent of the blockedObject;
        [SerializeField]
        public bool blocked = false;

        public void Init(SnappableObject anchorObject, int anchorHandleIndex, SnappableObject snappedObject, int snappedHandleIndex) {
            this.anchorObject = anchorObject;
            this.anchorHandleIndex = anchorHandleIndex;
            this.snappedObject = snappedObject;
            this.snappedHandleIndex = snappedHandleIndex;
        }


        /// <summary> true if both anchorObject and snappedObject are not null </summary>
        public bool IsOccupied {
            get { return anchorObject != null && snappedObject != null; }
        }

        public bool IsBlocked {
            get { return blocked; }
            set { blocked = value; }
        }

        public void SetAnchor(SnappableObject anchorObject, int anchorHandleIndex) {
            this.anchorObject = anchorObject;
            this.anchorHandleIndex = anchorHandleIndex;
        }

        public void SetSnapped(SnappableObject snappedObject, int snappedHandleIndex) {
            this.snappedObject = snappedObject;
            this.snappedHandleIndex = snappedHandleIndex;
        }

        public SnappableObject GetOther(SnappableObject notThisOne) {
            // has both 
            if ( IsOccupied ) {
                // one of the objects is the givenone
                if ( anchorObject == notThisOne || snappedObject == notThisOne ) {
                    return anchorObject == notThisOne ? snappedObject : anchorObject;
                }
            }
            return null;
        }

        public int GetHandleIndex(SnappableObject handleIndexOfThisOne) {
            if ( anchorObject == handleIndexOfThisOne )
                return anchorHandleIndex;
            if ( snappedObject == handleIndexOfThisOne )
                return snappedHandleIndex;
            return -1;
        }

        public int GetOtherHandleIndex (SnappableObject handleIndexOfNotThisOne) {
            if ( anchorObject == handleIndexOfNotThisOne )
                return snappedHandleIndex;
            if ( snappedObject == handleIndexOfNotThisOne )
                return anchorHandleIndex;
            return -1;
        }

        public Vector3 AnchorHandle {
            get {
                if ( AnchorRoadSection != null )
                    return AnchorRoadSection.Path.Point( anchorHandleIndex );
                return Vector3.zero;
            }
        }

        public Vector3 SnappedHandle {
            get {
                if ( SnappedRoadSection != null )
                    return SnappedRoadSection.Path.Point( snappedHandleIndex );
                return Vector3.zero;
            }
        }

        public RoadSection AnchorRoadSection {
            get {
                if ( anchorObject != null && anchorObject is RoadSection )
                    return (RoadSection)anchorObject;
                return null;
            }
            set {
                anchorObject = value;
            }
        }

        public RoadSection SnappedRoadSection {
            get {
                if ( snappedObject != null && snappedObject is RoadSection )
                    return (RoadSection)snappedObject;
                return null;
            }
            set {
                snappedObject = value;
            }
        }

        public void UpdateInfo(Vector3 snapPoint, int leftLaneIndex, int rightLaneIndex) {
            this.snapPoint = snapPoint;
            this.leftLaneIndex = leftLaneIndex;
            this.rightLaneIndex = rightLaneIndex;
            this.used = true;
        }

        public void UpdateInfo(Vector3 snapPoint) {
            this.snapPoint = snapPoint;
            this.used = true;
        }

        public bool AreSame(SnappableObject obj, int handleIndex, int leftLaneIndex, int rightLaneIndex) {
            bool isSameLeftLane = this.leftLaneIndex == leftLaneIndex;
            bool isSameRightLane = this.rightLaneIndex == rightLaneIndex;

            bool isSameObjAndHandleAnchor = this.anchorObject == obj && this.anchorHandleIndex == handleIndex;
            bool isSameObjAndHandleSnapped = this.snappedObject == obj && this.snappedHandleIndex == handleIndex;

            // bool result = ( isSameObjAndHandleAnchor || isSameObjAndHandleSnapped ) && isSameLeftLane && isSameRightLane;

            // if ( handleIndex == 3 && leftLaneIndex == -1 && rightLaneIndex == -1 ) {
            //     if ( (this.anchorHandleIndex == 3 || this.snappedHandleIndex == 3) && this.leftLaneIndex == -1 && this.rightLaneIndex == -1 ) {
            //         Debug.Log( "same : " + result );
            //     }
            // }

            return ( isSameObjAndHandleAnchor || isSameObjAndHandleSnapped ) && isSameLeftLane && isSameRightLane;
        }

        public bool IsLeft {
            get {
                return leftLaneIndex != -1;
            }
        }

        public bool IsRight {
            get {
                return rightLaneIndex != -1;
            }
        }

        public bool IsHandle {
            get {
                return !IsLeft && !IsRight;
            }
        }

        public bool IsAnchor(SnappableObject snappableObject) {
            return anchorObject == snappableObject;
        }

        public bool IsSnapped(SnappableObject snappableObject) {
            return snappedObject == snappableObject;
        }

        public bool Contains(SnappableObject snappableObject) {
            return anchorObject == snappableObject || snappedObject == snappableObject;
        }

    }

}
