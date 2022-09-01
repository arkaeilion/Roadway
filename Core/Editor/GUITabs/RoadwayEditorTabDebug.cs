using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;

namespace RoadwayEditor {

    public class RoadwayEditorTabDebug : RoadwayEditorTab {

        public override void Draw(RoadwayEditor roadwayEditor) {
            RoadwayManager roadway = roadwayEditor.roadway;


            GUILayout.Label( "Junction ID: " + roadwayEditor.roadwayEditorData.selectedJunctionIndex );
            GUILayout.Label( "Junction: " + roadwayEditor.SelectedJunction );

            EditorGUILayout.Separator();

            GUILayout.Label( "Road ID: " + roadwayEditor.roadwayEditorData.selectedRoadIndex );
            GUILayout.Label( "Road: " + roadwayEditor.SelectedRoad );

            EditorGUILayout.Separator();

            GUILayout.Label( "RoadSection ID: " + roadwayEditor.roadwayEditorData.selectedRoadSectionIndex );
            GUILayout.Label( "RoadSection: " + roadwayEditor.SelectedRoadSection );

            EditorGUILayout.Separator();

            GUILayout.Label( "Handle ID: " + roadwayEditor.roadwayEditorData.selectedHandleIndex );
            GUILayout.Label( "Segment ID: " + roadwayEditor.roadwayEditorData.selectedSegmentIndex );


        }

    }

}