using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;
using Roadway.Spline;
using Roadway.Roads;
using Roadway.Junctions;

namespace RoadwayEditor {

    public class RoadwayEditorJunctionFuncs {

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

        public RoadwayEditorJunctionFuncs(RoadwayEditor roadwayEditor) {
            this.roadwayEditor = roadwayEditor;
        }

        public void Draw(Vector3 mousePos) {
            for ( int i = 0; i < Roadway.NumJunctions; i++ ) {
                Junction junc = Junction( i );

                if ( RoadwayGUISettings.displayAnchorPoints ) {
                    DrawHandle( i, junc, mousePos );
                    DrawTools( junc );
                }

                if ( RoadwayGUISettings.displayJunctionConnections )
                    DrawJunctionConnections( junc );

                if ( RoadwayGUISettings.displayBoundingBox )
                    DrawJunctionBoundingBox( junc );
            }
        }

        public void DrawJunctionConnections(Junction junction) {
            Handles.color = Color.cyan;
            for ( int j = 0; j < junction.numJunctionConnections; j++ ) {
                JunctionConnection jc = junction.JunctionConnection( j );
                Handles.DrawLine( jc.junctionA.SnapPoint( jc.junctionASnapIndex ), jc.junctionB.SnapPoint( jc.junctionBSnapIndex ), 3 );
            }
        }

        public void DrawJunctionBoundingBox(Junction junction) {
            Handles.color = Color.white;
            Handles.DrawWireCube( ( junction ).Bounds.center, ( junction ).Bounds.size );
        }

        void DrawTools(Junction junction) {

            if ( junction == roadwayEditor.SelectedJunction ) {

                // rotation
                Handles.color = RoadwayEditorData.snapColours.defaultColour;
                Quaternion rot = Handles.Disc( junction.Rotation, junction.Position, junction.up, 8, false, .1f );
                if ( junction.Rotation != rot ) {
                    junction.Rotation = rot;
                    EditorUtility.SetDirty( junction );
                }

            }

            bool isDragging = RoadwayEditorData.draggingHandleIndex != -1;
            bool isDragHandle = RoadwayEditorData.draggingHandleIndex == RoadwayEditorData.selectedHandleIndex;
            bool isSelectedJunc = junction == roadwayEditor.SelectedJunction;
            bool isSelectedJuncForSnap = junction == Junction( RoadwayEditorData.selectedJunctionForSnapIndex );
            bool isSnap = RoadwayEditorData.selectedSnapIndex != -1;

            if ( RoadwayEditorData.selectedHandleIndex >= 0 &&
                    roadwayEditor.SelectedRoadSection != null &&
                    RoadwayEditorData.selectedHandleIndex < roadwayEditor.SelectedRoadSection.PathNumPoints ) {

                Vector3 pos = roadwayEditor.SelectedRoadSection.Path.Point( RoadwayEditorData.selectedHandleIndex );
                bool isInsideBounds = junction.Bounds.Contains( pos );

                if ( ( isDragging && isInsideBounds ) || ( isDragging && isSelectedJuncForSnap && isDragHandle ) ) {
                    Handles.color = RoadwayEditorData.snapColours.defaultColour;
                    // snap points
                    for ( int s = 0; s < junction.NumSnaps; s++ ) {
                        // if there isn't a road attached to this snap point, draw a marker
                        if ( junction.Snap( s ).SnappedRoadSection == null )
                            Handles.DrawSolidDisc( junction.Snap( s ).snapPoint, junction.up, RoadwayGUISettings.snapDiameter );
                    }
                }

            }

            // links
            if ( RoadwayGUISettings.displayLaneFlowMarks ) {
                for ( int i = 0; i < junction.numLinks; i++ ) {
                    JunctionLaneLink link = junction.Link( i );

                    Handles.color = link.color;
                    Handles.DrawLine( link.OriginPoint, link.DestinationPoint, 3 );

                    foreach ( ArrowVector arrow in link.arrows ) {
                        Handles.DrawLine( arrow.head, arrow.armLeft, 2 );
                        Handles.DrawLine( arrow.head, arrow.armRight, 2 );
                    }
                }
            }
        }

        void DrawHandle(int juncIndex, Junction junction, Vector3 mousePos) {
            // change this to local space soon 
            Vector3 handlePosition = junction.Position;
            float handleSize = RoadwayGUISettings.anchorDiameter;
            PathHandle.HandleColours handleColours = RoadwayEditorData.anchorHandleColours;
            if ( juncIndex == RoadwayEditorData.selectedJunctionIndex )
                handleColours.defaultColour = RoadwayGUISettings.anchorSelected;
            Handles.CapFunction cap = Handles.SphereHandleCap;

            PathHandle.HandleInputType handleInputType;
            Vector3 newHandlePosition = PathHandle.DrawHandle( handlePosition, RoadwayGUISettings.anchorHandlesAreInteractive, handleSize,
                                                                cap, handleColours, out handleInputType,
                                                                -1, -1, juncIndex, -1 );


            switch ( handleInputType ) {
                case PathHandle.HandleInputType.LMBDrag:
                    RoadwayEditorData.selectedJunctionIndex = juncIndex;
                    roadwayEditor.UpdateToolbar( junction );
                    roadwayEditor.Repaint();
                    break;
                case PathHandle.HandleInputType.LMBRelease:
                    RoadwayEditorData.selectedJunctionIndex = juncIndex;
                    roadwayEditor.UpdateToolbar( junction );
                    roadwayEditor.Repaint();
                    break;
                case PathHandle.HandleInputType.LMBClick:
                    RoadwayEditorData.selectedJunctionIndex = juncIndex;
                    roadwayEditor.UpdateToolbar( junction );
                    roadwayEditor.Repaint();
                    break;
                case PathHandle.HandleInputType.LMBPress:
                    if ( juncIndex != RoadwayEditorData.selectedJunctionIndex ) {
                        RoadwayEditorData.selectedJunctionIndex = -1;
                        roadwayEditor.Repaint();
                    }
                    break;
            }

            if ( junction.Position != newHandlePosition ) {
                Undo.RecordObject( junction, "Move Point" );
                junction.Position = mousePos;
                EditorUtility.SetDirty( junction );
            }

        }

    }

}
