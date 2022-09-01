using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway {

    [CreateAssetMenu( fileName = "Roadway GUI Settings", menuName = "Roadway/Roadway GUI Settings" )]
    public class RoadwayGUISettings : ScriptableObject {
        public enum HandleType { Sphere, Circle, Square };

        public float anchorDiameter = 2.5f;
        public float controlDiameter = 1.5f;
        public float toolDiameter = 2f;

        public float snapDiameter = 1f;
        public float snapRange = 2f;
        public float intersectionCreationRange = 4f;

        public float segmentSelectDistanceThreshold = 0.8f;
        // if a segment is found to be less then this Threshold the search is ended
        public float segmentSelectDistanceThresholdCut = 0.1f;

        public float handleScale = 1;

        // Should the path still be drawn when behind objects in the scene?
        // public bool visibleBehindObjects = true;
        // Should the path be drawn even when the path object is not selected?
        // public bool visibleWhenNotSelected = true;
        public HandleType anchorShape = HandleType.Sphere;
        public HandleType controlShape = HandleType.Sphere;
        public HandleType toolShape = HandleType.Sphere;

        public Color handleDisabled = new Color( 1, 1, 1 );

        // anchor colors
        public Color anchor = new Color( .95f, .25f, .25f );
        public Color anchorHighlighted = new Color( 1, .57f, .4f );
        public Color anchorSelected = Color.white;

        // Control Colours
        public Color control = new Color( .35f, .6f, 1 );
        public Color controlHighlighted = new Color( .8f, .67f, .97f );
        public Color controlSelected = Color.white;
        public Color controlLine = new Color( 0, 0, 0 );

        // snaps
        public Color snap = new Color( .75f, 0.9f, 0, .3f );
        public Color snapHighlighted = new Color( .85f, 1f, 0.06f, .3f );
        public Color snapSelected = Color.white;

        // Bezier Path Colours
        public Color path = new Color( 1, .6f, 0 );
        public Color pathHighlighted = Color.green;


        public bool displayAnchorPoints = true;
        public bool displayControlPoints = true;
        public bool displayPath = true;
        public bool displayTools = true;
        public bool displayJunctionConnections = false;

        public bool displayBoundingBox = false;
        public bool displayLaneFlowMarks = false;

        public bool anchorHandlesAreInteractive = true;
        public bool controlHandlesAreInteractive = true;
        public bool toolHandlesAreInteractive = true;

        
        public float maxRayDepth = 500;
        public float backupdepthForRay = 20;
        public float spacing = 1f;
        public float resolution = .5f;
    }

}
