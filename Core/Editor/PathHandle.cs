using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Roadway {

    public static class PathHandle {

        public const float extraInputRadius = .005f;

        static Vector2 handleDragMouseStart;
        static Vector2 handleDragMouseEnd;
        static Vector3 handleDragWorldStart;

        static int selectedHandleID;
        static bool mouseIsOverAHandle;

        public enum HandleInputType {
            None,
            LMBPress,
            LMBClick,
            LMBDrag,
            LMBRelease,
        };

        static float dstMouseToDragPointStart;

        static Dictionary<(int, int, int, int), int> ids;

        static PathHandle() {
            ids = new Dictionary<(int, int, int, int), int>();
            dstMouseToDragPointStart = float.MaxValue;
        }

        public static Vector3 DrawHandle(Vector3 position, bool isInteractive, float handleDiameter,
                                        Handles.CapFunction capFunc, HandleColours colours, out HandleInputType inputType,
                                        int roadIndex, int roadSectionIndex, int juncIndex, int handleIndex) {
            int id = GetID( roadIndex, roadSectionIndex, juncIndex, handleIndex );
            Vector3 screenPosition = Handles.matrix.MultiplyPoint( position );
            Matrix4x4 cachedMatrix = Handles.matrix;

            inputType = HandleInputType.None;

            EventType eventType = Event.current.GetTypeForControl( id );
            float handleRadius = handleDiameter / 2f;
            float dstToHandle = HandleUtility.DistanceToCircle( position, handleRadius + extraInputRadius );
            float dstToMouse = HandleUtility.DistanceToCircle( position, 0 );

            // Handle input events
            if ( isInteractive ) {
                // Repaint if mouse is entering/exiting handle (for highlight colour)
                if ( dstToHandle == 0 ) {
                    if ( !mouseIsOverAHandle ) {
                        HandleUtility.Repaint();
                        mouseIsOverAHandle = true;
                    }
                } else {
                    if ( mouseIsOverAHandle ) {
                        HandleUtility.Repaint();
                        mouseIsOverAHandle = false;
                    }
                }
                switch ( eventType ) {
                    case EventType.MouseDown:
                        if ( Event.current.button == 0 && Event.current.modifiers != EventModifiers.Alt ) {
                            if ( dstToHandle == 0 && dstToMouse < dstMouseToDragPointStart ) {
                                dstMouseToDragPointStart = dstToMouse;
                                GUIUtility.hotControl = id;
                                handleDragMouseEnd = handleDragMouseStart = Event.current.mousePosition;
                                handleDragWorldStart = position;
                                selectedHandleID = id;
                                inputType = HandleInputType.LMBPress;
                            }
                        }
                        break;

                    case EventType.MouseUp:
                        dstMouseToDragPointStart = float.MaxValue;
                        if ( GUIUtility.hotControl == id && Event.current.button == 0 ) {
                            GUIUtility.hotControl = 0;
                            selectedHandleID = -1;
                            Event.current.Use();

                            inputType = HandleInputType.LMBRelease;


                            if ( Event.current.mousePosition == handleDragMouseStart ) {
                                inputType = HandleInputType.LMBClick;
                            }
                        }
                        break;

                    case EventType.MouseDrag:
                        if ( GUIUtility.hotControl == id && Event.current.button == 0 ) {
                            handleDragMouseEnd += new Vector2( Event.current.delta.x, -Event.current.delta.y );
                            Vector3 position2 = Camera.current.WorldToScreenPoint( Handles.matrix.MultiplyPoint( handleDragWorldStart ) )
                                + (Vector3)( handleDragMouseEnd - handleDragMouseStart );
                            inputType = HandleInputType.LMBDrag;
                            // Handle can move freely in 3d space
                            position = Handles.matrix.inverse.MultiplyPoint( Camera.current.ScreenToWorldPoint( position2 ) );

                            GUI.changed = true;
                            Event.current.Use();
                        }
                        break;
                }
            }

            switch ( eventType ) {
                case EventType.Repaint:
                    Color originalColour = Handles.color;
                    Handles.color = ( isInteractive ) ? colours.defaultColour : colours.disabledColour;

                    if ( id == GUIUtility.hotControl ) {
                        Handles.color = colours.selectedColour;
                    } else if ( dstToHandle == 0 && selectedHandleID == -1 && isInteractive ) {
                        Handles.color = colours.highlightedColour;
                    }


                    Handles.matrix = Matrix4x4.identity;
                    Vector3 lookForward = Vector3.up;
                    Camera cam = Camera.current;
                    if ( cam != null ) {
                        if ( cam.orthographic ) {
                            lookForward = -cam.transform.forward;
                        } else {
                            lookForward = ( cam.transform.position - position );
                        }
                    }

                    capFunc( id, screenPosition, Quaternion.LookRotation( lookForward ), handleDiameter, EventType.Repaint );
                    Handles.matrix = cachedMatrix;

                    Handles.color = originalColour;
                    break;

                case EventType.Layout:
                    Handles.matrix = Matrix4x4.identity;
                    HandleUtility.AddControl( id, HandleUtility.DistanceToCircle( screenPosition, handleDiameter / 2f ) );
                    Handles.matrix = cachedMatrix;
                    break;
            }

            return position;
        }

        public struct HandleColours {
            public Color defaultColour;
            public Color highlightedColour;
            public Color selectedColour;
            public Color disabledColour;

            public HandleColours(Color defaultColour, Color highlightedColour, Color selectedColour, Color disabledColour) {
                this.defaultColour = defaultColour;
                this.highlightedColour = highlightedColour;
                this.selectedColour = selectedColour;
                this.disabledColour = disabledColour;
            }
        }

        static int GetID(int roadIndex, int roadSectionIndex, int juncIndex, int handleIndex) {
            if ( ids.ContainsKey( (roadIndex, roadSectionIndex, juncIndex, handleIndex) ) ) {
                // Debug.Log( "found " + ids[ (roadIndex, roadSectionIndex, juncIndex, handleIndex, toolIndex) ] );
                return ids[ (roadIndex, roadSectionIndex, juncIndex, handleIndex) ];
            }

            // create/add the id
            string hashString = string.Format( "pathhandle( roadIndex {0}, roadSectionIndex {1}, juncIndex {2}, handleIndex {3} )",
                                                            roadIndex, roadSectionIndex, juncIndex, handleIndex );
            int hash = hashString.GetHashCode();
            int id = GUIUtility.GetControlID( hash, FocusType.Passive );
            ids.Add( (roadIndex, roadSectionIndex, juncIndex, handleIndex), id );
            return id;
        }

        // this fixes a bug where the wrong id is returned
        // might be from having too many ids in memory
        //but this fixes it, going to putt this in the roadwayeditor 
        public static void ClearAndFix() {
            ids.Clear();
        }
    }
}