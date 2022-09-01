using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;
using Roadway.Roads;

namespace RoadwayEditor {

    public class RoadwayEditorTabRoad : RoadwayEditorTab {

        RoadwayEditor roadwayEditor;
        RoadwayManager roadway;
        RoadwaySettings roadwaySettings;
        RoadwayEditorData roadwayEditorData;
        Road road;
        RoadSection roadSection;

        List<bool> segmentsOpen = new List<bool>();

        public override void Draw(RoadwayEditor roadwayEditor) {
            this.roadwayEditor = roadwayEditor;
            roadway = roadwayEditor.roadway;
            roadwaySettings = roadway.RoadwaySettings;
            roadwayEditorData = roadwayEditor.roadwayEditorData;
            road = roadwayEditor.SelectedRoad;
            roadSection = roadwayEditor.SelectedRoadSection;

            DrawButtons();
            EditorGUILayout.Separator();
            DrawRoad();
        }

        void DrawButtons() {

            GUILayout.BeginHorizontal();

            if ( GUILayout.Button( "Create Road" ) ) {
                roadwayEditor.QueueAction( RoadwayQueueActions.CreateRoad );
            }

            roadway.selectedRoadPresetIndex = EditorGUILayout.Popup( roadway.selectedRoadPresetIndex, roadway.RoadPresetNames() );

            GUILayout.EndHorizontal();

            if ( GUILayout.Button( "Create Road Section" ) ) {
                if ( road != null ) {
                    roadwayEditor.QueueAction( RoadwayQueueActions.CreateRoadSection );
                }
            }

            if ( GUILayout.Button( "Delete All Roads" ) ) {
                roadwayEditor.roadway.DeleteAllRoads();
                roadwayEditorData.selectedRoadIndex = -1;
            }
        }

        void DrawRoad() {

            if ( road != null ) {

                Color save = GUI.backgroundColor;
                GUI.backgroundColor = road.IsLocked ? new Color( 1, 0, 0 ) : save;

                roadwayEditorData.showRoadInfo = EditorGUILayout.Foldout( roadwayEditorData.showRoadInfo, "Selected Road" );

                if ( roadwayEditorData.showRoadInfo ) {

                    EditorGUILayout.BeginVertical( "Box" );

                    EditorGUILayout.BeginHorizontal();
                    if ( GUILayout.Button( "Lock" ) )
                        road.IsLocked = !road.IsLocked;

                    EditorGUI.BeginDisabledGroup( road.IsLocked );

                    if ( GUILayout.Button( "Delete" ) ) {
                        roadway.RemoveRoad( road, true );
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;

                    road.RoadName = EditorGUILayout.TextField( "Road Name", road.RoadName );
                    road.RoadSpeed = EditorGUILayout.IntField( "Road Speed", road.RoadSpeed );

                    road.TravelDirection = (RoadTravelDirections)EditorGUILayout.Popup( "Travel Direction",
                                                                RoadwayEnums.IndexOf( road.TravelDirection ),
                                                                RoadwayEnums.RoadTravelDirectionsNames );

                    EditorGUILayout.BeginHorizontal();
                    road.MakeMesh = EditorGUILayout.Toggle( "Make Mesh", road.MakeMesh );
                    road.MakeAI = EditorGUILayout.Toggle( "Make AI", road.MakeAI );
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = save;

                }

            } else {
                EditorGUILayout.BeginVertical( "Box" );
                GUILayout.Label( "No road selected" );
                EditorGUILayout.EndVertical();
            }




            EditorGUILayout.Separator();
            DrawRoadSection();
        }

        void DrawRoadSection() {


            if ( roadSection != null ) {

                Color save = GUI.backgroundColor;
                GUI.backgroundColor = roadSection.IsLocked ? new Color( 1, 0, 0 ) : save;

                roadwayEditorData.showRoadSectionInfo = EditorGUILayout.Foldout( roadwayEditorData.showRoadSectionInfo, "Selected Road Section" );

                if ( roadwayEditorData.showRoadSectionInfo ) {

                    EditorGUILayout.BeginVertical( "Box" );

                    EditorGUILayout.BeginHorizontal();
                    if ( GUILayout.Button( "Lock" ) )
                        roadSection.IsLocked = !roadSection.IsLocked;

                    EditorGUI.BeginDisabledGroup( roadSection.IsLocked );

                    if ( GUILayout.Button( "Delete" ) ) {
                        road.RemoveRoadSection( roadSection );
                        return;
                    }

                    EditorGUILayout.EndHorizontal();


                    roadSection.RoadSectionNameOperation = (RoadSectionNameOperation)EditorGUILayout.Popup( "Name Operation",
                                                            RoadwayEnums.IndexOf( roadSection.RoadSectionNameOperation ),
                                                            RoadwayEnums.RoadSectionNameOperationNames );

                    if ( roadSection.RoadSectionNameOperation != RoadSectionNameOperation.None ) {
                        EditorGUI.indentLevel++;
                        roadSection.RoadSectionName = EditorGUILayout.TextField( "Section Name", roadSection.RoadSectionName );
                        EditorGUI.indentLevel--;
                    }

                    roadSection.RoadSectionSpeedOperation = (RoadSectionSpeedOperation)EditorGUILayout.Popup( "Speed Operation",
                                                    RoadwayEnums.IndexOf( roadSection.RoadSectionSpeedOperation ),
                                                    RoadwayEnums.RoadSectionSpeedOperationNames );

                    if ( roadSection.RoadSectionSpeedOperation != RoadSectionSpeedOperation.None ) {
                        EditorGUI.indentLevel++;
                        roadSection.RoadSectionSpeed =
                                    EditorGUILayout.IntField( "Road Section Speed", roadSection.RoadSectionSpeed );
                        EditorGUI.indentLevel--;
                    }

                    roadSection.IsSplit = EditorGUILayout.Toggle( "Split Sides", roadSection.IsSplit );

                    roadSection.RoadLines = (RoadLines)EditorGUILayout.Popup( "Road Lines",
                                                    RoadwayEnums.IndexOf( roadSection.RoadLines ),
                                                    RoadwayEnums.RoadLinesNames );

                    roadSection.Path.ControlMode = (ControlMode)EditorGUILayout.Popup( "Control Mode",
                                                    RoadwayEnums.IndexOf( roadSection.Path.ControlMode ),
                                                    RoadwayEnums.ControlModeNames );

                    EditorGUILayout.Separator();

                    roadSection.TransitionDistance = EditorGUILayout.Slider( "Transition Distance", roadSection.TransitionDistance, 0, 50 );

                    EditorGUILayout.Separator();

                    // roadSection.CenterWidthCurveMirror =
                    //                 EditorGUILayout.Toggle( "Mirror Center Width", roadSection.CenterWidthCurveMirror );

                    // if ( roadSection.CenterWidthCurveMirror ) {
                    //     if ( roadwaySettings.RoadDrivingSide == RoadDrivingSide.Right ) {
                    //         roadSection.CenterWidthCurveRight =
                    //                 EditorGUILayout.CurveField( "Center Width Curve", roadSection.CenterWidthCurveRight,
                    //                                                 Color.red, new Rect( 0, 0, 1, roadwaySettings.MaxRoadCenterWidth / 2 ) );
                    //     } else if ( roadwaySettings.RoadDrivingSide == RoadDrivingSide.Left ) {
                    //         roadSection.CenterWidthCurveLeft =
                    //                 EditorGUILayout.CurveField( "Center Width Curve", roadSection.CenterWidthCurveLeft,
                    //                                                 Color.red, new Rect( 0, 0, 1, roadwaySettings.MaxRoadCenterWidth / 2 ) );
                    //     }

                    // } else {
                    //     roadSection.CenterWidthCurveLeft =
                    //             EditorGUILayout.CurveField( "Center Width Curve Left", roadSection.CenterWidthCurveLeft,
                    //                                             Color.red, new Rect( 0, 0, 1, roadwaySettings.MaxRoadCenterWidth / 2 ) );
                    //     roadSection.CenterWidthCurveRight =
                    //             EditorGUILayout.CurveField( "Center Width Curve Right", roadSection.CenterWidthCurveRight,
                    //                                             Color.red, new Rect( 0, 0, 1, roadwaySettings.MaxRoadCenterWidth / 2 ) );

                    // }

                    // EditorGUILayout.Separator();

                    // roadSection.ExtraRoadWidthAtStart = EditorGUILayout.Slider( "Extra Road Width At Start", 
                    //                                     roadSection.ExtraRoadWidthAtStart, 0, roadwaySettings.LaneWidth*2 );
                    // roadSection.ExtraRoadWidthAtStartDst = EditorGUILayout.Slider( "Extra Road Width At Start Dst", 
                    //                                     roadSection.ExtraRoadWidthAtStartDst, 0, 30 );
                    // roadSection.ExtraRoadWidthAtEnd = EditorGUILayout.Slider( "Extra Road Width At End", 
                    //                                     roadSection.ExtraRoadWidthAtEnd, 0, roadwaySettings.LaneWidth*2 );
                    // roadSection.ExtraRoadWidthAtEndDst = EditorGUILayout.Slider( "Extra Road Width At End Dst", 
                    //                                     roadSection.ExtraRoadWidthAtEndDst, 0, 30 );

                    // EditorGUILayout.Separator();

                    EditorGUILayout.BeginHorizontal();
                    roadSection.MakeMesh = EditorGUILayout.Toggle( "Make Mesh", roadSection.MakeMesh );
                    roadSection.MakeAI = EditorGUILayout.Toggle( "Make AI", roadSection.MakeAI );
                    EditorGUILayout.EndHorizontal();

                    roadSection.ReverseDirection = EditorGUILayout.Toggle( "Reverse Direction", roadSection.ReverseDirection );

                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField( "Left Lanes " + roadSection.numLanesLeft +
                                                " | Inner " + roadSection.numLanesLeftInner +
                                                " | Outer " + roadSection.numLanesLeftOuter );

                    EditorGUILayout.LabelField( "Right Lanes " + roadSection.numLanesRight +
                                                " | Inner " + roadSection.numLanesRightInner +
                                                " | Outer " + roadSection.numLanesRightOuter );

                    EditorGUILayout.Separator();

                    // left add
                    EditorGUILayout.BeginHorizontal();
                    if ( GUILayout.Button( "Add Lane Left Inner" ) ) {
                        roadSection.AddLane( RoadSide.LeftInner );
                    }
                    if ( GUILayout.Button( "Add Lane Left Outer" ) ) {
                        roadSection.AddLane( RoadSide.LeftOuter );
                    }
                    EditorGUILayout.EndHorizontal();

                    // left remove
                    EditorGUILayout.BeginHorizontal();
                    if ( GUILayout.Button( "Remove Lane Left Inner" ) ) {
                        roadSection.RemoveLane( RoadSide.LeftInner );
                    }
                    if ( GUILayout.Button( "Remove Lane Left Outer" ) ) {
                        roadSection.RemoveLane( RoadSide.LeftOuter );
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Separator();

                    // right add
                    EditorGUILayout.BeginHorizontal();
                    if ( GUILayout.Button( "Add Lane Right Inner" ) ) {
                        roadSection.AddLane( RoadSide.RightInner );
                    }
                    if ( GUILayout.Button( "Add Lane Right Outer" ) ) {
                        roadSection.AddLane( RoadSide.RightOuter );
                    }
                    EditorGUILayout.EndHorizontal();

                    // right remove
                    EditorGUILayout.BeginHorizontal();
                    if ( GUILayout.Button( "Remove Lane Right Inner" ) ) {
                        roadSection.RemoveLane( RoadSide.RightInner );
                    }
                    if ( GUILayout.Button( "Remove Lane Right Outer" ) ) {
                        roadSection.RemoveLane( RoadSide.RightOuter );
                    }
                    EditorGUILayout.EndHorizontal();


                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndVertical();
                    GUI.backgroundColor = save;

                }

            } else {
                EditorGUILayout.BeginVertical( "Box" );
                GUILayout.Label( "No road section selected" );
                EditorGUILayout.EndVertical();
            }

        }

    }

}