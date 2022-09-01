using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;
using Roadway.Junctions;

namespace RoadwayEditor {

    public class RoadwayEditorTabIntersection : RoadwayEditorTab {
        
        RoadwayEditor roadwayEditor;
        RoadwayManager roadway;
        RoadwayEditorData roadwayEditorData;

        public override void Draw(RoadwayEditor roadwayEditor) {

            this.roadwayEditor = roadwayEditor;
            this.roadway = roadwayEditor.roadway;
            this.roadwayEditorData = roadwayEditor.roadwayEditorData;

            if ( GUILayout.Button( "Create Blank Intersection" ) ) {
                roadwayEditor.QueueAction( RoadwayQueueActions.CreateIntersection );
            }

            if ( GUILayout.Button( "Create Intersection" ) ) {
                Intersection newInter = roadway.AddIntersection( 0, 0, 0,
                                                        1, 0, 0,
                                                        2, 0, 0,
                                                        3, 0, 0,
                                                        Vector3.negativeInfinity );
                roadwayEditorData.selectedJunctionIndex = roadway.IndexOf( newInter );
            }

            if ( GUILayout.Button( "Delete All Junctions" ) ) {
                roadway.DeleteAllJunctions();
                roadwayEditorData.selectedJunctionIndex = -1;
            }

            if ( roadwayEditor.SelectedJunction == null || !( roadwayEditor.SelectedJunction is Intersection ) )
                GUILayout.Label( "No Intersection selected" );

            Intersection inter = (Intersection)roadwayEditor.SelectedJunction;

            if ( inter == null )
                return;

            Color save = GUI.backgroundColor;
            GUI.backgroundColor = inter.IsLocked ? new Color( 1, 0, 0 ) : save;

            EditorGUILayout.BeginVertical( "Box" );
            if ( GUILayout.Button( "Lock" ) )
                inter.IsLocked = !inter.IsLocked;
            EditorGUI.BeginDisabledGroup( inter.IsLocked );

            GUILayout.Label( "Selected Intersection: " + inter.GetType().ToString() );

            EditorGUILayout.Separator();

            StringBuilder sb = new StringBuilder( "Roads: " );
            for ( int j = 0; j < inter.NumSnaps; j++ ) {
                if ( inter.Snap( j ).IsOccupied ) {
                    sb.Append( inter.Snap( j ).SnappedRoadSection.RoadSectionName );
                    sb.Append( " | " );
                }
            }
            EditorGUILayout.LabelField( sb.ToString() );

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            inter.MakeMesh = EditorGUILayout.Toggle( "Make Mesh", inter.MakeMesh );
            inter.MakeAI = EditorGUILayout.Toggle( "Make AI", inter.MakeAI );
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = save;


        }

    }

}