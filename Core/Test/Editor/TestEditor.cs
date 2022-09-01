using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Roadway.Testing {

    [CustomEditor( typeof( Test ) )]
    public class TestEditor : UnityEditor.Editor {

        public Test test;

        void OnEnable() {
            test = (Test)target;
            Tools.hidden = true;
        }

        void OnDisable() {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if ( GUILayout.Button( "DO" ) ) {
                test.DO();
            }

        }

        void OnSceneGUI() {

        }

    }

}
