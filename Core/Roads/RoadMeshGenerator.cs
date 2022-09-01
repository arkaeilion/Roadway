using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Roadway.Spline;

namespace Roadway.Roads {

    public class RoadMeshGenerator {

        public enum TriTypes { Center, Left, Right, LeftTransitionInner, LeftTransitionOuter, RightTransitionInner, RightTransitionOuter };

        private Vector3[] points;
        private Vector3 roadLiftVector;
        private Vector3[] verts;
        private Vector2[] uvs;

        private List<int> trisCenter;
        private List<int> trisLeft;
        private List<int> trisRight;

        private Vector3[] pointsTransition;
        private Vector3[] vertsTransition;
        private Vector2[] uvsTransition;
        private List<int> trisLeftTransitionInner;
        private List<int> trisLeftTransitionOuter;
        private List<int> trisRightTransitionInner;
        private List<int> trisRightTransitionOuter;
        private List<int> trisCenterTransition;

        private List<int[]> tris;
        public List<TriTypes> triTypes;
        public Mesh mesh;

        private int vertMulti;
        private int vertTransitionMulti;

        private RoadSection roadSection;
        private Vector3 previousPoint;
        private Vector3 nextPoint;
        private float x0;
        private float x1;

        private bool isTrisCenter;
        private bool isTrisLeft;
        private bool isTrisRight;

        private bool hasLeftTransition;
        private bool hasRightTransition;

        public RoadMeshGenerator(RoadSection roadSection) {
            this.roadSection = roadSection;
            previousPoint = roadSection.SplineSnapPointPosition( 0 );
            nextPoint = roadSection.SplineSnapPointPosition( roadSection.PathNumPoints - 1 );

            points = roadSection.CalculateEvenlySpacedPoints;
            roadLiftVector = new Vector3( 0, roadSection.RoadLift, 0 );
            vertMulti = VertMulti();

            RoadTransitionCheck();

            int numPoints = points.Length - pointsTransition.Length;
            verts = new Vector3[ numPoints * vertMulti ];
            uvs = new Vector2[ verts.Length ];

            trisCenter = new List<int>();
            trisLeft = new List<int>();
            trisRight = new List<int>();

            trisLeftTransitionInner = new List<int>();
            trisLeftTransitionOuter = new List<int>();
            trisRightTransitionInner = new List<int>();
            trisRightTransitionOuter = new List<int>();
            trisCenterTransition = new List<int>();

            mesh = new Mesh();
            mesh.name = "Road Section Mesh";

            tris = new List<int[]>();
            triTypes = new List<TriTypes>();

            x0 = roadSection.UVsXValReverse ? 1 : 0;
            x1 = roadSection.UVsXValReverse ? 0 : 1;

            ProcessMesh();
            FinalizeMesh();
        }

        private void RoadTransitionCheck() {
            hasLeftTransition = false;
            hasRightTransition = false;
            pointsTransition = new Vector3[ 0 ];

            RoadSection nextRoadSection = roadSection.NextRoadSection;
            if ( nextRoadSection == null ) return;

            if ( nextRoadSection.numLanesLeft != roadSection.numLanesLeft )
                hasLeftTransition = true;

            if ( nextRoadSection.numLanesRight != roadSection.numLanesRight )
                hasRightTransition = true;

            if ( hasLeftTransition || hasRightTransition ) {
                List<Vector3> ps = new List<Vector3>();
                for ( int i = points.Length - 1; i >= 0; i-- ) {
                    float dst = Vector3.Distance( roadSection.Path.PointLast, points[ i ] );
                    if ( dst < roadSection.TransitionDistance ) {
                        ps.Add( points[ i ] );
                    } else {
                        // ps.Add( points[ i ] );
                        break;
                    }
                }

                ps.Reverse();
                pointsTransition = ps.ToArray();

                if ( hasLeftTransition && hasRightTransition && isTrisCenter ) { // both side and center
                    vertTransitionMulti = 16;
                } else if ( hasLeftTransition && hasRightTransition ) { // both sides and no center
                    vertTransitionMulti = 15;
                } else { // one side, can't have center
                    vertTransitionMulti = 8;
                }

                vertsTransition = new Vector3[ pointsTransition.Length * vertTransitionMulti ];
                uvsTransition = new Vector2[ vertsTransition.Length ];

                // remove points in transition from points array
                // List<Vector3> pointsTMP = new List<Vector3>( points );
                // int from = points.Length - pointsTransition.Length;
                // pointsTMP.RemoveRange( from, pointsTransition.Length );
                // points = pointsTMP.ToArray();
            }
        }

        private void ProcessMesh() {
            int vertIndex = 0;
            int vertIndexTransition = 0;

            for ( int i = 0; i < points.Length; i++ ) {
                RoadWidths widths = new RoadWidths( roadSection, points, i );

                if ( widths.leftIsInTransition || widths.rightIsInTransition )
                    break;

                ProcessVerts( i, vertIndex, widths );
                ProcessUVs( i, vertIndex, widths );
                ProcessTris( i, vertIndex, widths );
                vertIndex += vertMulti;
            }

            for ( int i = 0; i < pointsTransition.Length; i++ ) {
                RoadWidths widths = new RoadWidths( roadSection, points, i );

                ProcessVertsTransition( i, vertIndexTransition, widths );
                ProcessUVsTransition( i, vertIndexTransition, widths );
                ProcessTrisTransition( i, vertIndexTransition, widths );
                vertIndexTransition += vertTransitionMulti;
            }

        }

        private void ProcessVerts(int i, int vertIndex, RoadWidths widths) {
            SplinePointDirectionalVectors vectors = new SplinePointDirectionalVectors( points, i, nextPoint, previousPoint );

            Vector3 vertLeftCenter = ( points[ i ] + vectors.leftVector * widths.leftCenter ) + roadLiftVector;
            Vector3 vertRightCenter = ( points[ i ] + vectors.rightVector * widths.rightCenter ) + roadLiftVector;

            Vector3 vertLeft = ( points[ i ] + vectors.leftVector * widths.leftSide ) + roadLiftVector;
            Vector3 vertRight = ( points[ i ] + vectors.rightVector * widths.rightSide ) + roadLiftVector;

            if ( vertMulti == 4 ) {
                verts[ vertIndex ] = vertLeft;
                verts[ vertIndex + 1 ] = vertLeftCenter;
                verts[ vertIndex + 2 ] = vertRightCenter;
                verts[ vertIndex + 3 ] = vertRight;
            } else if ( vertMulti == 3 ) {
                verts[ vertIndex ] = vertLeft;
                verts[ vertIndex + 1 ] = vertLeftCenter;
                verts[ vertIndex + 2 ] = vertRight;
            } else { // 2
                verts[ vertIndex ] = vertLeft;
                verts[ vertIndex + 1 ] = vertRight;
            }
        }

        private void ProcessUVs(int i, int vertIndex, RoadWidths widths) {
            // 0 - 1
            // liner display texture over whole mesh
            float uvYVal = i / (float)( points.Length - 1 );
            // 0 - 1 - 0 
            // wrap uv coords back to 0 for smooth transition
            if ( roadSection.WrapUVs )
                uvYVal = 1 - Mathf.Abs( 2 * uvYVal - 1 );

            if ( vertMulti == 4 ) {
                uvs[ vertIndex ] = new Vector2( x0, uvYVal );
                uvs[ vertIndex + 1 ] = new Vector2( x1, uvYVal );
                uvs[ vertIndex + 2 ] = new Vector2( x0, uvYVal );
                uvs[ vertIndex + 3 ] = new Vector2( x1, uvYVal );
            } else if ( vertMulti == 3 ) {
                uvs[ vertIndex ] = new Vector2( x0, uvYVal );
                uvs[ vertIndex + 1 ] = new Vector2( x1, uvYVal );
                uvs[ vertIndex + 2 ] = new Vector2( x0, uvYVal );
            } else { // 2
                uvs[ vertIndex ] = new Vector2( x0, uvYVal );
                uvs[ vertIndex + 1 ] = new Vector2( x1, uvYVal );
            }
        }

        private void ProcessTris(int i, int vertIndex, RoadWidths widths) {

            if ( !( i < points.Length - pointsTransition.Length - 1 || roadSection.Path.IsClosed ) )
                return;

            if ( vertMulti == 4 ) {

                trisLeft.Add( vertIndex );
                trisLeft.Add( ( vertIndex + 4 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );

                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 4 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 5 ) % verts.Length );

                trisRight.Add( ( vertIndex + 2 ) % verts.Length );
                trisRight.Add( ( vertIndex + 6 ) % verts.Length );
                trisRight.Add( ( vertIndex + 3 ) % verts.Length );

                trisRight.Add( ( vertIndex + 3 ) % verts.Length );
                trisRight.Add( ( vertIndex + 6 ) % verts.Length );
                trisRight.Add( ( vertIndex + 7 ) % verts.Length );

                trisCenter.Add( ( vertIndex + 1 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 5 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );

                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 5 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 6 ) % verts.Length );

            } else if ( vertMulti == 3 ) {

                trisLeft.Add( vertIndex );
                trisLeft.Add( ( vertIndex + 3 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );

                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 3 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 4 ) % verts.Length );

                trisRight.Add( ( vertIndex + 1 ) % verts.Length );
                trisRight.Add( ( vertIndex + 4 ) % verts.Length );
                trisRight.Add( ( vertIndex + 2 ) % verts.Length );

                trisRight.Add( ( vertIndex + 2 ) % verts.Length );
                trisRight.Add( ( vertIndex + 4 ) % verts.Length );
                trisRight.Add( ( vertIndex + 5 ) % verts.Length );

            } else { // 2

                trisCenter.Add( vertIndex );
                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 1 ) % verts.Length );

                trisCenter.Add( ( vertIndex + 1 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 3 ) % verts.Length );

            }

        }

        private void FinalizeMesh() {

            mesh.vertices = verts;
            mesh.uv = uvs;

            if ( trisCenter.Count > 0 ) {
                tris.Add( trisCenter.ToArray() );
                triTypes.Add( TriTypes.Center );
            }
            if ( trisLeft.Count > 0 ) {
                tris.Add( trisLeft.ToArray() );
                triTypes.Add( TriTypes.Left );
            }
            if ( trisRight.Count > 0 ) {
                tris.Add( trisRight.ToArray() );
                triTypes.Add( TriTypes.Right );
            }
            if ( trisLeftTransitionInner.Count > 0 ) {
                tris.Add( trisLeftTransitionInner.ToArray() );
                triTypes.Add( TriTypes.LeftTransitionInner );
            }
            if ( trisLeftTransitionOuter.Count > 0 ) {
                tris.Add( trisLeftTransitionOuter.ToArray() );
                triTypes.Add( TriTypes.LeftTransitionOuter );
            }
            if ( trisRightTransitionInner.Count > 0 ) {
                tris.Add( trisRightTransitionInner.ToArray() );
                triTypes.Add( TriTypes.RightTransitionInner );
            }
            if ( trisRightTransitionOuter.Count > 0 ) {
                tris.Add( trisRightTransitionOuter.ToArray() );
                triTypes.Add( TriTypes.RightTransitionOuter );
            }

            mesh.subMeshCount = tris.Count;
            for ( int i = 0; i < tris.Count; i++ )
                mesh.SetTriangles( tris[ i ], i );

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private int VertMulti() {
            isTrisCenter = false;
            isTrisLeft = false;
            isTrisRight = false;

            // check if tris should be made for this submesh
            if ( roadSection.numLanesLeft > 0 ) {
                if ( roadSection.TravelDirection == RoadTravelDirections.Bothways ) {
                    isTrisLeft = true;
                } else if ( roadSection.TravelDirection == RoadTravelDirections.Oneway && roadSection.DrivingSide == RoadDrivingSide.Left ) {
                    isTrisLeft = true;
                }
            }
            // check if tris should be made for this submesh
            if ( roadSection.numLanesRight > 0 ) {
                if ( roadSection.TravelDirection == RoadTravelDirections.Bothways ) {
                    isTrisRight = true;
                } else if ( roadSection.TravelDirection == RoadTravelDirections.Oneway && roadSection.DrivingSide == RoadDrivingSide.Right ) {
                    isTrisRight = true;
                }
            }

            if ( roadSection.TravelDirection == RoadTravelDirections.Bothways ) {
                // might be better way to do this in a single loop
                for ( int i = 0; i < points.Length; i++ ) {
                    RoadWidths widths = new RoadWidths( roadSection, points, i );
                    // check if tris should be made for this submesh
                    if ( widths.center > 0 ) {
                        isTrisCenter = true;
                        break;
                    }
                }
            }

            int vertMulti = 2;
            if ( isTrisLeft && isTrisRight ) vertMulti = 3;
            if ( isTrisCenter ) vertMulti = 4;

            return vertMulti;
        }




        private void ProcessVertsTransition(int i, int vertIndex, RoadWidths widths) {
            SplinePointDirectionalVectors vectors = new SplinePointDirectionalVectors( points, i, nextPoint, previousPoint );

            Vector3 vertLeftCenter = ( points[ i ] + vectors.leftVector * widths.leftCenter ) + roadLiftVector;
            Vector3 vertRightCenter = ( points[ i ] + vectors.rightVector * widths.rightCenter ) + roadLiftVector;

            float left = widths.leftSide - ( roadSection.LaneWidth * .1f);
            float right = widths.rightSide - ( roadSection.LaneWidth * .1f);

            Vector3 vertLeft = ( points[ i ] + vectors.leftVector * left ) + roadLiftVector;
            Vector3 vertRight = ( points[ i ] + vectors.rightVector * right ) + roadLiftVector;

            // Vector3 vertLeftTransition = ( points[ i ] + vectors.leftVector * widths.leftTotal ) + roadLiftVector;
            // Vector3 vertRightTransition = ( points[ i ] + vectors.rightVector * widths.rightTotal ) + roadLiftVector;

            if ( vertTransitionMulti == 8 ) { // one side
                vertsTransition[ vertIndex ] = vertLeft;
                vertsTransition[ vertIndex + 1 ] = vertLeftCenter;
                vertsTransition[ vertIndex + 2 ] = vertRightCenter;
                vertsTransition[ vertIndex + 3 ] = vertRight;
            } else if ( vertTransitionMulti == 15 ) { // both side no center
                vertsTransition[ vertIndex ] = vertLeft;
                vertsTransition[ vertIndex + 1 ] = vertLeftCenter;
                vertsTransition[ vertIndex + 2 ] = vertRight;
            } else if ( vertTransitionMulti == 16 ) { // both side with center
                vertsTransition[ vertIndex ] = vertLeft;
                vertsTransition[ vertIndex + 1 ] = vertRight;
            } else {
                throw new System.Exception( "vertTransitionMulti is un expected number" );
            }
        }

        private void ProcessUVsTransition(int i, int vertIndex, RoadWidths widths) {
            // 0 - 1
            // liner display texture over whole mesh
            float uvYVal = i / (float)( points.Length - 1 );
            // 0 - 1 - 0 
            // wrap uv coords back to 0 for smooth transition
            if ( roadSection.WrapUVs )
                uvYVal = 1 - Mathf.Abs( 2 * uvYVal - 1 );

            if ( vertMulti == 4 ) {
                uvsTransition[ vertIndex ] = new Vector2( x0, uvYVal );
                uvsTransition[ vertIndex + 1 ] = new Vector2( x1, uvYVal );
                uvsTransition[ vertIndex + 2 ] = new Vector2( x0, uvYVal );
                uvsTransition[ vertIndex + 3 ] = new Vector2( x1, uvYVal );
            } else if ( vertMulti == 3 ) {
                uvsTransition[ vertIndex ] = new Vector2( x0, uvYVal );
                uvsTransition[ vertIndex + 1 ] = new Vector2( x1, uvYVal );
                uvsTransition[ vertIndex + 2 ] = new Vector2( x0, uvYVal );
            } else { // 2
                uvsTransition[ vertIndex ] = new Vector2( x0, uvYVal );
                uvsTransition[ vertIndex + 1 ] = new Vector2( x1, uvYVal );
            }
        }

        private void ProcessTrisTransition(int i, int vertIndex, RoadWidths widths) {

            if ( i >= points.Length - 1 || !roadSection.Path.IsClosed )
                return;

            if ( vertMulti == 4 ) {

                trisLeft.Add( vertIndex );
                trisLeft.Add( ( vertIndex + 4 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );

                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 4 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 5 ) % verts.Length );

                trisRight.Add( ( vertIndex + 2 ) % verts.Length );
                trisRight.Add( ( vertIndex + 6 ) % verts.Length );
                trisRight.Add( ( vertIndex + 3 ) % verts.Length );

                trisRight.Add( ( vertIndex + 3 ) % verts.Length );
                trisRight.Add( ( vertIndex + 6 ) % verts.Length );
                trisRight.Add( ( vertIndex + 7 ) % verts.Length );

                trisCenter.Add( ( vertIndex + 1 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 5 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );

                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 5 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 6 ) % verts.Length );

            } else if ( vertMulti == 3 ) {

                trisLeft.Add( vertIndex );
                trisLeft.Add( ( vertIndex + 3 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );

                trisLeft.Add( ( vertIndex + 1 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 3 ) % verts.Length );
                trisLeft.Add( ( vertIndex + 4 ) % verts.Length );

                trisRight.Add( ( vertIndex + 1 ) % verts.Length );
                trisRight.Add( ( vertIndex + 4 ) % verts.Length );
                trisRight.Add( ( vertIndex + 2 ) % verts.Length );

                trisRight.Add( ( vertIndex + 2 ) % verts.Length );
                trisRight.Add( ( vertIndex + 4 ) % verts.Length );
                trisRight.Add( ( vertIndex + 5 ) % verts.Length );

            } else { // 2

                trisCenter.Add( vertIndex );
                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 1 ) % verts.Length );

                trisCenter.Add( ( vertIndex + 1 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 2 ) % verts.Length );
                trisCenter.Add( ( vertIndex + 3 ) % verts.Length );

            }

        }



    }
}
