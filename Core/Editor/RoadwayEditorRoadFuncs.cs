using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;
using Roadway.Spline;
using Roadway.Roads;
using Roadway.Junctions;

namespace RoadwayEditor {

    public class RoadwayEditorRoadFuncs {

        RoadwayEditor roadwayEditor;

        RoadwayManager Roadway {
            get { return roadwayEditor.roadway; }
        }

        Road Road(int roadIndex) {
            return roadwayEditor.Road( roadIndex );
        }

        RoadSection RoadSection(int roadIndex, int roadSectionIndex) {
            return roadwayEditor.RoadSection( roadIndex, roadSectionIndex );
        }

        Junction Junction(int juncIndex) {
            return roadwayEditor.Junction( juncIndex );
        }

        RoadwaySettings RoadwaySettings {
            get { return roadwayEditor.RoadwaySettings; }
        }

        RoadwayGUISettings RoadwayGUISettings {
            get { return roadwayEditor.RoadwayGUISettings; }
        }

        RoadwayEditorData RoadwayEditorData {
            get { return roadwayEditor.roadwayEditorData; }
        }


        public RoadwayEditorRoadFuncs(RoadwayEditor roadwayEditor) {
            this.roadwayEditor = roadwayEditor;
        }

        public void Draw(Vector3 mousePos) {
            for ( int r = 0; r < Roadway.NumRoads; r++ ) {
                Road road = Road( r );

                if ( RoadwayGUISettings.displayBoundingBox )
                    DrawRoadBoundingBox( road );

                for ( int s = 0; s < road.NumRoadSections; s++ ) {
                    RoadSection roadSection = RoadSection( r, s );

                    DrawSegments( roadSection );
                    DrawRoadSectionLanes( roadSection );

                    if ( RoadwayGUISettings.displayAnchorPoints ) {
                        for ( int i = 0; i < roadSection.PathNumPoints; i += 3 ) {
                            DrawHandle( r, s, i, roadSection, mousePos );
                        }
                    }

                    if ( RoadwayGUISettings.displayControlPoints &&
                            roadSection.PathNumPoints > 1 &&
                            roadSection.Path.ControlMode != ControlMode.Automatic ) {
                        for ( int i = 1; i < roadSection.PathNumPoints - 1; i += 3 ) {
                            DrawHandle( r, s, i, roadSection, mousePos );
                            DrawHandle( r, s, i + 1, roadSection, mousePos );
                        }
                    }

                    if ( RoadwayGUISettings.displayTools ) {
                        DrawTools( roadSection, mousePos );
                    }

                    if ( RoadwayGUISettings.displayBoundingBox )
                        DrawRoadSectionBoundingBox( roadSection );

                }

            }
        }

        public void DrawRoadSectionLanes(RoadSection roadSection) {
            if ( !RoadwayGUISettings.displayLaneFlowMarks )
                return;

            foreach ( Lane lane in roadSection.LeftLanes ) {
                Handles.color = lane.color;
                for ( int p = 0; p < lane.points.Count - 1; p++ ) {
                    Handles.DrawLine( lane.points[ p ], lane.points[ p + 1 ], 3 );
                }
                foreach ( ArrowVector arrow in lane.arrows ) {
                    Handles.DrawLine( arrow.head, arrow.armLeft, 2 );
                    Handles.DrawLine( arrow.head, arrow.armRight, 2 );
                }
            }

            foreach ( Lane lane in roadSection.RightLanes ) {
                Handles.color = lane.color;
                for ( int p = 0; p < lane.points.Count - 1; p++ ) {
                    Handles.DrawLine( lane.points[ p ], lane.points[ p + 1 ], 3 );
                }
                foreach ( ArrowVector arrow in lane.arrows ) {
                    Handles.DrawLine( arrow.head, arrow.armLeft, 2 );
                    Handles.DrawLine( arrow.head, arrow.armRight, 2 );
                }
            }

        }

        public void DrawSegments(RoadSection roadSection) {
            if ( !RoadwayGUISettings.displayPath )
                return;

            for ( int i = 0; i < roadSection.Path.NumSegments; i++ ) {
                Vector3[] points = roadSection.Path.GetPointsInSegment( i );
                if ( RoadwayGUISettings.displayControlPoints &&
                    roadSection.Path.ControlMode != ControlMode.Automatic ) {
                    Handles.color = RoadwayGUISettings.controlLine;
                    Handles.DrawLine( points[ 1 ], points[ 0 ] );
                    Handles.DrawLine( points[ 2 ], points[ 3 ] );
                }

                bool isSelected = i == RoadwayEditorData.selectedSegmentIndex && roadwayEditor.SelectedRoadSection == roadSection;
                bool isShift = Event.current.shift;
                Color segmentColor = ( isShift && isSelected && RoadwayEditorData.selectedSegmentIndexLastFrame ) ?
                                                                    RoadwayGUISettings.pathHighlighted : RoadwayGUISettings.path;
                Handles.DrawBezier( points[ 0 ], points[ 3 ], points[ 1 ], points[ 2 ], segmentColor, null, 4 );
            }
        }

        public void DrawRoadBoundingBox(Road road) {
            Handles.color = Color.white;
            Handles.DrawWireCube( road.Bounds.center, road.Bounds.size );
        }

        public void DrawRoadSectionBoundingBox(RoadSection roadSection) {
            Handles.color = Color.white;
            Handles.DrawWireCube( roadSection.Bounds.center, roadSection.Bounds.size );
        }

        void DrawHandle(int roadIndex, int roadSectionIndex, int handleIndex, RoadSection roadSection, Vector3 mousePos) {
            if ( roadSection == null || handleIndex >= roadSection.PathNumPoints )
                return;

            // always draw handle if only one
            SplineSnapPoint ssp = roadSection.SnapOfHandle( handleIndex );
            // if there is a SplineSnapPoint for this handle don't draw if it is snapped to something
            // unless that something is a junction
            if ( roadSection.PathNumPoints > 1 && ssp != null && ssp.IsSnapped( roadSection ) && !( ssp.anchorObject is Junction ) ) {
                return;
            }

            // change this to local space soon 
            Vector3 handlePosition = roadSection.Path.Point( handleIndex );

            bool isAnchorPoint = handleIndex % 3 == 0;
            bool isInteractive = isAnchorPoint ? RoadwayGUISettings.anchorHandlesAreInteractive : RoadwayGUISettings.controlHandlesAreInteractive;
            float handleSize = ( handleIndex % 3 == 0 ) ? RoadwayGUISettings.anchorDiameter : RoadwayGUISettings.controlDiameter;

            PathHandle.HandleColours handleColours = ( isAnchorPoint ) ? RoadwayEditorData.anchorHandleColours : RoadwayEditorData.controlHandleColours;
            if ( roadIndex == RoadwayEditorData.selectedRoadIndex &&
                    roadSectionIndex == RoadwayEditorData.selectedRoadSectionIndex &&
                    handleIndex == RoadwayEditorData.selectedHandleIndex ) {
                handleColours.defaultColour = ( isAnchorPoint ) ? RoadwayGUISettings.anchorSelected : RoadwayGUISettings.controlSelected;
            }
            Handles.CapFunction cap = Handles.SphereHandleCap;


            PathHandle.HandleInputType handleInputType;
            Vector3 newHandlePosition = PathHandle.DrawHandle( handlePosition, isInteractive, handleSize,
                                                                cap, handleColours, out handleInputType,
                                                                roadIndex, roadSectionIndex, -1, handleIndex );

            switch ( handleInputType ) {
                case PathHandle.HandleInputType.LMBDrag:
                    RoadwayEditorData.selectedHandleIndex = handleIndex;
                    RoadwayEditorData.selectedRoadIndex = roadIndex;
                    RoadwayEditorData.selectedRoadSectionIndex = roadSectionIndex;
                    RoadwayEditorData.draggingHandleIndex = handleIndex;
                    roadwayEditor.UpdateToolbar( roadSection );
                    roadwayEditor.Repaint();
                    break;
                case PathHandle.HandleInputType.LMBRelease:
                    // RoadwayEditorData.selectedHandleIndex = -1;
                    RoadwayEditorData.selectedHandleIndex = handleIndex;
                    RoadwayEditorData.selectedRoadIndex = roadIndex;
                    RoadwayEditorData.selectedRoadSectionIndex = roadSectionIndex;
                    RoadwayEditorData.releasedRoadIndex = roadIndex;
                    RoadwayEditorData.releasedRoadSectionIndex = roadSectionIndex;
                    RoadwayEditorData.releasedHandleIndex = handleIndex;
                    RoadwayEditorData.draggingHandleIndex = -1;
                    RoadwayEditorData.ResetDoNotSnapOnThis();
                    roadwayEditor.UpdateToolbar( roadSection );
                    roadwayEditor.Repaint();
                    break;
                case PathHandle.HandleInputType.LMBClick:
                    RoadwayEditorData.selectedHandleIndex = handleIndex;
                    RoadwayEditorData.selectedRoadIndex = roadIndex;
                    RoadwayEditorData.selectedRoadSectionIndex = roadSectionIndex;
                    RoadwayEditorData.draggingHandleIndex = -1;
                    roadwayEditor.UpdateToolbar( roadSection );
                    roadwayEditor.Repaint();
                    break;
                case PathHandle.HandleInputType.LMBPress:
                    if ( handleIndex != RoadwayEditorData.selectedHandleIndex ) {
                        RoadwayEditorData.selectedHandleIndex = -1;
                        roadwayEditor.Repaint();
                    }
                    if ( roadIndex != RoadwayEditorData.selectedRoadIndex ) {
                        RoadwayEditorData.selectedRoadIndex = -1;
                        RoadwayEditorData.selectedRoadSectionIndex = -1;
                        roadwayEditor.Repaint();
                    }
                    break;
            }

            if ( RoadwayEditorData.doNotMoveThis == roadSection ) {
                return;
            }

            if ( roadSection.Path.Point( handleIndex ) != newHandlePosition ) {
                Undo.RecordObject( roadSection, "Move Point" );
                // unsnap from current snap point
                RoadwayEditorData.StoreDoNotSnapOnThis( roadSection.UnsnapHandle( handleIndex ) );
                // move point
                roadSection.Path.MovePoint( handleIndex, mousePos, Event.current.alt );
                HandleSnapSearch( roadSection, handleIndex, mousePos, RoadwayEditorData.doNotSnapOnThis );
                EditorUtility.SetDirty( roadSection );
            }

        }

        void HandleSnapSearch(RoadSection roadSection, int handleIndex, Vector3 mousePos, SnappableObject noSnapOnThis) {

            // this has to be the selected roadsection and selected handle and this needs to be the handle being dragged
            if ( roadSection != roadwayEditor.SelectedRoadSection ||
                handleIndex != RoadwayEditorData.selectedHandleIndex ||
                RoadwayEditorData.draggingHandleIndex != handleIndex )
                return;

            // start search from previous selectedJunctionForSnapIndex or the selected junction
            int searchJuncIndex = ( RoadwayEditorData.selectedJunctionForSnapIndex != -1 ) ? RoadwayEditorData.selectedJunctionForSnapIndex :
                    ( RoadwayEditorData.selectedJunctionIndex != -1 ) ? RoadwayEditorData.selectedJunctionIndex : 0;

            // start search from previous
            int searchRoadIndex = ( RoadwayEditorData.selectedRoadForSnapIndex != -1 ) ? RoadwayEditorData.selectedRoadForSnapIndex :
                    ( RoadwayEditorData.selectedRoadIndex != -1 ) ? RoadwayEditorData.selectedRoadIndex : 0;

            // start search from previous
            int searchRoadSecIndex = ( RoadwayEditorData.selectedRoadSectionForSnapIndex != -1 ) ? RoadwayEditorData.selectedRoadSectionForSnapIndex :
                    ( RoadwayEditorData.selectedRoadSectionIndex != -1 ) ? RoadwayEditorData.selectedRoadSectionIndex : 0;

            Road sRoad = roadwayEditor.SelectedRoad;
            RoadSection sRoadSec = roadwayEditor.SelectedRoadSection;

            bool found = false;

            // search Junctions
            for ( int i = 0; i < Roadway.NumJunctions; i++ ) {
                int forLoopInterIndex = ( i + searchJuncIndex ) % Roadway.NumJunctions;
                Junction junc = Junction( forLoopInterIndex );
                if ( junc.Bounds.Contains( mousePos ) ) {
                    int closestSnapIndex = -1;
                    float dst = float.MaxValue;
                    for ( int s = 0; s < junc.NumSnaps; s++ ) {
                        // is snap is currently unoccupied
                        if ( !junc.IsSnapOccupied( s ) ) {
                            // is snap closer then other snaps;
                            float snapDST = Vector3.Distance( mousePos, junc.Snap( s ).snapPoint );
                            if ( snapDST < dst ) {
                                closestSnapIndex = s;
                                dst = snapDST;
                            }
                        }
                    }
                    if ( dst < RoadwayGUISettings.snapRange ) {
                        RoadwayEditorData.selectedJunctionForSnapIndex = forLoopInterIndex;
                        RoadwayEditorData.selectedSnapIndex = closestSnapIndex;
                        RoadwayEditorData.selectedRoadForSnapIndex = -1;
                        RoadwayEditorData.selectedRoadSectionForSnapIndex = -1;
                        found = true;
                        break;
                    }
                }
            }

            if ( !found ) {
                // search Roads
                for ( int i = 0; i < Roadway.NumRoads; i++ ) {
                    int forLoopRoadIndex = ( i + searchRoadIndex ) % Roadway.NumRoads;

                    // can snap to same Road
                    // if ( forLoopRoadIndex == RoadwayEditorData.selectedRoadIndex )
                    //     continue;

                    Road road = Road( forLoopRoadIndex );
                    if ( road.Bounds.Contains( mousePos ) ) {
                        for ( int j = 0; j < road.NumRoadSections; j++ ) {
                            int forLoopRoadSectionIndex = ( j + searchRoadSecIndex ) % road.NumRoadSections;

                            // can't snap to same RoadSection
                            if ( forLoopRoadIndex == RoadwayEditorData.selectedRoadIndex && forLoopRoadSectionIndex == RoadwayEditorData.selectedRoadSectionIndex )
                                continue;

                            RoadSection roadSec = road.RoadSection( forLoopRoadSectionIndex );
                            if ( roadSec.Bounds.Contains( mousePos ) ) {
                                int closestSnapIndex = -1;
                                float dst = float.MaxValue;
                                for ( int s = 0; s < roadSec.NumSnaps; s++ ) {

                                    if ( CanUseDrawSnap( sRoad.TravelDirection, road.TravelDirection, roadSec, s ) ) {
                                        // is snap closer then other snaps;
                                        if ( Vector3.Distance( mousePos, roadSec.SnapPoint( s ) ) < dst ) {
                                            closestSnapIndex = s;
                                            dst = Vector3.Distance( mousePos, roadSec.SnapPoint( s ) );
                                        }
                                    }

                                }
                                if ( dst < RoadwayGUISettings.snapRange ) {
                                    RoadwayEditorData.selectedRoadForSnapIndex = forLoopRoadIndex;
                                    RoadwayEditorData.selectedRoadSectionForSnapIndex = forLoopRoadSectionIndex;
                                    RoadwayEditorData.selectedSnapIndex = closestSnapIndex;
                                    RoadwayEditorData.selectedJunctionForSnapIndex = -1;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                    if ( found )
                        break;
                }
            }

            // if search was not successful
            if ( !found ) {
                RoadwayEditorData.selectedRoadForSnapIndex = -1;
                RoadwayEditorData.selectedRoadSectionForSnapIndex = -1;
                RoadwayEditorData.selectedJunctionForSnapIndex = -1;
                RoadwayEditorData.selectedSnapIndex = -1;
                return;
            }

            // if search was successful
            // else do the snap
            if ( RoadwayEditorData.selectedJunctionForSnapIndex != -1 ) {
                // snap on junction
                Junction foundForSnap = Junction( RoadwayEditorData.selectedJunctionForSnapIndex );

                if ( foundForSnap == noSnapOnThis )
                    return;

                foundForSnap.AddSnapPoint( RoadwayEditorData.selectedSnapIndex, roadSection, handleIndex );
                RoadwayEditorData.selectedJunctionIndex = RoadwayEditorData.selectedJunctionForSnapIndex;
                RoadwayEditorData.doNotMoveThis = roadSection;

            } else if ( RoadwayEditorData.selectedRoadForSnapIndex != -1 && RoadwayEditorData.selectedRoadSectionForSnapIndex == -1 ) {
                // snap on road
                // might be done in future
            } else if ( RoadwayEditorData.selectedRoadForSnapIndex != -1 && RoadwayEditorData.selectedRoadSectionForSnapIndex != -1 ) {
                // snap on road section
                RoadSection foundForSnap = RoadSection( RoadwayEditorData.selectedRoadForSnapIndex, RoadwayEditorData.selectedRoadSectionForSnapIndex );

                if ( foundForSnap == noSnapOnThis )
                    return;

                // if we are holding a modifying key (shift)
                // we can check if the roadsections can be joined
                // if they can't snap as normal

                if ( !JoinRoadSections( roadSection, foundForSnap, RoadwayEditorData.selectedSnapIndex, handleIndex ) ) {
                    foundForSnap.AddSnapPoint( RoadwayEditorData.selectedSnapIndex, roadSection, handleIndex );
                } else {
                    // it did join, need to set the selected road and roadsection
                    RoadwayEditorData.selectedRoadIndex = RoadwayEditorData.selectedRoadForSnapIndex;
                    RoadwayEditorData.selectedRoadSectionIndex = RoadwayEditorData.selectedRoadSectionForSnapIndex;
                }

            }

        }

        void DrawTools(RoadSection roadSection, Vector3 mousePos) {
            if ( roadwayEditor.SelectedRoad == null ||
                roadSection == null ||
                roadwayEditor.SelectedRoadSection == roadSection ||
                RoadwayEditorData.draggingHandleIndex == -1 )
                return;

            if ( !roadSection.Bounds.Contains( mousePos ) )
                return;

            Handles.color = RoadwayEditorData.snapColours.defaultColour;
            for ( int s = 0; s < roadSection.NumSnaps; s++ ) {
                if ( CanUseDrawSnap( roadwayEditor.SelectedRoad.TravelDirection, roadSection.Road.TravelDirection, roadSection, s ) )
                    Handles.DrawSolidDisc( roadSection.SnapPoint( s ), roadSection.up, RoadwayGUISettings.snapDiameter );
            }
        }

        bool CanUseDrawSnap(RoadTravelDirections sRoadTD, RoadTravelDirections roadTD, RoadSection roadSec, int snapId) {
            SplineSnapPoint snap = roadSec.Snap( snapId );

            if ( snap == null || snap.IsOccupied || snap.IsBlocked )
                return false;

            if ( sRoadTD == RoadTravelDirections.Bothways &&
                    roadTD == RoadTravelDirections.Bothways &&
                    snap.IsHandle ) {
                return true;
            }

            if ( sRoadTD == RoadTravelDirections.Oneway ) {

                if ( roadTD == RoadTravelDirections.Bothways && ( snap.IsLeft || snap.IsRight ) )
                    return true;

                if ( roadTD == RoadTravelDirections.Oneway )
                    return true;

            }

            return false;
        }

        bool JoinRoadSections(RoadSection activeRoadSection, RoadSection foundForSnapRoadSection, int selectedSnapIndex, int handleIndex) {
            if ( !Event.current.shift )
                return false;

            bool isSameTravelDirection = activeRoadSection.Road.TravelDirection == foundForSnapRoadSection.Road.TravelDirection;
            bool isSnapFirstOrLastAnchor = foundForSnapRoadSection.Snap( selectedSnapIndex ).IsHandle;
            bool isSameLeftLanes = activeRoadSection.numLanesLeft == foundForSnapRoadSection.numLanesLeft;
            bool isSameRightLanes = activeRoadSection.numLanesRight == foundForSnapRoadSection.numLanesRight;

            if ( isSameTravelDirection && isSnapFirstOrLastAnchor && isSameLeftLanes && isSameRightLanes ) {
                foundForSnapRoadSection.Assimilate( selectedSnapIndex, activeRoadSection, handleIndex );
                activeRoadSection.Road.RemoveRoadSection( activeRoadSection );
                return true;
            }

            return false;
        }

    }

}
