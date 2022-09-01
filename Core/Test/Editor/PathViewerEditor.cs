using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Roadway.Testing {

    [CustomEditor( typeof( PathViewer ) )]
    public class PathViewerEditor : UnityEditor.Editor {

        public PathViewer pv;

        void OnEnable() {
            pv = (PathViewer)target;
            // Tools.hidden = true;
        }

        void OnDisable() {
            // Tools.hidden = false;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if ( GUILayout.Button( "DO" ) ) {
                pv.DO();
                SceneView.RepaintAll();
            }

        }

        void OnSceneGUI() {
            if ( pv.path == null || !pv.path.IsComplete )
                return;

            Handles.color = Color.cyan;
            Handles.DrawLine( pv.transform.position, pv.target, 3 );
            Handles.DrawLine( pv.target, pv.path.path[ 0 ].Position, 3 );

            for ( int i = 0; i < pv.path.path.Count - 1; i++ )
                Handles.DrawLine( pv.path.path[ i ].Position, pv.path.path[ i + 1 ].Position, 3 );

            Handles.DrawLine( pv.path.path[ pv.path.path.Count - 1 ].Position, pv.path.end.Point, 3 );
            Handles.DrawLine( pv.path.end.Point, pv.end, 3 );

            Handles.color = Color.red;
            for ( int i = 0; i < pv.points.Count - 1; i++ )
                Handles.DrawLine( pv.points[ i ], pv.points[ i + 1 ], 3 );





        }

    }

}
