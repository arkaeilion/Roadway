using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Roadway;
using Roadway.Junctions;

namespace RoadwayEditor {

    public class RoadwayEditorTabRoundabout : RoadwayEditorTab {

        public override void Draw(RoadwayEditor roadwayEditor) {

            RoadwayManager roadway = roadwayEditor.roadway;
            Roundabout round = null;
            // Roundabout round = roadwayEditor.SelectedIntersection;

            if ( round != null ) {
                
                

            } else {
                GUILayout.Label( "No Roundabout selected" );
            }

        }

    }

}