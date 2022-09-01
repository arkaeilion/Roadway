using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Roadway;
using Roadway.Spline;

namespace RoadwayEditor {

    [System.Serializable]
    public class RoadwayEditorData {

        public bool showRoadInfo = true;
        public bool showRoadSectionInfo = true;
        public bool showRoadwayGUISettingsInfo = false;
        public bool showRoadwayRoadMaterialSettingsInfo = false;
        public bool showRoadwayRoadPresetInfo = false;

        public bool editorTabsLocked = false;

        public int selectedRoadIndex = -1;
        public int selectedRoadSectionIndex = -1;
        public int selectedHandleIndex = -1;
        public int selectedSegmentIndex = -1;
        public int selectedJunctionIndex = -1;

        public int toolbarIndex = 0;

        public PathHandle.HandleColours anchorHandleColours;
        public PathHandle.HandleColours controlHandleColours;
        public PathHandle.HandleColours snapColours;
        public Dictionary<RoadwayGUISettings.HandleType, Handles.CapFunction> capFunctions;

        public int otherRoadSelectedSegmentIndex = -1;
        public int otherRoadSelectedIndex = -1;
        public Vector3 otherRoadSplitPoint;

        public int mouseOverHandleIndex = -1;
        public int draggingHandleIndex = -1;

        public SnappableObject doNotMoveThis = null;
        public SnappableObject doNotSnapOnThis = null;
        public int doNotSnapOnThisNullCount = 0;
        public int doNotSnapOnThisNullThreshold = 10;

        public int selectedJunctionForSnapIndex = -1; // this is a Junction index
        public int selectedRoadForSnapIndex = -1; // this is a Road index
        public int selectedRoadSectionForSnapIndex = -1; // this is a RoadSection index
        public int selectedSnapIndex = -1; // this is the snap to use from whatever is selected

        public int releasedRoadIndex = -1;
        public int releasedRoadSectionIndex = -1;
        public int releasedHandleIndex = -1;

        public bool selectedSegmentIndexLastFrame = false;
        public Vector3 splitPoint;

        public bool shiftLastFrame = false;
        public bool backspaceLastFrame = false;

        public void Reset() {
            selectedRoadIndex = -1;
            selectedRoadSectionIndex = -1;
            selectedHandleIndex = -1;
            selectedSegmentIndex = -1;
            selectedJunctionIndex = -1;
            mouseOverHandleIndex = -1;
            draggingHandleIndex = -1;
            otherRoadSelectedSegmentIndex = -1;
            otherRoadSelectedIndex = -1;
            selectedSnapIndex = -1;

            ResetDoNotSnapOnThis();
        }

        public void UpdateHandleColours(RoadwayGUISettings roadwayGUISettings) {
            if ( roadwayGUISettings == null ) return;

            anchorHandleColours = new PathHandle.HandleColours(
                                            roadwayGUISettings.anchor,
                                            roadwayGUISettings.anchorHighlighted,
                                            roadwayGUISettings.anchorSelected,
                                            roadwayGUISettings.handleDisabled );
            controlHandleColours = new PathHandle.HandleColours(
                                            roadwayGUISettings.control,
                                            roadwayGUISettings.controlHighlighted,
                                            roadwayGUISettings.controlSelected,
                                            roadwayGUISettings.handleDisabled );
            snapColours = new PathHandle.HandleColours(
                                            roadwayGUISettings.snap,
                                            roadwayGUISettings.snapHighlighted,
                                            roadwayGUISettings.snapSelected,
                                            roadwayGUISettings.handleDisabled );
        }

        public void StoreDoNotSnapOnThis(SnappableObject snappableObject) {
            if ( snappableObject == null ) {
                doNotSnapOnThisNullCount++;
                return;
            }

            if ( doNotSnapOnThisNullCount > doNotSnapOnThisNullThreshold ) {
                doNotSnapOnThisNullCount = 0;
                doNotSnapOnThis = null;
            }

            if ( snappableObject != null ) {
                doNotSnapOnThis = snappableObject;
                doNotSnapOnThisNullCount = 0;
            }
        }

        public void ResetDoNotSnapOnThis() {
            doNotSnapOnThis = null;
            doNotSnapOnThisNullCount = 0;
        }
    }

}