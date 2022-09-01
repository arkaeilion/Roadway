using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Spline;

namespace Roadway.Roads {

    [System.Serializable]
    public class RoadWidths {

        // | = vert
        //
        // centerWidth = 0
        // |---||---|
        // |---||---|
        // |---||---|
        // |---||---|
        // but effectively the double up in the center can be treated as one 
        //
        // centerWidth > 0
        // |---|-|---|
        // |---|-|---|
        // |---|-|---|
        // |---|-|---|
        // leftExtraWidth - leftWidth --- centerLeft - centerRight --- rightWidth - rightExtraWidth

        // as some extra info the following fill in the dashed sections above
        // lanesLeft, center, lanesRight

        public float left;
        public float right;
        public float leftExtra;
        public float rightExtra;

        public float leftCenter;
        public float rightCenter;
        public float center;

        public float leftTransitionNumLanes;
        public float rightTransitionNumLanes;
        public float leftTransition;
        public float rightTransition;

        public bool leftIsInTransition;
        public bool rightIsInTransition;

        public float leftSide {
            get { return left + leftExtra + leftCenter; }
        }

        public float rightSide {
            get { return right + rightExtra + rightCenter; }
        }

        public float leftTotal {
            get { return left + leftExtra + leftCenter + leftTransition; }
        }

        public float rightTotal {
            get { return right + rightExtra + rightCenter + rightTransition; }
        }

        public RoadWidths(float left = 0, float right = 0, 
                            float leftExtra = 0, float rightExtra = 0, 
                            float leftCenter = 0, float rightCenter = 0, float center = 0) {
            this.left = left;
            this.right = right;
            this.leftExtra = leftExtra;
            this.rightExtra = rightExtra;
            this.leftCenter = leftCenter;
            this.rightCenter = rightCenter;
            this.center = center;

            leftTransitionNumLanes = 0;
            rightTransitionNumLanes = 0;
            leftTransition = 0;
            rightTransition = 0;
            leftIsInTransition = false;
            rightIsInTransition = false;
        }

        public RoadWidths(RoadSection roadSection, Vector3[] points, int pointIndex) {
            CalculateLanes( roadSection );
            CalculateCenters( roadSection, pointIndex / (float)points.Length );
            CalculateSideExtra( roadSection, points[ 0 ], points[ points.Length - 1 ], points[ pointIndex ] );
            CalculateTransition( roadSection, points[ points.Length - 1 ], points[ pointIndex ] );
        }

        public RoadWidths(RoadSection roadSection, int handleIndex) {
            CalculateLanes( roadSection );
            CalculateCenters( roadSection, roadSection.Path.EstimateHandleIntervalAlongPath( handleIndex ) );
            CalculateSideExtra( roadSection, roadSection.Path.PointFirst, roadSection.Path.PointLast, roadSection.Path.Point( handleIndex ) );
            CalculateTransition( roadSection, roadSection.Path.PointLast, roadSection.Path.Point( handleIndex ) );
        }

        private void CalculateLanes(RoadSection roadSection) {
            left = roadSection.numLanesLeft * roadSection.LaneWidth;
            right = roadSection.numLanesRight * roadSection.LaneWidth;

            bool isBoth = roadSection.TravelDirection == RoadTravelDirections.Bothways;
            bool isLeft = roadSection.DrivingSide == RoadDrivingSide.Left && roadSection.TravelDirection == RoadTravelDirections.Oneway;
            bool isRight = roadSection.DrivingSide == RoadDrivingSide.Right && roadSection.TravelDirection == RoadTravelDirections.Oneway;

            if ( isBoth ) {
                // leave as normal
            } else if ( isLeft ) {
                // left and right get half of left
                left = left / 2;
                right = left / 2;
            } else if ( isRight ) {
                left = right / 2;
                right = right / 2;
            }
        }

        private void CalculateCenters(RoadSection roadSection, float t) {
            leftCenter = roadSection.CenterWidthCurveLeft.Evaluate( t );
            rightCenter = roadSection.CenterWidthCurveRight.Evaluate( t );

            if ( roadSection.CenterWidthCurveMirror ) {
                if ( roadSection.DrivingSide == RoadDrivingSide.Right ) {
                    leftCenter = rightCenter;
                } else if ( roadSection.DrivingSide == RoadDrivingSide.Left ) {
                    rightCenter = leftCenter;
                }
            }

            center = leftCenter + rightCenter;
        }

        private void CalculateSideExtra(RoadSection roadSection, Vector3 start, Vector3 end, Vector3 point) {
            leftExtra = 0;
            rightExtra = 0;

            float startDst = Vector3.Distance( start, point );
            if ( startDst < roadSection.ExtraRoadWidthAtStartDst ) {
                float p = 1 - ( startDst / roadSection.ExtraRoadWidthAtStartDst );
                float e = roadSection.ExtraRoadWidthAtStart * ( p * p );
                leftExtra = e / 2;
                rightExtra = e / 2;
            }

            float endDst = Vector3.Distance( end, point );
            if ( endDst < roadSection.ExtraRoadWidthAtEndDst ) {
                float p = 1 - ( endDst / roadSection.ExtraRoadWidthAtEndDst );
                float e = roadSection.ExtraRoadWidthAtEnd * ( p * p );
                leftExtra = e / 2;
                rightExtra = e / 2;
            }
        }

        private void CalculateTransition(RoadSection roadSection, Vector3 endPoint, Vector3 currentPoint) {
            leftTransitionNumLanes = 0;
            rightTransitionNumLanes = 0;
            leftTransition = 0;
            rightTransition = 0;
            leftIsInTransition = false;
            rightIsInTransition = false;

            float dst = Vector3.Distance( endPoint, currentPoint );
            if ( dst > roadSection.TransitionDistance ) return;

            RoadSection nextRoadSection = roadSection.NextRoadSection;
            if ( nextRoadSection == null ) return;
            
            leftTransitionNumLanes = nextRoadSection.numLanesLeft - roadSection.numLanesLeft;
            rightTransitionNumLanes = nextRoadSection.numLanesRight - roadSection.numLanesRight;
            leftTransition = leftTransitionNumLanes * roadSection.LaneWidth;
            rightTransition = rightTransitionNumLanes * roadSection.LaneWidth;


            float grow = dst / roadSection.TransitionDistance;
            grow = 1 - ( grow * grow );

            float shrink = 1 - ( dst / roadSection.TransitionDistance );
            shrink = ( shrink * shrink );

            if ( leftTransition != 0 ) {
                leftTransition *= leftTransition > 0 ? grow : shrink;
                leftIsInTransition = true;
            }
            if ( rightTransition != 0 ) {
                rightTransition *= rightTransition > 0 ? grow : shrink;
                rightIsInTransition = true;
            }
        }

        public void SwapSides() {
            float tmp = left;
            float tmpExtra = leftExtra;
            float tmpCenter = leftCenter;

            left = right;
            leftExtra = rightExtra;
            leftCenter = rightCenter;

            right = tmp;
            rightExtra = tmpExtra;
            rightCenter = tmpCenter;
        }

    }

}
