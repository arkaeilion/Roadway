using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;

namespace RoadwayEditor {

    public class RoadwayEditorTabSettings : RoadwayEditorTab {

        public override void Draw(RoadwayEditor roadwayEditor) {

            RoadwayManager roadway = roadwayEditor.roadway;
            RoadwaySettings roadwaySettings = roadway.RoadwaySettings;
            RoadwayGUISettings roadwayGUISettings = roadway.RoadwayGUISettings;
            RoadwayEditorData roadwayEditorData = roadwayEditor.roadwayEditorData;

            EditorGUILayout.Separator();

            if ( GUILayout.Button( "Bake" ) ) {
                roadway.Bake();
            }

            EditorGUILayout.Separator();

            roadway.RoadwaySettings = (RoadwaySettings)EditorGUILayout.ObjectField(
                                        "Roadway Settings", roadwaySettings, typeof( RoadwaySettings ), true );
            roadway.RoadwayGUISettings = (RoadwayGUISettings)EditorGUILayout.ObjectField(
                                        "Roadway GUI Settings", roadwayGUISettings, typeof( RoadwayGUISettings ), true );

            float newRoadLaneWidth = EditorGUILayout.Slider( "Road Lane Width", roadwaySettings.LaneWidth, 1, 10 );
            if ( roadwaySettings.LaneWidth != newRoadLaneWidth ) {
                roadwaySettings.LaneWidth = newRoadLaneWidth;
                roadway.NotifyModified();
            }

            roadwaySettings.Tiling = EditorGUILayout.Slider( "Road Texture Tiling", roadwaySettings.Tiling, 1, 10 );

            EditorGUILayout.Separator();
            RoadDrivingSide newRoadDrivingSide = (RoadDrivingSide)EditorGUILayout.Popup( "Road Driving Side",
                                                RoadwayEnums.IndexOf( roadwaySettings.RoadDrivingSide ),
                                                RoadwayEnums.RoadDrivingSideNames );
            if ( roadwaySettings.RoadDrivingSide != newRoadDrivingSide ) {
                roadwaySettings.RoadDrivingSide = newRoadDrivingSide;
                roadway.NotifyModified();
            }

            roadwaySettings.RoadUnitOptions = (RoadUnitOptions)EditorGUILayout.Popup( "Road Unit Options",
                                            RoadwayEnums.IndexOf( roadwaySettings.RoadUnitOptions ),
                                            RoadwayEnums.RoadUnitOptionsNames );

            EditorGUILayout.Separator();

            roadwayGUISettings.intersectionCreationRange = EditorGUILayout.Slider( "Intersection Creation Range", roadwayGUISettings.intersectionCreationRange, .5f, 20 );
            roadwayGUISettings.snapRange = EditorGUILayout.Slider( "Snap Range", roadwayGUISettings.snapRange, .5f, 20 );

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            roadwayGUISettings.displayAnchorPoints = EditorGUILayout.Toggle( "Display Anchor Points", roadwayGUISettings.displayAnchorPoints );
            roadwayGUISettings.displayControlPoints = EditorGUILayout.Toggle( "Display Control Points", roadwayGUISettings.displayControlPoints );
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            roadwayGUISettings.displayPath = EditorGUILayout.Toggle( "Display Path", roadwayGUISettings.displayPath );
            roadwayGUISettings.displayTools = EditorGUILayout.Toggle( "Display Tools", roadwayGUISettings.displayTools );
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            roadwayGUISettings.displayBoundingBox = EditorGUILayout.Toggle( "Display Bounding Box", roadwayGUISettings.displayBoundingBox );
            roadwayGUISettings.displayLaneFlowMarks = EditorGUILayout.Toggle( "Display Lane Flow Marks", roadwayGUISettings.displayLaneFlowMarks );
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            roadwayGUISettings.displayJunctionConnections = EditorGUILayout.Toggle( "Display Junction Connections", roadwayGUISettings.displayJunctionConnections );
            EditorGUILayout.EndHorizontal();
            

            EditorGUILayout.Separator();

            roadwayEditorData.showRoadwayRoadMaterialSettingsInfo = EditorGUILayout.Foldout( roadwayEditorData.showRoadwayRoadMaterialSettingsInfo, "Road Material Settings" );
            if ( roadwayEditorData.showRoadwayRoadMaterialSettingsInfo ) {
                EditorGUI.indentLevel++;

                EditorGUILayout.Separator();
                roadwaySettings.RoadCenterMat = (Material)EditorGUILayout.ObjectField( "Road Center Mat", roadwaySettings.RoadCenterMat, typeof( Material ), true );
                roadwaySettings.RoadLaneMat = (Material)EditorGUILayout.ObjectField( "Road Lane Mat", roadwaySettings.RoadLaneMat, typeof( Material ), true );
                roadwaySettings.RoadLaneTransitionMat = (Material)EditorGUILayout.ObjectField( "Road Lane Transition Mat", roadwaySettings.RoadLaneTransitionMat, typeof( Material ), true );
                roadwaySettings.IntersectionMat = (Material)EditorGUILayout.ObjectField( "Intersection Mat", roadwaySettings.IntersectionMat, typeof( Material ), true );

                EditorGUILayout.Separator();
                roadwaySettings.RoadBaseColor = EditorGUILayout.ColorField( "Road Base", roadwaySettings.RoadBaseColor );
                roadwaySettings.RoadOuterLineColor = EditorGUILayout.ColorField( "Road Outer Line", roadwaySettings.RoadOuterLineColor );
                roadwaySettings.RoadInnerLineColor = EditorGUILayout.ColorField( "Road Inner Line", roadwaySettings.RoadInnerLineColor );
                roadwaySettings.RoadDashLineColor = EditorGUILayout.ColorField( "Road Dash Line", roadwaySettings.RoadDashLineColor );

                EditorGUILayout.Separator();
                roadwaySettings.RoadLineWidth = EditorGUILayout.Slider( "Road Line Width", roadwaySettings.RoadLineWidth, .001f, .2f );
                roadwaySettings.RoadOuterLineDistance = EditorGUILayout.Slider( "Road Outer Line Distance", roadwaySettings.RoadOuterLineDistance, 0, 1 );
                roadwaySettings.RoadDashRatio = EditorGUILayout.Slider( "Road Dash Ratio", roadwaySettings.RoadDashRatio, 0, 1 );

                roadwaySettings.RoadCenterOuterLineDistance = EditorGUILayout.Slider( "Road Center Outer Line Distance", roadwaySettings.RoadCenterOuterLineDistance, 0f, 1f );

            }

            EditorGUILayout.Separator();
            roadwayEditorData.showRoadwayGUISettingsInfo = EditorGUILayout.Foldout( roadwayEditorData.showRoadwayGUISettingsInfo, "Path Settings" );

            if ( roadwayEditorData.showRoadwayGUISettingsInfo ) {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.Separator();
                roadwayGUISettings.maxRayDepth = EditorGUILayout.FloatField( "Max Depth For Ray", roadwayGUISettings.maxRayDepth );
                roadwayGUISettings.backupdepthForRay = EditorGUILayout.FloatField( "Depth For Ray Backup", roadwayGUISettings.backupdepthForRay );

                EditorGUILayout.Separator();
                roadwayGUISettings.anchorDiameter = EditorGUILayout.Slider( "Anchor Diameter", roadwayGUISettings.anchorDiameter, .5f, 10 );
                roadwayGUISettings.controlDiameter = EditorGUILayout.Slider( "Control Diameter", roadwayGUISettings.controlDiameter, .5f, 10 );
                roadwayGUISettings.toolDiameter = EditorGUILayout.Slider( "Tool Diameter", roadwayGUISettings.toolDiameter, .5f, 10 );

                EditorGUILayout.Separator();
                roadwayGUISettings.anchor = EditorGUILayout.ColorField( "Anchor", roadwayGUISettings.anchor );
                roadwayGUISettings.anchorHighlighted = EditorGUILayout.ColorField( "Anchor Highlighted", roadwayGUISettings.anchorHighlighted );
                roadwayGUISettings.anchorSelected = EditorGUILayout.ColorField( "Anchor Selected", roadwayGUISettings.anchorSelected );

                EditorGUILayout.Separator();
                roadwayGUISettings.control = EditorGUILayout.ColorField( "Control", roadwayGUISettings.control );
                roadwayGUISettings.controlHighlighted = EditorGUILayout.ColorField( "Control Highlighted", roadwayGUISettings.controlHighlighted );
                roadwayGUISettings.controlSelected = EditorGUILayout.ColorField( "Control Selected", roadwayGUISettings.controlSelected );
                roadwayGUISettings.controlLine = EditorGUILayout.ColorField( "Control Line", roadwayGUISettings.controlLine );

                EditorGUILayout.Separator();
                roadwayGUISettings.snap = EditorGUILayout.ColorField( "Snap", roadwayGUISettings.snap );
                roadwayGUISettings.snapHighlighted = EditorGUILayout.ColorField( "Snap Highlighted", roadwayGUISettings.snapHighlighted );
                roadwayGUISettings.snapSelected = EditorGUILayout.ColorField( "Snap Selected", roadwayGUISettings.snapSelected );

                EditorGUILayout.Separator();
                roadwayGUISettings.handleDisabled = EditorGUILayout.ColorField( "Handle Disabled", roadwayGUISettings.handleDisabled );

                roadwayGUISettings.path = EditorGUILayout.ColorField( "Path", roadwayGUISettings.path );
                roadwayGUISettings.pathHighlighted = EditorGUILayout.ColorField( "Path Highlighted", roadwayGUISettings.pathHighlighted );

                EditorGUILayout.Separator();

                roadwayGUISettings.spacing = EditorGUILayout.Slider( "Mesh Resolution (Spacing)", roadwayGUISettings.spacing, .5f, 10 );
                // RoadwayGUISettings.resolution = EditorGUILayout.Slider( "Resolution: ", RoadwayGUISettings.resolution, .5f, 10 );

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
            roadwayEditorData.showRoadwayRoadPresetInfo = EditorGUILayout.Foldout( roadwayEditorData.showRoadwayRoadPresetInfo, "Road Presets" );

            if ( roadwayEditorData.showRoadwayRoadPresetInfo ) {
                EditorGUI.indentLevel++;

                for ( int i = 0; i < roadway.RoadPresets.Count; i++ ) {
                    roadway.RoadPresets[ i ] = (RoadPreset)EditorGUILayout.ObjectField( roadway.RoadPresets[ i ], typeof( RoadPreset ), true );
                }


                GUILayout.BeginHorizontal();

                if ( GUILayout.Button( "+" ) ) {
                    roadway.RoadPresets.Add( null );
                }
                if ( GUILayout.Button( "-" ) ) {
                    roadway.RoadPresets.RemoveAt( roadway.RoadPresets.Count - 1 );
                }

                GUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

        }

    }

}