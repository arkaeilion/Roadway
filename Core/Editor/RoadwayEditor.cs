using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Roadway;
using Roadway.Spline;
using Roadway.Roads;
using Roadway.Junctions;

namespace RoadwayEditor {

    // editor script is destroyed when object is deselected
    // and a new instance is create when the object (type) is reselected
    [CustomEditor( typeof( RoadwayManager ) )]
    public class RoadwayEditor : UnityEditor.Editor {

        public RoadwayManager roadway;
        public RoadwayEditorData roadwayEditorData;

        public RoadwayEditorRoadFuncs roadFuncs;
        public RoadwayEditorJunctionFuncs juncFuncs;

        RoadwayQueueActions queuedAction = RoadwayQueueActions.None;

        static readonly string[] toolbarStrings = { "Road", "Intersection", "Roundabout", "Settings", "Debug" };
        RoadwayEditorTab[] toolbarTabs;

        public void UpdateToolbar(object obj) {
            if ( roadwayEditorData.editorTabsLocked )
                return;

            int i = roadwayEditorData.toolbarIndex;

            if ( obj is Road || obj is RoadSection )
                i = 0;
            else if ( obj is Intersection )
                i = 1;
            else if ( obj is Roundabout )
                i = 2;

            roadwayEditorData.toolbarIndex = i;
        }

        #region setup

        // called when gameObject is selected
        void OnEnable() {
            PathHandle.ClearAndFix();
            roadway = (RoadwayManager)target;
            roadwayEditorData = new RoadwayEditorData();
            Tools.hidden = true;
            roadwayEditorData.UpdateHandleColours( RoadwayGUISettings );

            roadFuncs = new RoadwayEditorRoadFuncs( this );
            juncFuncs = new RoadwayEditorJunctionFuncs( this );

            toolbarTabs = new RoadwayEditorTab[] { ScriptableObject.CreateInstance<RoadwayEditorTabRoad>(),
                                                    ScriptableObject.CreateInstance<RoadwayEditorTabIntersection>(),
                                                    ScriptableObject.CreateInstance<RoadwayEditorTabRoundabout>(),
                                                    ScriptableObject.CreateInstance<RoadwayEditorTabSettings>(),
                                                    ScriptableObject.CreateInstance<RoadwayEditorTabDebug>() };
        }

        // called when gameObject is deselected
        void OnDisable() {
            Tools.hidden = false;
        }

        void OtherReset() {
            roadway.hasReset = false;
            roadwayEditorData.Reset();
            PathHandle.ClearAndFix();
            EditorUtility.SetDirty( target );
        }

        public void QueueAction(RoadwayQueueActions action) {
            queuedAction = action;
        }

        public bool ProcessQueueAction(Vector3 mousePos) {
            if ( queuedAction == RoadwayQueueActions.None )
                return true;

            bool isMouseOverHandle = roadwayEditorData.mouseOverHandleIndex != -1;
            bool isMouseDown = Event.current.type == EventType.MouseDown;
            bool isLeftButton = Event.current.button == 0;

            if ( queuedAction == RoadwayQueueActions.CreateIntersection ) {

                if ( !isMouseOverHandle && isMouseDown && isLeftButton ) {
                    Undo.RecordObject( roadway, "Add Intersection" );
                    Intersection inter = roadway.AddIntersection( mousePos );
                    roadwayEditorData.selectedJunctionIndex = roadway.IndexOf( inter );
                    queuedAction = RoadwayQueueActions.None;
                    return true;
                }

            } else if ( queuedAction == RoadwayQueueActions.CreateRoad ) {

                if ( !isMouseOverHandle && isMouseDown && isLeftButton ) {
                    Undo.RecordObject( roadway, "Add Road" );
                    Road road = roadway.AddRoad( mousePos );
                    roadwayEditorData.selectedRoadIndex = roadway.IndexOf( road );
                    roadwayEditorData.selectedRoadSectionIndex = 0;
                    roadwayEditorData.selectedHandleIndex = 0;
                    queuedAction = RoadwayQueueActions.None;

                    // small hack, if shift is being held normal code for adding to a path will create a roadsection point
                    // if shift is not held no point is created to show something has happened
                    if ( !Event.current.shift ) {
                        SelectedRoadSection.Path.AddSegment( mousePos );
                    }

                    return true;
                }

            } else if ( queuedAction == RoadwayQueueActions.CreateRoadSection ) {

                if ( !isMouseOverHandle && isMouseDown && isLeftButton ) {
                    Undo.RecordObject( roadway, "Add RoadSection" );
                    RoadSection roadSection = SelectedRoad.AddRoadSection( mousePos );
                    roadwayEditorData.selectedRoadSectionIndex = SelectedRoad.IndexOf( roadSection );
                    roadwayEditorData.selectedHandleIndex = 0;

                    // small hack, if shift is being held normal code for adding to a path will create a roadsection point
                    // if shift is not held no point is created to show something has happened
                    if ( !Event.current.shift ) {
                        roadSection.Path.AddSegment( mousePos );
                    }

                    queuedAction = RoadwayQueueActions.None;
                    return true;
                }

            }

            return false;
        }

        # endregion

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            using ( var check = new EditorGUI.ChangeCheckScope() ) {

                EditorGUILayout.BeginHorizontal();

                roadwayEditorData.toolbarIndex = GUILayout.Toolbar( roadwayEditorData.toolbarIndex, toolbarStrings );

                Color save = GUI.backgroundColor;
                GUI.backgroundColor = roadwayEditorData.editorTabsLocked ? new Color( 1, 0, 0 ) : save;
                if ( GUILayout.Button( roadwayEditorData.editorTabsLocked ? "-" : "o", GUILayout.Width( 20 ) ) )
                    roadwayEditorData.editorTabsLocked = !roadwayEditorData.editorTabsLocked;
                GUI.backgroundColor = save;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();
                toolbarTabs[ roadwayEditorData.toolbarIndex ].Draw( this );

                if ( check.changed ) {
                    roadwayEditorData.UpdateHandleColours( RoadwayGUISettings );
                    SceneView.RepaintAll();
                }
            }

        }

        void OnSceneGUI() {
            PathHandle.ClearAndFix();

            if ( roadway.hasReset )
                OtherReset();

            using ( var check = new EditorGUI.ChangeCheckScope() ) {
                if ( !DataIsNull ) {
                    Vector3 mousePos = MouseZ.MousePosition( RoadwayGUISettings.maxRayDepth, RoadwayGUISettings.backupdepthForRay );

                    if ( Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout )
                        ProcessInput( mousePos );
                    roadFuncs.Draw( mousePos );
                    juncFuncs.Draw( mousePos );
                }

                if ( check.changed )
                    EditorApplication.QueuePlayerLoopUpdate();
            }

            if ( Event.current.type == EventType.Layout )
                // Don't allow clicking over empty space or on other object to deselect the object
                HandleUtility.AddDefaultControl( 0 );
        }

        #region Input

        void ProcessInput(Vector3 mousePos) {
            if ( Event.current.type == EventType.MouseUp )
                roadwayEditorData.doNotMoveThis = null;

            InputMouseOverHandleCheck( mousePos );

            // action queue need to return true before any other action can be taken            
            if ( ProcessQueueAction( mousePos ) ) {
                InputSplitAddRoadSegment( mousePos );
                InputDeleteRoadSegment( mousePos );
                InputDeselectSegment( mousePos );
                InputUpdateSelectedRoadSegmentWhenDragging( mousePos );
                InputCreateIntersection( mousePos );
            }
        }

        void InputMouseOverHandleCheck(Vector3 mousePos) {
            // Find which handle mouse is over. Start by looking at previous handle index first, as most likely to still be closest to mouse
            int previousMouseOverHandleIndex = ( roadwayEditorData.mouseOverHandleIndex == -1 ) ? 0 : roadwayEditorData.mouseOverHandleIndex;
            roadwayEditorData.mouseOverHandleIndex = -1;

            for ( int r = 0; r < roadway.NumRoads; r++ ) {
                Road road = Road( r );
                if ( !road.Bounds.Contains( mousePos ) )
                    continue;

                for ( int s = 0; s < Road( r ).NumRoadSections; s++ ) {
                    RoadSection roadSection = RoadSection( r, s );
                    if ( !roadSection.Bounds.Contains( mousePos ) )
                        continue;

                    for ( int i = 0; i < roadSection.PathNumPoints; i += 3 ) {

                        int handleIndex = ( previousMouseOverHandleIndex + i ) % roadSection.PathNumPoints;
                        float handleRadius = RoadwayGUISettings.anchorDiameter / 2f;
                        Vector3 pos = roadSection.Path.Point( handleIndex );
                        float dst = HandleUtility.DistanceToCircle( pos, handleRadius );
                        if ( dst == 0 ) {
                            roadwayEditorData.mouseOverHandleIndex = handleIndex;
                            return;
                        }

                    }
                }
            }
        }

        void InputSplitAddRoadSegment(Vector3 mousePos) {
            RoadSection roadSection = SelectedRoadSection;

            if ( roadSection == null || roadwayEditorData.mouseOverHandleIndex != -1 )
                return;

            bool isMouseDown = Event.current.type == EventType.MouseDown;
            bool isLeftButton = Event.current.button == 0;
            bool isShift = Event.current.shift;

            // Shift-left click (when mouse not over a handle) to split or add segment
            if ( isMouseDown && isLeftButton && isShift ) {

                int segmentIndex = roadwayEditorData.selectedSegmentIndex;
                bool isSegmentInArrayBounds = segmentIndex >= 0 && segmentIndex < roadSection.Path.NumSegments;

                // Insert point along selected segment in the selected road
                if ( isSegmentInArrayBounds && roadwayEditorData.selectedSegmentIndexLastFrame ) {
                    Undo.RecordObject( roadSection, "Split Path Segment" );
                    roadSection.Path.SplitSegment( roadwayEditorData.splitPoint, segmentIndex );
                }
                // add new point on to the end of the path of the selected road
                else if ( roadwayEditorData.selectedHandleIndex >= 0 ) {
                    Undo.RecordObject( roadSection, "Add Path Segment" );
                    roadSection.Path.AddSegment( mousePos, roadwayEditorData.selectedHandleIndex == 0 );
                }

            }
        }

        void InputDeleteRoadSegment(Vector3 mousePos) {
            bool isMouseOverHandle = roadwayEditorData.mouseOverHandleIndex != -1;
            bool isDragging = roadwayEditorData.draggingHandleIndex != -1;
            bool isMouseDown = Event.current.type == EventType.MouseDown;
            bool isLeftButton = Event.current.button == 0;
            bool isBackspace = Event.current.keyCode == KeyCode.Backspace;
            bool isKeyDown = Event.current.type == EventType.KeyDown;
            bool isCtrl = Event.current.control || Event.current.command;
            // Control click or backspace/delete to remove point
            if ( ( isMouseOverHandle && isDragging && isBackspace && isKeyDown && !roadwayEditorData.backspaceLastFrame ) || ( isCtrl && isMouseDown && isLeftButton ) ) {
                roadwayEditorData.backspaceLastFrame = true;

                if ( SelectedRoadSection != null ) {
                    Undo.RecordObject( roadway, "Delete segment" );
                    SelectedRoadSection.Path.DeleteSegment( roadwayEditorData.mouseOverHandleIndex );
                    roadwayEditorData.selectedHandleIndex = -1;
                    roadwayEditorData.mouseOverHandleIndex = -1;
                    roadwayEditorData.draggingHandleIndex = -1;
                    Repaint();
                }
            } else if ( roadwayEditorData.backspaceLastFrame && Event.current.keyCode == KeyCode.Backspace && Event.current.type == EventType.KeyUp ) {
                roadwayEditorData.backspaceLastFrame = false;
            }

        }

        void InputCreateIntersection(Vector3 mousePos) {

            if ( roadwayEditorData.releasedRoadIndex == -1 || roadwayEditorData.releasedRoadSectionIndex == -1 || roadwayEditorData.releasedHandleIndex == -1 ) {
                return;
            }

            if ( roadwayEditorData.releasedRoadIndex >= roadway.NumRoads ||
                    roadwayEditorData.releasedRoadSectionIndex >= Road( roadwayEditorData.releasedRoadIndex ).NumRoadSections ||
                    roadwayEditorData.releasedHandleIndex >= RoadSection( roadwayEditorData.releasedRoadIndex, roadwayEditorData.releasedRoadSectionIndex ).PathNumPoints ) {
                roadwayEditorData.releasedRoadIndex = -1;
                roadwayEditorData.releasedRoadSectionIndex = -1;
                roadwayEditorData.releasedHandleIndex = -1;
                return;
            }

            // should we make an intersection?
            bool isReleasedRoad = roadwayEditorData.releasedRoadIndex != -1;
            bool isReleasedRoadSection = roadwayEditorData.releasedRoadSectionIndex != -1;
            bool isReleasedHandle = roadwayEditorData.releasedHandleIndex != -1;
            bool isHandleAnchor = roadwayEditorData.releasedHandleIndex % 3 == 0;

            if ( isReleasedRoad && isReleasedRoadSection && isReleasedHandle && isHandleAnchor ) {
                // Debug.Log( "checking to make intersection" );
                Vector3 pos = RoadSection( roadwayEditorData.releasedRoadIndex, roadwayEditorData.releasedRoadSectionIndex ).
                                                    Path.Point( roadwayEditorData.releasedHandleIndex );

                // is there any other road this point crosses?
                // does it loop back on itself?

                // different segment
                bool createdIntersection = false;

                for ( int r = 0; r < roadway.NumRoads; r++ ) {
                    Road road = Road( r );
                    if ( !road.Bounds.Contains( mousePos ) )
                        continue;

                    for ( int s = 0; s < road.NumRoadSections; s++ ) {
                        RoadSection roadSection = RoadSection( r, s );
                        if ( !roadSection.Bounds.Contains( mousePos ) )
                            continue;

                        for ( int i = 0; i < roadSection.Path.NumSegments; i++ ) {

                            // if different road or if same road, handle is not part of this segment
                            bool isDifferentRoad = r != roadwayEditorData.releasedRoadIndex;
                            bool isDifferentRoadSection = s != roadwayEditorData.releasedRoadSectionIndex;

                            if ( isDifferentRoad || ( !isDifferentRoad && isDifferentRoadSection ) ) {
                                // is pos near segment?
                                Vector3[] segmentPoints = roadSection.Path.GetPointsInSegment( i );
                                float dst = HandleUtility.DistancePointBezier( mousePos, segmentPoints[ 0 ], segmentPoints[ 3 ], segmentPoints[ 1 ], segmentPoints[ 2 ] );
                                if ( dst < RoadwayGUISettings.intersectionCreationRange ) {

                                    // don't make if within range of a snap point (plus a bit)
                                    bool snapBLOCKED = false;
                                    for ( int snap = 0; snap < roadSection.NumSnaps; snap++ ) {
                                        float snapDST = Vector3.Distance( mousePos, roadSection.SnapPoint( snap ) );
                                        if ( snapDST < ( RoadwayGUISettings.snapRange * 3 ) + .01f ) {
                                            snapBLOCKED = true;
                                            break;
                                        }
                                    }

                                    if ( snapBLOCKED ) {
                                        continue;
                                    }


                                    Debug.Log( "roadway.AddIntersection" );

                                    // split segment
                                    int newHandleIndex = roadSection.Path.SplitSegment( pos, i );

                                    int roadAIndex = roadwayEditorData.releasedRoadIndex;
                                    int roadARoadSectionIndex = roadwayEditorData.releasedRoadSectionIndex;
                                    int roadARoadSectionHandleIndex = roadwayEditorData.releasedHandleIndex;

                                    int roadBIndex = roadwayEditorData.releasedRoadIndex;
                                    int roadBRoadSectionIndex = Road( roadwayEditorData.releasedRoadIndex ).
                                                            SplitRoadSection( roadwayEditorData.releasedRoadSectionIndex, roadwayEditorData.releasedHandleIndex );
                                    int roadBRoadSectionHandleIndex = 0;

                                    int roadCIndex = r;
                                    int roadCRoadSectionIndex = s;
                                    int roadCRoadSectionHandleIndex = newHandleIndex;

                                    int roadDIndex = r;
                                    int roadDRoadSectionIndex = Road( r ).SplitRoadSection( s, newHandleIndex );
                                    int roadDRoadSectionHandleIndex = 0;

                                    Undo.RecordObject( roadway, "Add Intersection" );
                                    roadway.AddIntersection( roadAIndex, roadARoadSectionIndex, roadARoadSectionHandleIndex,
                                                            roadBIndex, roadBRoadSectionIndex, roadBRoadSectionHandleIndex,
                                                            roadCIndex, roadCRoadSectionIndex, roadCRoadSectionHandleIndex,
                                                            roadDIndex, roadDRoadSectionIndex, roadDRoadSectionHandleIndex,
                                                            pos );

                                    Debug.Log( "createdIntersection " + createdIntersection.ToString() );

                                    roadwayEditorData.releasedRoadIndex = -1;
                                    roadwayEditorData.releasedRoadSectionIndex = -1;
                                    roadwayEditorData.releasedHandleIndex = -1;
                                    return;
                                }
                            }
                        }
                    }
                }

                roadwayEditorData.releasedRoadIndex = -1;
                roadwayEditorData.releasedRoadSectionIndex = -1;
                roadwayEditorData.releasedHandleIndex = -1;
            }
        }

        void InputDeselectSegment(Vector3 mousePos) {
            bool isShift = Event.current.shift;
            // if there was a segment selected last frame and we are still holding shift
            // do a single check to see if it should still be selected
            if ( roadwayEditorData.selectedSegmentIndexLastFrame && isShift && SelectedRoad != null ) {
                Vector3[] segmentPoints = SelectedRoadSection.Path.GetPointsInSegment( roadwayEditorData.selectedSegmentIndex );
                float dst = HandleUtility.DistancePointBezier( mousePos, segmentPoints[ 0 ], segmentPoints[ 3 ], segmentPoints[ 1 ], segmentPoints[ 2 ] );
                if ( dst > RoadwayGUISettings.segmentSelectDistanceThreshold ) {
                    roadwayEditorData.selectedSegmentIndexLastFrame = false;
                }
            } else {
                roadwayEditorData.selectedSegmentIndexLastFrame = false;
            }
        }

        void InputUpdateSelectedRoadSegmentWhenDragging(Vector3 mousePos) {
            bool isShift = Event.current.shift;
            bool isMouseOverHandle = roadwayEditorData.mouseOverHandleIndex != -1;
            bool isMouseMove = Event.current.type == EventType.MouseMove;
            bool isMouseDrag = Event.current.type == EventType.MouseDrag;
            bool isDragging = roadwayEditorData.draggingHandleIndex != -1;
            // Holding shift and moving mouse (but mouse not over a handle/dragging a handle)
            if ( !isDragging && !isMouseOverHandle ) {
                bool shiftDown = Event.current.shift && !roadwayEditorData.shiftLastFrame;
                if ( shiftDown || ( ( isMouseMove || isMouseDrag ) && isShift ) ) {

                    float minDistanceToSegment = RoadwayGUISettings.segmentSelectDistanceThreshold;
                    int newSelectedSegmentIndex = -1;
                    int newSelectedRoadIndex = -1;
                    int newSelectedRoadSectionIndex = -1;

                    for ( int r = 0; r < roadway.NumRoads; r++ ) {
                        Road road = Road( r );
                        if ( !road.Bounds.Contains( mousePos ) )
                            continue;

                        for ( int s = 0; s < road.NumRoadSections; s++ ) {
                            RoadSection roadSection = RoadSection( r, s );
                            if ( !roadSection.Bounds.Contains( mousePos ) )
                                continue;

                            for ( int i = 0; i < roadSection.Path.NumSegments; i++ ) {

                                // int handleIndex = ( previousMouseOverHandleIndex + i ) % roadway.Roads[ r ].NumPoints;

                                Vector3[] segmentPoints = roadSection.Path.GetPointsInSegment( i );
                                float dst = HandleUtility.DistancePointBezier( mousePos, segmentPoints[ 0 ], segmentPoints[ 3 ], segmentPoints[ 1 ], segmentPoints[ 2 ] );
                                if ( dst < minDistanceToSegment ) {
                                    minDistanceToSegment = dst;
                                    newSelectedSegmentIndex = i;
                                    newSelectedRoadIndex = r;
                                    newSelectedRoadSectionIndex = s;

                                    if ( dst < RoadwayGUISettings.segmentSelectDistanceThresholdCut ) {
                                        break;
                                    }
                                }

                            }
                        }
                    }

                    if ( newSelectedSegmentIndex != -1 || newSelectedRoadIndex != -1 ) {
                        roadwayEditorData.selectedSegmentIndexLastFrame = true;
                    }

                    bool changeSelectedSegmentIndex = newSelectedSegmentIndex != roadwayEditorData.selectedSegmentIndex && newSelectedSegmentIndex != -1;
                    bool changeSelectedRoadIndex = newSelectedRoadIndex != roadwayEditorData.selectedRoadIndex && newSelectedRoadIndex != -1;
                    bool changeSelectedRoadSectionIndex = newSelectedRoadSectionIndex != roadwayEditorData.selectedRoadSectionIndex && newSelectedRoadSectionIndex != -1;
                    if ( changeSelectedSegmentIndex || changeSelectedRoadIndex || changeSelectedRoadSectionIndex ) {
                        roadwayEditorData.selectedSegmentIndex = newSelectedSegmentIndex;
                        roadwayEditorData.selectedRoadIndex = newSelectedRoadIndex;
                        roadwayEditorData.selectedRoadSectionIndex = newSelectedRoadSectionIndex;
                        roadwayEditorData.splitPoint = mousePos;
                        HandleUtility.Repaint();
                    }

                }
            }

            roadwayEditorData.shiftLastFrame = Event.current.shift;
        }

        #endregion

        #region Getters

        public Road Road(int roadIndex) {
            return roadway.Road( roadIndex );
        }

        public RoadSection RoadSection(int roadIndex, int roadSectionIndex) {
            Road road = Road( roadIndex );
            RoadSection roadSection = road != null ? road.RoadSection( roadSectionIndex ) : null;
            return roadSection;
        }

        public Junction Junction(int juncIndex) {
            return roadway.Junction( juncIndex );
        }

        public RoadwaySettings RoadwaySettings {
            get { return roadway.RoadwaySettings; }
        }

        public RoadwayGUISettings RoadwayGUISettings {
            get { return roadway.RoadwayGUISettings; }
        }

        public bool DataIsNull {
            get { return RoadwaySettings == null || RoadwayGUISettings == null; }
        }

        public RoadSection SelectedRoadSection {
            get {
                if ( SelectedRoad != null && roadwayEditorData.selectedRoadSectionIndex != -1 && roadwayEditorData.selectedRoadSectionIndex < SelectedRoad.NumRoadSections )
                    return SelectedRoad.RoadSection( roadwayEditorData.selectedRoadSectionIndex );
                return null;
            }
        }

        public Road SelectedRoad {
            get {
                if ( roadwayEditorData.selectedRoadIndex != -1 && roadwayEditorData.selectedRoadIndex < roadway.NumRoads )
                    return roadway.Road( roadwayEditorData.selectedRoadIndex );
                return null;
            }
        }

        public Junction SelectedJunction {
            get {
                if ( roadwayEditorData.selectedJunctionIndex != -1 )
                    return roadway.Junction( roadwayEditorData.selectedJunctionIndex );
                return null;
            }
        }

        public void SetSelected(Junction junc) {
            roadwayEditorData.selectedJunctionIndex = roadway.IndexOf( junc );
        }

        public void SetSelected(Road road) {
            roadwayEditorData.selectedRoadIndex = roadway.IndexOf( road );
        }

        public void SetSelected(RoadSection roadSection) {
            SetSelected( roadSection.Road );
            roadwayEditorData.selectedRoadSectionIndex = roadSection.Road.IndexOf( roadSection );
        }

        # endregion

        # region Menu

        [MenuItem( "Component/Roadway/Create Roadway" )]
        public static void RoadwayMenuCreateRoadway() {
            GameObject r = new GameObject( "Roadway" );
            r.AddComponent<RoadwayManager>();
        }

        #endregion

    }

}