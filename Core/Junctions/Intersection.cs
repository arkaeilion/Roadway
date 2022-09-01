using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.Events;

using Roadway.Roads;
using Roadway.Spline;

namespace Roadway.Junctions {

    [ExecuteInEditMode]
    public class Intersection : Junction {

        # region Modify Event 

        /// <summary> if suppressOnModify is false the event is triggered to modify those subscribed</summary>
        public override void NotifyModified(bool suppressOnModify = false) {
            CreateMesh();
            UpdateSnapPoints();
            UpdateSnappedObjects();
            UpdateLinks();
            UpdateBoundingBox();
            if ( !suppressOnModify && OnModified != null ) {
                OnModified.Invoke();
            }
        }

        void OnRoadSectionModified() {
            NotifyModified();
        }

        # endregion

        [SerializeField, HideInInspector]
        private IntersectionVectors interVectors;
        public IntersectionVectors InterVectors {
            get {
                if ( interVectors == null )
                    interVectors = new IntersectionVectors( Position, forwardVector, this );
                return interVectors;
            }
        }

        private Vector3 forwardVector {
            get { return transform.rotation * Vector3.forward; }
        }

        public void Init(Vector3 pos, bool suppressOnModify = false) {
            Undo.undoRedoPerformed -= UndoCallback;
            Undo.undoRedoPerformed += UndoCallback;

            transform.position = pos;

            for ( int i = 0; i < 4; i++ )
                snaps.Add( SplineSnapPoint.NewSplineSnapPoint( this, i, null, -1 ) );

            if ( !suppressOnModify )
                NotifyModified();
        }

        public void Init(Vector3 pos, SplineSnapPoint[] snapPoints) {
            if ( snapPoints.Length > 4 ) {
                Debug.LogWarning( string.Format( "Intersection was given more then 4 roads to attach, only the first 4 will be used. Array Length: {0}", snapPoints.Length ) );
            }

            if ( pos == Vector3.negativeInfinity || pos == Vector3.positiveInfinity )
                Init( Centroid(), true );
            else
                Init( pos, true );

            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( i < snapPoints.Length ) {
                    SnapOfHandle( i, snapPoints[ i ] );
                    RoadSection roadSec = Snap( i ).SnappedRoadSection;
                    if ( roadSec != null ) {
                        UnityEventTools.RemovePersistentListener( roadSec.OnModified, OnRoadSectionModified );
                        UnityEventTools.AddPersistentListener( roadSec.OnModified, OnRoadSectionModified );
                        roadSec.OnModified.SetPersistentListenerState( roadSec.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );
                    }
                }
            }
            NotifyModified();
        }

        public override bool AddSnapPoint(int snapIndex, SnappableObject snappableObject, int handleIndex) {
            if ( !( snappableObject is RoadSection ) )
                return false;

            // Debug.Log( "Intersection AddSnapPoint" );

            RoadSection toSnap = (RoadSection)snappableObject;
            SplineSnapPoint ssp = Snap( snapIndex );

            // Debug.Log(handleIndex);

            // Debug.Log( "ssp.anchorObject " + ssp.anchorObject.ToString() );

            if ( ssp.IsOccupied )
                return false;

            ssp.SetSnapped( toSnap, handleIndex );
            ssp.IsBlocked = true;
            ssp.used = true;

            toSnap.SnapOfHandle( handleIndex, ssp );
            toSnap.PathLocks( handleIndex, true );

            Snap( snapIndex, ssp );

            UnityEventTools.RemovePersistentListener( toSnap.OnModified, OnRoadSectionModified );
            UnityEventTools.AddPersistentListener( toSnap.OnModified, OnRoadSectionModified );
            toSnap.OnModified.SetPersistentListenerState( toSnap.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );

            NotifyModified();
            return true;
        }

        public override bool RemoveSnapPoint(SplineSnapPoint ssp) {
            if ( !snaps.Contains( ssp ) || ssp.snappedObject == null )
                return false;

            // Debug.Log( "Intersection RemoveSnapPoint" );

            RoadSection roadSection = ssp.SnappedRoadSection;
            if ( roadSection != null )
                UnityEventTools.RemovePersistentListener( roadSection.OnModified, OnRoadSectionModified );

            ssp.IsBlocked = false;
            ssp.SetSnapped( null, -1 );

            NotifyModified();
            return true;
            // return false;
        }

        public void CreateMesh() {
            if ( !MakeMesh )
                return;

            IntersectionVectors interVectorsL = new IntersectionVectors( Vector3.zero, Vector3.forward, this );
            Vector3 roadLiftVector = new Vector3( 0, RoadwaySettings.RoadLift, 0 );

            Vector3[] verts = new Vector3[ 4 ];
            Vector2[] uvs = new Vector2[ verts.Length ];
            int[] tris;

            verts[ 0 ] = interVectorsL.vA + roadLiftVector;
            verts[ 1 ] = interVectorsL.vB + roadLiftVector;
            verts[ 2 ] = interVectorsL.vC + roadLiftVector;
            verts[ 3 ] = interVectorsL.vD + roadLiftVector;

            tris = new int[] { 0, 1, 2, 1, 3, 2 };

            uvs[ 0 ] = new Vector2( 0, 1 );
            uvs[ 1 ] = new Vector2( 1, 1 );
            uvs[ 2 ] = new Vector2( 0, 0 );
            uvs[ 3 ] = new Vector2( 1, 0 );

            Mesh mesh = new Mesh();
            mesh.name = "Intersection";
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            TrySetMesh( mesh );
            TrySetCollider( mesh );
            TrySetMaterial( new Material( RoadwaySettings.IntersectionMat ) );
        }

        public RoadWidths RoadWidth(int i) {
            RoadWidths roadWidths = new RoadWidths( RoadwaySettings.LaneWidth, RoadwaySettings.LaneWidth );
            if ( i >= 0 && i < NumSnaps && Snap( i ) != null && Snap( i ).SnappedRoadSection != null ) {
                roadWidths = Snap( i ).SnappedRoadSection.WidthAtHandle( Snap( i ).snappedHandleIndex );
                if ( Snap( i ).snappedHandleIndex == 0 )
                    roadWidths.SwapSides();
            }
            return roadWidths;
        }

        /// <summary> center point between all road section handles </summary>
        public Vector3 Centroid() {
            Vector3 centroid = Vector3.zero;
            int j = 0; // use j for average so null aren't counted
            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( Snap( i ).SnappedRoadSection != null ) {
                    centroid += Snap( i ).SnappedHandle;
                    j++;
                }
            }
            centroid /= j;
            return centroid;
        }

        void UpdateSnapPoints() {
            if ( NumSnaps == 0 )
                return;

            interVectors = new IntersectionVectors( Position, forwardVector, this );

            for ( int i = 0; i < NumSnaps; i++ ) {
                Snap( i ).snapPoint = interVectors.rs[ i ];
            }
        }

        void UpdateSnappedObjects() {
            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( Snap( i ).SnappedRoadSection != null )
                    Snap( i ).SnappedRoadSection.SuppressedPathSnap( Snap( i ).snappedHandleIndex, Snap( i ).snapPoint,
                                                                        interVectors.dirs[ i ], interVectors.dsts[ i ], false );
            }
        }

        public override void UpdateLinks() {
            links.Clear();
            for ( int i = 0; i < NumSnaps; i++ ) {
                // needs to be a Road connected on this snap
                RoadSection originRoadSection = Snap( i ).SnappedRoadSection;
                if ( originRoadSection != null ) {
                    // only make links for roads that have lanes leading into the intersection
                    bool originLeft = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Left && originRoadSection.numLanesLeft > 0;
                    bool originRight = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Right && originRoadSection.numLanesRight > 0;
                    if ( originLeft || originRight ) {

                        for ( int j = 0; j < NumSnaps; j++ ) {
                            // needs to be a Road connected on this snap
                            RoadSection destinationRoadSection = Snap( j ).SnappedRoadSection;
                            // don't link same snaps
                            if ( destinationRoadSection != null && i != j ) {
                                // only make links for roads that have lanes leading out of the intersection
                                bool destinationLeft = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Left && destinationRoadSection.numLanesRight > 0;
                                bool destinationRight = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Right && destinationRoadSection.numLanesLeft > 0;
                                if ( destinationLeft || destinationRight ) {

                                    bool originIsStart = Snap( i ).snappedHandleIndex == 0;
                                    bool destinationIsStart = Snap( j ).snappedHandleIndex == 0;

                                    List<Lane> originLanes = originLeft ? originRoadSection.LeftLanes : originRoadSection.RightLanes;
                                    if ( originIsStart )
                                        originLanes = originLeft ? originRoadSection.RightLanes : originRoadSection.LeftLanes;

                                    List<Lane> destinationLanes = destinationLeft ? destinationRoadSection.RightLanes : destinationRoadSection.LeftLanes;
                                    if ( destinationIsStart )
                                        destinationLanes = destinationLeft ? destinationRoadSection.LeftLanes : destinationRoadSection.RightLanes;

                                    for ( int o = 0; o < originLanes.Count; o++ ) {
                                        for ( int d = 0; d < destinationLanes.Count; d++ ) {
                                            links.Add( new JunctionLaneLink( originLanes[ o ], originIsStart, i,
                                                                                destinationLanes[ d ], destinationIsStart, j ) );
                                        }
                                    }

                                }
                            }

                        }

                    }
                }
            }
        }

        void OnDestroy() {
            if ( Application.isEditor && !Application.isPlaying ) {
                for ( int i = 0; i < NumSnaps; i++ ) {
                    RoadSection roadSec = Snap( i ).SnappedRoadSection;
                    if ( roadSec != null ) {
                        UnityEventTools.RemovePersistentListener( roadSec.OnModified, OnRoadSectionModified );
                    }
                }

                if ( Roadway != null )
                    Roadway.RemoveJunction( this, false );
            }
        }

        void OnDrawGizmos() {

            // foreach ( JunctionConnection jc in junctionConnections ) {
            //         Gizmos.color = Color.red;
            //         Gizmos.DrawLine( jc.junctionA.Position, jc.junctionB.Position );
            // }


            // foreach ( IntersectionLink link in links ) {
            //     if ( link.snapIndex == 3 ) {
            //         Gizmos.color = Color.red;
            //         Gizmos.DrawSphere( link.OriginPoint, .3f );
            //         Gizmos.color = Color.cyan;
            //         Gizmos.DrawSphere( link.DestinationPoint, .3f );
            //     }
            // }

            // IntersectionVectors interVectors = new IntersectionVectors( Position, forwardVector, this );
            // float size = .3f;

            // Gizmos.color = Color.red;
            // Gizmos.DrawSphere( interVectors.vA, size );
            // Gizmos.color = Color.cyan;
            // Gizmos.DrawSphere( interVectors.vB, size );
            // Gizmos.color = Color.magenta;
            // Gizmos.DrawSphere( interVectors.vC, size );
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawSphere( interVectors.vD, size );
        }

    }

}