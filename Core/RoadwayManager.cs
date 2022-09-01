using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;
using Roadway.Spline;
using Roadway.AI;

namespace Roadway {

    [ExecuteInEditMode]
    public class RoadwayManager : MonoBehaviour {

        [HideInInspector]
        public bool hasReset = false;

        [SerializeField, HideInInspector]
        private RoadwaySettings _roadwaySettings;
        public RoadwaySettings RoadwaySettings {
            get {
                if ( _roadwaySettings != null )
                    return _roadwaySettings;
                // try and find the default
                var rs = Resources.Load<RoadwaySettings>( "RoadwaySettingsDefault" );
                if ( rs != null ) {
                    _roadwaySettings = rs;
                    return _roadwaySettings;
                }
                // else
                // make new asset?
                // for now throw error
                throw new System.Exception( "No RoadwaySettingsScriptableObject provided and couldn't locate the default in Roadway/Resources/RoadwaySettingsDefault. Please create and provide a RoadwaySettingsDefault in the settings tab of the RoadwayManager" );
            }
            set {
                _roadwaySettings = value;
            }

        }

        [SerializeField, HideInInspector]
        private RoadwayGUISettings _roadwayGUISettings;
        public RoadwayGUISettings RoadwayGUISettings {
            get {
                if ( _roadwayGUISettings != null )
                    return _roadwayGUISettings;
                // try and find the default
                var rgs = Resources.Load<RoadwayGUISettings>( "RoadwayGUISettingsDefault" );
                if ( rgs != null ) {
                    _roadwayGUISettings = rgs;
                    return _roadwayGUISettings;
                }
                // else
                // make new asset?
                // for now throw error
                throw new System.Exception( "No RoadwayGUISettingsScriptableObject provided and couldn't locate the default in Roadway/Resources/RoadwayGUISettingsDefault. Please create and provide a RoadwayGUISettingsScriptableObject in the settings tab of the RoadwayManager" );
            }
            set {
                _roadwayGUISettings = value;
            }
        }

        [SerializeField, HideInInspector]
        private List<RoadPreset> _roadPresets = new List<RoadPreset>();
        private bool roadPresetSearchDone = false;
        public List<RoadPreset> RoadPresets {
            get {
                if ( _roadPresets == null ) {
                    _roadPresets = new List<RoadPreset>();
                }
                if ( _roadPresets.Count == 0 && !roadPresetSearchDone ) {
                    // try and find any
                    var rp = Resources.LoadAll<RoadPreset>( "RoadPresets" );
                    _roadPresets.AddRange( rp );
                    if ( _roadPresets.Count == 0 ) { // if still none
                        Debug.LogWarning( "Roadway: RoadPresets list is empty and couldn't locate any presets at Roadway/Resources/RoadPresets. " +
                                            "It is recommended that a preset is created and added in the roadway settings tab" );
                    }
                    // set this so check is only done once
                    roadPresetSearchDone = true;
                }

                return _roadPresets;
            }
            set {
                _roadPresets = value;

                if ( _roadPresets.Count == 0 )
                    roadPresetSearchDone = false;
            }
        }

        [SerializeField, HideInInspector]
        public int selectedRoadPresetIndex = 0;

        public string[] RoadPresetNames() {
            List<string> names = new List<string>();

            for ( int i = 0; i < _roadPresets.Count; i++ ) {
                if ( _roadPresets[ i ] != null )
                    names.Add( _roadPresets[ i ].presetName );
            }

            return names.ToArray();
        }

        public RoadPreset ActiveRoadPreset() {
            if ( _roadPresets == null || selectedRoadPresetIndex >= _roadPresets.Count )
                return null;
            return RoadPresets[ selectedRoadPresetIndex ];
        }

        [SerializeField, HideInInspector]
        private RoadwayMaterialManager roadwayMaterialManager;
        public RoadwayMaterialManager RoadwayMaterialManager {
            get {
                if ( roadwayMaterialManager == null )
                    roadwayMaterialManager = new RoadwayMaterialManager( this );
                return roadwayMaterialManager;
            }
        }

        [SerializeField, HideInInspector]
        private List<Road> roads = new List<Road>();
        [SerializeField, HideInInspector]
        private List<Junction> junctions = new List<Junction>();

        public GameObject RoadContainer {
            get {
                if ( transform.Find( "Roads" ) )
                    return transform.Find( "Roads" ).gameObject;

                GameObject roadContainer = new GameObject( "Roads" );
                roadContainer.transform.parent = transform;
                roadContainer.AddComponent<RoadwayContainer>();
                return roadContainer;
            }
        }

        public GameObject JunctionContainer {
            get {
                if ( transform.Find( "Junctions" ) )
                    return transform.Find( "Junctions" ).gameObject;

                GameObject JunctionContainer = new GameObject( "Junctions" );
                JunctionContainer.transform.parent = transform;
                JunctionContainer.AddComponent<RoadwayContainer>();
                return JunctionContainer;
            }
        }

        void OnEnable() {
            // int layerMask = LayerMask.GetMask("Roadway");
            // Debug.Log( layerMask );
        }

        void Reset() {
            hasReset = true;

            roadwayMaterialManager = new RoadwayMaterialManager( this );

            // set Road Preset search to false so it searches again
            roadPresetSearchDone = false;
            // invoke RoadPresets so it searches
            int n = RoadPresets.Count;

            if ( RoadContainer != null ) {
                foreach ( Transform child in RoadContainer.transform ) {
                    UtilityZ.SafeDestroy( child.gameObject );
                }
            }
            UtilityZ.SafeDestroy( RoadContainer );

            if ( JunctionContainer != null ) {
                foreach ( Transform child in JunctionContainer.transform ) {
                    UtilityZ.SafeDestroy( child.gameObject );
                }
            }
            UtilityZ.SafeDestroy( JunctionContainer );

            roads.Clear();
            junctions.Clear();

            // make sure there aren't any loose objects
            for ( int i = transform.childCount - 1; i >= 0; i-- ) {
                if ( transform.GetChild( i ).GetComponent<RoadwayContainer>() != null ||
                        transform.GetChild( i ).GetComponent<Road>() != null ||
                        transform.GetChild( i ).GetComponent<Junction>() != null ) {
                    UtilityZ.SafeDestroy( transform.GetChild( i ).gameObject );
                }
            }
        }

        public int IndexOf(Road road) {
            return roads.IndexOf( road );
        }

        public int IndexOf(Junction junc) {
            return junctions.IndexOf( junc );
        }

        public int NumRoads {
            get { return roads.Count; }
        }

        public int NumJunctions {
            get { return junctions.Count; }
        }

        public Road Road(int i) {
            if ( i >= 0 && i < NumRoads )
                return roads[ i ];
            return null;
        }

        public Junction Junction(int i) {
            if ( i >= 0 && i < NumJunctions )
                return junctions[ i ];
            return null;
        }

        public Intersection Intersection(int i) {
            if ( junctions[ i ] is Intersection  )
                return (Intersection) junctions[ i ];
            return null;
        }

        public void NotifyModified() {
            for ( int r = 0; r < NumRoads; r++ ) {
                Road road = Road( r );
                for ( int s = 0; s < road.NumRoadSections; s++ ) {
                    RoadSection roadSection = road.RoadSection( s );
                    roadSection.NotifyModified();
                }
            }

            for ( int i = 0; i < NumJunctions; i++ ) {
                Junction junction = Junction( i );
                // junction.NotifyModified(); TODO
            }
        }

        public Road AddRoad(Vector3 pos) {
            GameObject newRoadGameObject = new GameObject( "Road" );
            Road road = newRoadGameObject.AddComponent<Road>();
            road.transform.parent = RoadContainer.transform;
            road.Init( pos, ActiveRoadPreset() );
            roads.Add( road );
            return road;
        }

        public void RemoveRoad(Road road, bool doDestroy) {
            roads.Remove( road );

            if ( doDestroy )
                UtilityZ.SafeDestroy( road.gameObject );
        }

        public void DeleteAllRoads() {
            roads.Clear();

            if ( RoadContainer != null ) {
                for ( int i = RoadContainer.transform.childCount - 1; i >= 0; i-- ) {
                    if ( RoadContainer.transform.GetChild( i ).GetComponent<Road>() )
                        UtilityZ.SafeDestroy( RoadContainer.transform.GetChild( i ).gameObject );
                }
            }
        }

        public void RemoveJunction(Junction junction, bool doDestroy) {
            junctions.Remove( junction );

            if ( doDestroy )
                UtilityZ.SafeDestroy( junction.gameObject );
        }

        public void DeleteAllJunctions() {
            junctions.Clear();

            if ( JunctionContainer != null ) {
                for ( int i = JunctionContainer.transform.childCount - 1; i >= 0; i-- ) {
                    if ( JunctionContainer.transform.GetChild( i ).GetComponent<Junction>() )
                        UtilityZ.SafeDestroy( JunctionContainer.transform.GetChild( i ).gameObject );
                }
            }

        }

        public Intersection AddIntersection(Vector3 pos) {
            GameObject newIntersectionGameObject = new GameObject( "Intersection" );
            Intersection intersection = newIntersectionGameObject.AddComponent<Intersection>();
            intersection.transform.parent = JunctionContainer.transform;
            intersection.Init( pos );
            junctions.Add( intersection );
            return intersection;
        }

        public Intersection AddIntersection(int roadAIndex, int roadARoadSectionIndex, int roadARoadSectionHandleIndex,
                                            int roadBIndex, int roadBRoadSectionIndex, int roadBRoadSectionHandleIndex,
                                            int roadCIndex, int roadCRoadSectionIndex, int roadCRoadSectionHandleIndex,
                                            int roadDIndex, int roadDRoadSectionIndex, int roadDRoadSectionHandleIndex,
                                            Vector3 pos) {

            GameObject newIntersectionGameObject = new GameObject( "Intersection" );
            Intersection intersection = newIntersectionGameObject.AddComponent<Intersection>();
            intersection.transform.parent = JunctionContainer.transform;

            Road roadA = roadAIndex == -1 ? null : roads[ roadAIndex ];
            Road roadB = roadBIndex == -1 ? null : roads[ roadBIndex ];
            Road roadC = roadCIndex == -1 ? null : roads[ roadCIndex ];
            Road roadD = roadDIndex == -1 ? null : roads[ roadDIndex ];

            RoadSection roadARoadSection = roadA == null ? null : roadARoadSectionIndex == -1 ? null : roadA.RoadSection( roadARoadSectionIndex );
            RoadSection roadBRoadSection = roadB == null ? null : roadBRoadSectionIndex == -1 ? null : roadB.RoadSection( roadBRoadSectionIndex );
            RoadSection roadCRoadSection = roadC == null ? null : roadCRoadSectionIndex == -1 ? null : roadC.RoadSection( roadCRoadSectionIndex );
            RoadSection roadDRoadSection = roadD == null ? null : roadDRoadSectionIndex == -1 ? null : roadD.RoadSection( roadDRoadSectionIndex );

            List<SplineSnapPoint> splineSnapPoints = new List<SplineSnapPoint>();
            splineSnapPoints.Add( SplineSnapPoint.NewSplineSnapPoint( intersection, 0, roadARoadSection, roadARoadSectionHandleIndex ) );
            splineSnapPoints.Add( SplineSnapPoint.NewSplineSnapPoint( intersection, 1, roadBRoadSection, roadBRoadSectionHandleIndex ) );
            splineSnapPoints.Add( SplineSnapPoint.NewSplineSnapPoint( intersection, 2, roadCRoadSection, roadCRoadSectionHandleIndex ) );
            splineSnapPoints.Add( SplineSnapPoint.NewSplineSnapPoint( intersection, 3, roadDRoadSection, roadDRoadSectionHandleIndex ) );

            intersection.Init( pos, splineSnapPoints.ToArray() );
            junctions.Add( intersection );

            return intersection;
        }

        public void CollectChildren() {
            if ( NumRoads == 0 ) {

                for ( int r = 0; r < RoadContainer.transform.childCount; r++ ) {
                    if ( RoadContainer.transform.GetChild( r ).GetComponent<Road>() ) {
                        Road road = RoadContainer.transform.GetChild( r ).GetComponent<Road>();
                        roads.Add( road );
                        if ( road.NumRoadSections == 0 ) {
                            List<RoadSection> roadSections = new List<RoadSection>();
                            for ( int s = 0; s < road.transform.childCount; s++ ) {
                                if ( road.transform.GetChild( s ).GetComponent<RoadSection>() ) {
                                    roadSections.Add( road.transform.GetChild( s ).GetComponent<RoadSection>() );
                                }
                            }
                            road.AddRoadSections( roadSections );
                        }
                    }
                }


            }

            if ( NumJunctions == 0 ) {
                for ( int i = 0; i < JunctionContainer.transform.childCount; i++ ) {
                    if ( JunctionContainer.transform.GetChild( i ).GetComponent<Junction>() ) {
                        junctions.Add( JunctionContainer.transform.GetChild( i ).GetComponent<Junction>() );
                    }
                }
            }
        }

        public void Bake() {

            foreach ( Junction jun in junctions ) {
                jun.ClearJunctionConnection();
            }

            foreach ( Junction jun in junctions ) {
                for ( int s = 0; s < jun.NumSnaps; s++ ) {
                    if ( jun.Snap( s ).IsOccupied ) {

                        RoadSection roadSection = jun.Snap( s ).SnappedRoadSection;
                        int handleConnectedToJunction = jun.Snap( s ).snappedHandleIndex;
                        int otherendHandle = handleConnectedToJunction == 0 ? jun.Snap( s ).SnappedRoadSection.Path.NumPoints - 1 : 0;
                        (Junction junction, int snapIndex) other = RoadwayAI.GetNextJunction( roadSection, otherendHandle );

                        if ( other.junction != null ) {
                            JunctionConnection jc = JunctionConnection.NewJunctionConnection( jun, s, other.junction, other.snapIndex );
                            jun.AddJunctionConnection( jc );
                            other.junction.AddJunctionConnection( jc );
                        }

                    } // end IsOccupied
                } // end for each snap
            } // end foreach Junction

        }

    }

}
