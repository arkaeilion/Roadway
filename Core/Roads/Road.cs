using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor.Events;

using Roadway.Spline;

namespace Roadway.Roads {

    [ExecuteInEditMode]
    public class Road : MonoBehaviour {

        # region Modify Event 

        [SerializeField, HideInInspector]
        public UnityEvent OnModified = new UnityEvent();

        // Called when the Road is modified
        /// <summary> if suppressOnModify is false the event is triggered to modify those subscribed</summary>
        public void NotifyModified(bool suppressOnModify = false) {
            if ( !suppressOnModify && OnModified != null ) {
                OnModified.Invoke();
            }
        }

        void OnRoadSectionModified() {
            UpdateBoundingBox();
        }

        # endregion

        # region Roadway Getters

        private RoadwayManager roadway;

        RoadwayManager Roadway {
            get {
                if ( roadway == null )
                    roadway = FindObjectOfType<RoadwayManager>();
                return roadway;
            }
        }

        RoadwaySettings RoadwaySettings {
            get { return Roadway.RoadwaySettings; }
        }

        RoadwayGUISettings RoadwayGUISettings {
            get { return Roadway.RoadwayGUISettings; }
        }

        # endregion

        # region Bounds

        [SerializeField, HideInInspector]
        private Bounds bounds;

        public Bounds Bounds {
            get { return bounds; }
        }

        private Bounds UpdateBoundingBox() {

            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;

            for ( int r = 0; r < NumRoadSections; r++ ) {
                Vector3 vMin = roadSections[ r ].Bounds.min;
                Vector3 vMax = roadSections[ r ].Bounds.max;

                min = new Vector3( Mathf.Min( min.x, vMin.x ), Mathf.Min( min.y, vMin.y ), Mathf.Min( min.z, vMin.z ) );
                max = new Vector3( Mathf.Max( max.x, vMax.x ), Mathf.Max( max.y, vMax.y ), Mathf.Max( max.z, vMax.z ) );
            }

            bounds = new Bounds( ( min + max ) / 2, max - min );
            return bounds;
        }

        # endregion

        [SerializeField, HideInInspector]
        List<RoadSection> roadSections = new List<RoadSection>();

        [SerializeField, HideInInspector]
        private int roadSpeed = 60;
        [SerializeField, HideInInspector]
        private string roadName = "";

        [SerializeField, HideInInspector]
        private RoadTravelDirections travelDirection = RoadTravelDirections.Bothways;

        [SerializeField, HideInInspector]
        private bool makeMesh = true;
        [SerializeField, HideInInspector]
        private bool makeAI = true;

        [SerializeField, HideInInspector]
        RoadPreset preset = null;

        [SerializeField, HideInInspector]
        bool locked = false;

        public RoadSection RoadSection(int i) {
            if ( i >= 0 && i < NumRoadSections )
                return roadSections[ i ];
            return null;
        }

        public int IndexOf(RoadSection roadSection) {
            return roadSections.IndexOf( roadSection );
        }

        public int NumRoadSections {
            get { return roadSections.Count; }
        }

        public string RoadName {
            get { return roadName; }
            set {
                if ( roadName != value ) {
                    roadName = value;
                    name = roadName;
                }
            }
        }

        public int RoadSpeed {
            get { return roadSpeed; }
            set {
                if ( roadSpeed != value ) {
                    roadSpeed = value;
                }
            }
        }

        public bool MakeMesh {
            get { return makeMesh; }
            set {
                if ( makeMesh != value ) {
                    makeMesh = value;
                    foreach ( RoadSection rs in roadSections )
                        rs.MakeMesh = value;
                }
            }
        }

        public bool MakeAI {
            get { return makeAI; }
            set {
                if ( makeAI != value ) {
                    makeAI = value;
                    foreach ( RoadSection rs in roadSections )
                        rs.MakeAI = value;
                }
            }
        }

        public RoadTravelDirections TravelDirection {
            get { return travelDirection; }
            set {
                if ( travelDirection != value ) {
                    travelDirection = value;
                    foreach ( RoadSection rs in roadSections )
                        rs.NotifyModified();
                }
            }
        }

        public bool IsLocked {
            get { return locked; }
            set {
                locked = value;
                foreach ( RoadSection rs in roadSections )
                    rs.IsLocked = value;
            }
        }

        public void Init(Vector3 pos, RoadPreset preset) {
            if ( preset != null ) {
                this.roadSpeed = preset.roadSpeed;
                this.roadName = preset.roadName;
                this.travelDirection = preset.travelDirection;
                this.makeMesh = preset.makeMesh;
                this.makeAI = preset.makeAI;
                this.preset = preset;
            } else {
                this.roadSpeed = 60;
                this.roadName = "";
                this.travelDirection = RoadTravelDirections.Bothways;
                this.makeMesh = true;
                this.makeAI = true;
                this.preset = null;
            }

            if ( this.roadName == "" )
                this.roadName = RoadNameGenerator.Name();

            name = this.roadName;

            roadSections.Clear();
            AddRoadSection();
        }

        public RoadSection AddRoadSection(RoadSection toCopy = null) {
            GameObject newRoadSectionGameObject = new GameObject( "Road Section" );
            RoadSection roadSection = newRoadSectionGameObject.AddComponent<RoadSection>();
            roadSection.transform.parent = transform;

            if ( toCopy != null )
                roadSection.Init( toCopy );
            else
                roadSection.Init( preset, RoadName, RoadSpeed );


            UnityEventTools.RemovePersistentListener( roadSection.OnModified, OnRoadSectionModified );
            UnityEventTools.AddPersistentListener( roadSection.OnModified, OnRoadSectionModified );
            roadSection.OnModified.SetPersistentListenerState( roadSection.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );

            roadSections.Add( roadSection );
            NotifyModified();
            return roadSection;
        }

        public RoadSection AddRoadSection(Vector3 pos) {
            GameObject newRoadSectionGameObject = new GameObject( "Road Section" );
            RoadSection roadSection = newRoadSectionGameObject.AddComponent<RoadSection>();
            roadSection.transform.parent = transform;

            roadSection.Init( preset, RoadName, RoadSpeed );

            UnityEventTools.RemovePersistentListener( roadSection.OnModified, OnRoadSectionModified );
            UnityEventTools.AddPersistentListener( roadSection.OnModified, OnRoadSectionModified );
            roadSection.OnModified.SetPersistentListenerState( roadSection.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );

            roadSections.Add( roadSection );
            NotifyModified();
            return roadSection;
        }

        public int SplitRoadSection(int indexRoadSectionToSplit, int indexHanleToSplitAt) {
            RoadSection curRoadSection = RoadSection( indexRoadSectionToSplit );
            RoadSection newRoadSection = AddRoadSection( curRoadSection );

            UnityEventTools.RemovePersistentListener( newRoadSection.OnModified, OnRoadSectionModified );
            UnityEventTools.AddPersistentListener( newRoadSection.OnModified, OnRoadSectionModified );
            newRoadSection.OnModified.SetPersistentListenerState( newRoadSection.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );

            // remove points from path
            curRoadSection.Path.DeleteAllPointsAfter( indexHanleToSplitAt );
            newRoadSection.Path.DeleteAllPointsBefore( indexHanleToSplitAt );

            NotifyModified();
            // return the index of the new road section
            return roadSections.IndexOf( newRoadSection );
        }

        public void AddRoadSections(List<RoadSection> newRoadSections) {
            roadSections.AddRange( newRoadSections );

            foreach ( RoadSection rs in newRoadSections ) {
                UnityEventTools.RemovePersistentListener( rs.OnModified, OnRoadSectionModified );
                UnityEventTools.AddPersistentListener( rs.OnModified, OnRoadSectionModified );
                rs.OnModified.SetPersistentListenerState( rs.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );
            }
            NotifyModified();
        }

        public void RemoveRoadSection(RoadSection roadSection) {
            roadSections.Remove( roadSection );
            UnityEventTools.RemovePersistentListener( roadSection.OnModified, OnRoadSectionModified );
            UtilityZ.SafeDestroy( roadSection.gameObject );

            if ( NumRoadSections == 0 )
                Roadway.RemoveRoad( this, true );
        }

        public void DeleteAllRoadSections() {
            roadSections.Clear();

            for ( int i = transform.childCount - 1; i >= 0; i-- ) {
                if ( transform.GetChild( i ).GetComponent<RoadSection>() ) {
                    UnityEventTools.RemovePersistentListener( transform.GetChild( i ).GetComponent<RoadSection>().OnModified, OnRoadSectionModified );
                    UtilityZ.SafeDestroy( transform.GetChild( i ).gameObject );
                }
            }
            NotifyModified();
        }

        void OnDestroy() {
            if ( Application.isEditor && !Application.isPlaying ) {
                foreach ( RoadSection rs in roadSections ) {
                    UnityEventTools.RemovePersistentListener( rs.OnModified, OnRoadSectionModified );
                }

                if ( Roadway != null )
                    Roadway.RemoveRoad( this, false );
            }
        }

        void OnEnable() {
            foreach ( RoadSection rs in roadSections ) {
                UnityEventTools.RemovePersistentListener( rs.OnModified, OnRoadSectionModified );
                UnityEventTools.AddPersistentListener( rs.OnModified, OnRoadSectionModified );
                rs.OnModified.SetPersistentListenerState( rs.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );
            }
        }

        void OnDisable() {
            foreach ( RoadSection rs in roadSections ) {
                UnityEventTools.RemovePersistentListener( rs.OnModified, OnRoadSectionModified );
            }
        }

    }

}
