using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Roadway.Spline {

    [System.Serializable]
    public class SplineBezier {

        [SerializeField, HideInInspector]
        public UnityEvent OnModified = new UnityEvent();

        // Called when the path is modified
        public void NotifyModified() {
            if ( OnModified != null ) {
                OnModified.Invoke();
            }
        }

        [SerializeField]
        List<Vector3> points;
        [SerializeField]
        List<bool> pointLocks;
        [SerializeField, HideInInspector]
        ControlMode controlMode;
        [SerializeField, HideInInspector]
        bool isClosed;
        [SerializeField, HideInInspector]
        bool isLocked = false;

        public SplineBezier() {
            controlMode = ControlMode.Automatic;
            points = new List<Vector3>();
            pointLocks = new List<bool>();
            isClosed = false;
        }

        public SplineBezier(SplineBezier sb) {
            controlMode = sb.ControlMode;
            points = sb.points;
            pointLocks = sb.pointLocks;
            isClosed = sb.IsClosed;
        }

        /// Get world space position of point
        public Vector3 Point(int i) {
            return points[ i ];
        }

        /// Set world space position of point
        public void Point(int i, Vector3 newPos) {
            points[ i ] = newPos;
        }

        /// Get world space position of point, but from end
        /// <summary> access points list starting from last element. passing i = 0 will give last element</summary>
        public Vector3 PointReverse(int i) {
            return points[ NumPoints - i - 1 ];
        }

        public Vector3 PointLast {
            get { return points[ NumPoints - 1 ]; }
        }

        public Vector3 PointFirst {
            get { return points[ 0 ]; }
        }

        /// Set world space position of point, but from end
        /// <summary> access points list starting from last element. passing i = 0 will give last element</summary>
        public void PointReverse(int i, Vector3 newPos) {
            points[ NumPoints - i - 1 ] = newPos;
        }

        /// Total number of points in the path (anchors and controls)
        public int NumPoints {
            get { return points.Count; }
        }

        /// Number of anchor points making up the path
        public int NumAnchorPoints {
            get { return ( IsClosed ) ? NumPoints / 3 : ( NumPoints + 2 ) / 3; }
        }

        /// Number of bezier curves making up this path
        public int NumSegments {
            get { return NumPoints / 3; }
        }

        /// The control mode determines the behaviour of control points.
        /// Possible modes are:
        /// Aligned = controls stay in straight line around their anchor
        /// Free = no constraints (use this if sharp corners are needed)
        /// Automatic = controls placed automatically to try make the path smooth
        public ControlMode ControlMode {
            get { return controlMode; }
            set {
                if ( controlMode != value ) {
                    controlMode = value;
                    if ( controlMode == ControlMode.Automatic ) {
                        AutoSetAllControlPoints();
                        NotifyModified();
                    }
                }
            }
        }

        /// If closed, path will loop back from end point to start point
        public bool IsClosed {
            get { return isClosed; }
            set {
                if ( isClosed != value ) {
                    isClosed = value;
                    UpdateClosedState();
                }
            }
        }

        public bool IsLocked {
            get { return isLocked; }
            set { isLocked = value; }
        }

        public bool PointLock(int i) {
            return pointLocks[ i ];
        }

        /// <summary> access Snaps list starting from last element. passing i = 0 will give last element</summary>
        public bool PointLockReverse(int i) {
            return pointLocks[ NumPoints - i - 1 ];
        }

        public void PointLock(int i, bool lockState) {
            pointLocks[ i ] = lockState;
        }

        public void Assimilate(int i, SplineBezier sb, int j) {
            if ( i == 0 ) {
                // add to start
                if ( j == 0 ) {
                    // add reversed
                    List<Vector3> newPoints = sb.points.GetRange( 1, sb.NumPoints - 1 );
                    newPoints.Reverse();
                    points.InsertRange( 0, newPoints );

                    List<bool> newLockPoints = sb.pointLocks.GetRange( 1, sb.NumPoints - 1 );
                    newLockPoints.Reverse();
                    pointLocks.InsertRange( 0, newLockPoints );
                } else {
                    points.InsertRange( 0, sb.points.GetRange( 0, sb.NumPoints - 1 ) );
                    pointLocks.InsertRange( 0, sb.pointLocks.GetRange( 0, sb.NumPoints - 1 ) );
                }
            } else {
                // add to end
                if ( j == 0 ) {
                    points.AddRange( sb.points.GetRange( 1, sb.NumPoints - 1 ) );
                    pointLocks.AddRange( sb.pointLocks.GetRange( 1, sb.NumPoints - 1 ) );
                } else {
                    // add reversed
                    List<Vector3> newPoints = sb.points.GetRange( 0, sb.NumPoints - 1 );
                    newPoints.Reverse();
                    points.AddRange( newPoints );

                    List<bool> newLockPoints = sb.pointLocks.GetRange( 0, sb.NumPoints - 1 );
                    newLockPoints.Reverse();
                    pointLocks.AddRange( newLockPoints );
                }
            }

            if ( controlMode == ControlMode.Automatic ) {
                AutoSetAllControlPoints();
            }

            NotifyModified();
        }

        public void DeleteAllPointsBefore(int index) {
            points.RemoveRange( 0, index );
            pointLocks.RemoveRange( 0, index );
            NotifyModified();
        }

        public void DeleteAllPointsAfter(int index) {
            points.RemoveRange( index + 1, NumPoints - ( index + 1 ) );
            pointLocks.RemoveRange( index + 1, NumPoints - ( index + 1 ) );
            NotifyModified();
        }

        public void AddSegment(Vector3 anchorPos, bool toStart = false) {
            if ( IsClosed || IsLocked )
                return;

            if ( NumPoints == 0 ) {
                points.Add( anchorPos );
                pointLocks.Add( false );
                NotifyModified();
                return;
            }

            if ( NumPoints == 1 ) {
                // add to end there is only one point so start and end are the same
                points.Add( points[ 0 ] - ( points[ 0 ] - anchorPos ) * .2f );
                points.Add( points[ 0 ] - ( points[ 0 ] - anchorPos ) * .8f );
                points.Add( anchorPos );
                pointLocks.AddRange( new List<bool> { false, false, false } );
                NotifyModified();
                return;
            }

            if ( toStart ) {
                // add to start
                Vector3 controlHandleB = points[ 0 ] * 2 - points[ 1 ];
                Vector3 controlHandleA = ( controlHandleB + anchorPos ) * .5f;
                points.InsertRange( 0, new Vector3[] { anchorPos, controlHandleA, controlHandleB } );
                pointLocks.InsertRange( 0, new List<bool> { false, false, false } );
                if ( controlMode == ControlMode.Automatic )
                    AutoSetAllAffectedControlPoints( 0 );
            } else {
                // add to end
                points.Add( points[ NumPoints - 1 ] * 2 - points[ NumPoints - 2 ] );
                points.Add( ( points[ NumPoints - 1 ] + anchorPos ) * .5f );
                points.Add( anchorPos );
                pointLocks.AddRange( new List<bool> { false, false, false } );
                if ( controlMode == ControlMode.Automatic )
                    AutoSetAllAffectedControlPoints( NumPoints - 1 );
            }
            NotifyModified();
        }

        public void DeleteSegment(int anchorIndex) {
            if ( IsLocked )
                return;

            if ( NumSegments > 2 || !IsClosed && NumSegments > 1 ) {
                if ( anchorIndex == 0 ) { // first 
                    if ( IsClosed )
                        PointReverse( 0, Point( 2 ) );
                    points.RemoveRange( 0, 3 );
                    pointLocks.RemoveRange( 0, 3 );
                } else if ( anchorIndex == NumPoints - 1 && !IsClosed ) { // last
                    points.RemoveRange( anchorIndex - 2, 3 );
                    pointLocks.RemoveRange( anchorIndex - 2, 3 );
                } else { // between
                    points.RemoveRange( anchorIndex - 1, 3 );
                    pointLocks.RemoveRange( anchorIndex - 1, 3 );
                }

                if ( controlMode == ControlMode.Automatic ) {
                    AutoSetAllControlPoints();
                }

                NotifyModified();
            }
        }

        /// <summary>returns index of new handle</summary>
        public int SplitSegment(Vector3 AnchorPos, int segmentIndex) {
            if ( IsLocked )
                return -1;

            points.InsertRange( segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, AnchorPos, Vector3.zero } );
            pointLocks.InsertRange( segmentIndex * 3 + 2, new List<bool> { false, false, false } );
            AutoSetAnchorControlPoints( segmentIndex * 3 + 3 );
            NotifyModified();
            return segmentIndex * 3 + 1;
        }

        public Vector3[] GetPointsInSegment(int segmentIndex) {
            segmentIndex = Mathf.Clamp( segmentIndex, 0, NumSegments - 1 );
            return new Vector3[] { points[ segmentIndex * 3 ],
                                    points[ segmentIndex * 3 + 1 ],
                                    points[ segmentIndex * 3 + 2 ],
                                    points[ LoopIndex( segmentIndex * 3 + 3 ) ] };
        }

        /// Add/remove the extra 2 controls required for a closed path
        void UpdateClosedState() {
            if ( IsClosed ) {
                points.Add( points[ NumPoints - 1 ] * 2 - points[ NumPoints - 2 ] );
                points.Add( points[ 0 ] * 2 - points[ 1 ] );

                if ( controlMode == ControlMode.Automatic ) {
                    AutoSetAnchorControlPoints( 0 );
                    AutoSetAnchorControlPoints( NumPoints - 3 );
                }

            } else {
                points.RemoveRange( NumPoints - 2, 2 );

                if ( controlMode == ControlMode.Automatic ) {
                    AutoSetStartAndEndControls();
                }
            }

            NotifyModified();
        }

        /// <summary> altMove - move all points by the delta move </summary>
        public void MovePoint(int i, Vector3 pos, bool altMove) {
            if ( IsLocked )
                return;

            Vector3 deltaMove = pos - Point( i );
            bool isAnchorPoint = i % 3 == 0;

            if ( isAnchorPoint && altMove ) {
                for ( int j = 0; j < NumPoints; j++ ) {
                    // if is anchor and not locked
                    // point can be moved
                    if ( j % 3 == 0 ) {
                        if ( !pointLocks[ j ] )
                            Point( j, Point( j ) + deltaMove );
                    } else {
                        // else this must be a control
                        // if it is locked don't move
                        // also if adjacent anchor is locked don't move
                        bool nextAnchorIsLocked = ( j + 1 ) < NumPoints && ( j + 1 ) % 3 == 0 && PointLock( j + 1 );
                        bool previousAnchorIsLocked = ( j - 1 ) >= 0 && ( j - 1 ) % 3 == 0 && PointLock( j - 1 );
                        if ( !nextAnchorIsLocked && !previousAnchorIsLocked )
                            Point( j, Point( j ) + deltaMove );
                    }
                }

                NotifyModified();

            } else {
                if ( isAnchorPoint || controlMode != ControlMode.Automatic ) {
                    Point( i, Point( i ) + deltaMove );

                    if ( controlMode == ControlMode.Automatic ) {
                        AutoSetAllAffectedControlPoints( i );
                    } else {

                        // this is an anchor
                        if ( isAnchorPoint ) {

                            if ( i + 1 < NumAnchorPoints || IsClosed ) {
                                int k = LoopIndex( i + 1 );
                                Point( k, Point( k ) + deltaMove );
                            }

                            if ( i - 1 >= 0 || IsClosed ) {
                                int k = LoopIndex( i - 1 );
                                Point( k, Point( k ) + deltaMove );
                            }

                        } else if ( ControlMode == ControlMode.Aligned ) {
                            bool nextPointIsAnchor = ( i + 1 ) % 3 == 0;
                            int correspondingControlIndex = ( nextPointIsAnchor ) ? i + 2 : i - 2;
                            int anchorIndex = ( nextPointIsAnchor ) ? i + 1 : i - 1;

                            if ( correspondingControlIndex >= 0 && correspondingControlIndex < NumPoints || IsClosed ) {
                                float dst = ( Point( LoopIndex( anchorIndex ) ) - Point( LoopIndex( correspondingControlIndex ) ) ).magnitude;
                                Vector3 dir = ( Point( LoopIndex( anchorIndex ) ) - Point( i ) ).normalized;
                                Point( LoopIndex( correspondingControlIndex ), Point( LoopIndex( anchorIndex ) ) + dir * dst );
                            }
                        }
                    }

                    NotifyModified();
                }

            }

        }

        int LoopIndex(int i) {
            return ( i + NumPoints ) % NumPoints;
        }

        void AutoSetAnchorControlPoints(int anchorIndex) {

            if ( NumPoints < 4 || ControlMode == ControlMode.Free ) {
                return;
            }

            if ( NumPoints == 4 ) {
                // only two anchors, make their handles straight
                if ( !PointLock( 0 ) )
                    Point( 1, ( Point( 0 ) - ( Point( 0 ) - Point( 3 ) ) * .2f ) );
                if ( !PointLockReverse( 0 ) )
                    Point( 2, ( Point( 3 ) + ( Point( 0 ) - Point( 3 ) ) * .2f ) );
                return;
            }

            if ( anchorIndex == 0 ) {
                if ( !PointLock( 0 ) ) {
                    // we want the point to stay perpendicular to where it was snapped
                    // but we do want to maintain a good distance
                    Vector3 dir0 = ( Point( 1 ) - Point( 0 ) ).normalized;
                    float dst0 = ( Point( 0 ) - Point( 3 ) ).magnitude;
                    Point( 1, Point( 0 ) + dir0 * ( dst0 * .5f ) );
                    return;
                }
            }

            if ( anchorIndex == NumPoints - 1 ) {
                if ( !PointLockReverse( 0 ) ) {
                    Vector3 dir1 = ( PointReverse( 1 ) - PointReverse( 0 ) ).normalized;
                    float dst1 = ( PointReverse( 0 ) - PointReverse( 3 ) ).magnitude;
                    PointReverse( 1, PointReverse( 0 ) + dir1 * ( dst1 * .5f ) );
                    return;
                }
            }

            Vector3 anchorPos = Point( anchorIndex );
            Vector3 dir = Vector3.zero;
            float[] neighborDistances = new float[ 2 ];

            if ( anchorIndex - 3 >= 0 || IsClosed ) {
                Vector3 offset = Point( LoopIndex( anchorIndex - 3 ) ) - anchorPos;
                dir += offset.normalized;
                neighborDistances[ 0 ] = offset.magnitude;
            }
            if ( anchorIndex + 3 >= 0 || IsClosed ) {
                Vector3 offset = Point( LoopIndex( anchorIndex + 3 ) ) - anchorPos;
                dir -= offset.normalized;
                neighborDistances[ 1 ] = -offset.magnitude;
            }

            dir.Normalize();

            for ( int i = 0; i < 2; i++ ) {
                int controlIndex = anchorIndex + i * 2 - 1;
                if ( controlIndex >= 0 && controlIndex < NumPoints || IsClosed ) {
                    Point( LoopIndex( controlIndex ), anchorPos + dir * neighborDistances[ i ] * .5f );
                }
            }
        }

        void AutoSetStartAndEndControls() {
            if ( NumPoints < 2 || IsClosed )
                return;

            if ( !PointLock( 0 ) )
                Point( 1, ( Point( 0 ) + Point( 2 ) ) * .5f );
            if ( !PointLockReverse( 0 ) )
                PointReverse( 1, ( PointReverse( 0 ) + PointReverse( 2 ) ) * .5f );
        }

        void AutoSetAllControlPoints() {
            if ( NumAnchorPoints > 2 ) {
                for ( int i = 0; i < NumPoints; i += 3 ) {
                    AutoSetAnchorControlPoints( i );
                }
            }
            AutoSetStartAndEndControls();
        }

        void AutoSetAllAffectedControlPoints(int updatedAnchorIndex) {
            for ( int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3 ) {
                if ( ( i >= 0 && i < NumPoints ) || IsClosed ) {
                    AutoSetAnchorControlPoints( LoopIndex( i ) );
                }
            }
            AutoSetStartAndEndControls();
        }

        public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1) {
            List<Vector3> evenlySpacedPoints = new List<Vector3>();
            evenlySpacedPoints.Add( Point( 0 ) );
            Vector3 previousPoint = Point( 0 );
            float dstSinceLastEvenPoint = 0;

            for ( int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++ ) {
                Vector3[] p = GetPointsInSegment( segmentIndex );
                float estimatedCurveLength = EstimateCurveLength( p[ 0 ], p[ 1 ], p[ 2 ], p[ 3 ] );
                int divisions = Mathf.CeilToInt( estimatedCurveLength * resolution * 10 );
                float t = 0;
                while ( t <= 1 ) {
                    t += 1f / divisions;
                    Vector3 pointOnCurve = EvaluateCubic( p[ 0 ], p[ 1 ], p[ 2 ], p[ 3 ], t );
                    dstSinceLastEvenPoint += Vector3.Distance( previousPoint, pointOnCurve );

                    while ( dstSinceLastEvenPoint >= spacing ) {
                        float overShootDst = dstSinceLastEvenPoint - spacing;
                        Vector3 newEvenlySpacedPoint = pointOnCurve + ( previousPoint - pointOnCurve ).normalized * overShootDst;
                        evenlySpacedPoints.Add( newEvenlySpacedPoint );
                        dstSinceLastEvenPoint = overShootDst;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = pointOnCurve;
                }
            }

            if ( !IsClosed )
                evenlySpacedPoints.Add( PointReverse( 0 ) );

            return evenlySpacedPoints.ToArray();
        }

        public float EstimateHandleIntervalAlongPath(int i) {
            if ( i == 0 )
                return 0;
            if ( i == NumPoints - 1 )
                return 1;

            float pathLengthTotal = EstimatePathLength();
            float pathLength = 0;
            int segmentIndex = i / 3;
            for ( int j = 0; j < segmentIndex; j++ ) {
                pathLength += EstimateCurveLength( GetPointsInSegment( j ) );
            }
            return pathLength / pathLengthTotal;
        }

        public float EstimatePathLength() {
            float length = 0;
            for ( int i = 0; i < NumSegments; i++ )
                length += EstimateCurveLength( GetPointsInSegment( i ) );
            return length;
        }

        public Vector3[] CalculateEvenlySpacedPointsInSegment(int segmentIndex, float spacing, float resolution = 1) {
            return CalculateEvenlySpacedPointsInSegment( GetPointsInSegment( segmentIndex ), segmentIndex, spacing, resolution );
        }

        public static Vector3[] CalculateEvenlySpacedPointsInSegment(Vector3[] segmentVectors, int segmentIndex, float spacing, float resolution = 1) {

            List<Vector3> evenlySpacedPoints = new List<Vector3>();

            evenlySpacedPoints.Add( segmentVectors[ 0 ] );
            Vector3 previousPoint = segmentVectors[ 0 ];
            float dstSinceLastEvenPoint = 0;

            float estimatedCurveLength = EstimateCurveLength( segmentVectors[ 0 ], segmentVectors[ 1 ], segmentVectors[ 2 ], segmentVectors[ 3 ] );
            int divisions = Mathf.CeilToInt( estimatedCurveLength * resolution * 10 );
            float t = 0;
            while ( t <= 1 ) {
                t += 1f / divisions;
                Vector3 pointOnCurve = EvaluateCubic( segmentVectors[ 0 ], segmentVectors[ 1 ], segmentVectors[ 2 ], segmentVectors[ 3 ], t );
                dstSinceLastEvenPoint += Vector3.Distance( previousPoint, pointOnCurve );

                while ( dstSinceLastEvenPoint >= spacing ) {
                    float overShootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + ( previousPoint - pointOnCurve ).normalized * overShootDst;
                    evenlySpacedPoints.Add( newEvenlySpacedPoint );
                    dstSinceLastEvenPoint = overShootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }

            return evenlySpacedPoints.ToArray();
        }

        /// <summary> returned as { middle, forward, left, right } </summary>
        public static Vector3[] CalculateSegmentVectors(Vector3[] segmentPoints, float t, float r = .01f) {
            Vector3 before = EvaluateCubic( segmentPoints[ 0 ], segmentPoints[ 1 ], segmentPoints[ 2 ], segmentPoints[ 3 ], Mathf.Max( t - r, 0 ) );
            Vector3 middle = EvaluateCubic( segmentPoints[ 0 ], segmentPoints[ 1 ], segmentPoints[ 2 ], segmentPoints[ 3 ], t );
            Vector3 after = EvaluateCubic( segmentPoints[ 0 ], segmentPoints[ 1 ], segmentPoints[ 2 ], segmentPoints[ 3 ], Mathf.Min( t + r, 1 ) );

            Vector3 forward = ( ( after - middle ) + ( middle - before ) ).normalized;
            Vector3 left = new Vector3( -forward.z, forward.y, forward.x );
            Vector3 right = new Vector3( forward.z, forward.y, -forward.x );

            return new Vector3[] { middle, forward, left, right };
        }

        public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t) {
            Vector3 p0 = Vector3.Lerp( a, b, t );
            Vector3 p1 = Vector3.Lerp( b, c, t );
            return Vector3.Lerp( p0, p1, t );
        }

        public Vector3 EvaluateCubic(int segmentIndex, float t) {
            Vector3[] p = GetPointsInSegment( segmentIndex );
            return EvaluateCubic( p[ 0 ], p[ 1 ], p[ 2 ], p[ 3 ], t );
        }

        public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
            Vector3 p0 = EvaluateQuadratic( a, b, c, t );
            Vector3 p1 = EvaluateQuadratic( b, c, d, t );
            return Vector3.Lerp( p0, p1, t );
        }

        public static float EstimateCurveLength(Vector3[] ps) {
            return EstimateCurveLength( ps[ 0 ], ps[ 1 ], ps[ 2 ], ps[ 3 ] );
        }

        public static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
            float controlNetLength = ( p0 - p1 ).magnitude + ( p1 - p2 ).magnitude + ( p2 - p3 ).magnitude;
            float estimatedCurveLength = ( p0 - p3 ).magnitude + controlNetLength / 2f;
            return estimatedCurveLength;
        }

    }

}
