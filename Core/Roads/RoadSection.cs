using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.Events;

using Roadway.Spline;
using Roadway.Junctions;


namespace Roadway.Roads {

    public class RoadSection : SplineObject {

        #region Modify Event 

        [SerializeField, HideInInspector]
        public UnityEvent OnModified = new UnityEvent();

        // Called when the Road Section is modified
        /// <summary> if suppressOnModify is false the event is triggered to modify those subscribed</summary>
        public void NotifyModified(bool suppressOnModify = false) {
            CreateMesh();
            UpdateBoundingBox();
            UpdateLanes();
            UpdateSnapPoints();
            UpdateSnappedObjects();
            if ( !suppressOnModify && OnModified != null ) {
                OnModified.Invoke();
            }
        }

        void OnPathModified() {
            NotifyModified();
        }

        void UndoCallback() {
            NotifyModified();
        }

        # endregion

        # region Bounds 

        [SerializeField, HideInInspector]
        protected Bounds bounds;

        public Bounds Bounds {
            get { return bounds; }
        }

        private Bounds UpdateBoundingBox() {

            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;

            if ( MakeMesh && meshFilter != null && meshFilter.sharedMesh != null ) {
                foreach ( Vector3 vert in meshFilter.sharedMesh.vertices ) {
                    // if one of the meshes has a zero width and zero lane the resulting vertex will be zero
                    // the overtaking lanes are the main cause of this.
                    if ( vert != Vector3.zero ) {
                        min = new Vector3( Mathf.Min( min.x, vert.x ), Mathf.Min( min.y, vert.y ), Mathf.Min( min.z, vert.z ) );
                        max = new Vector3( Mathf.Max( max.x, vert.x ), Mathf.Max( max.y, vert.y ), Mathf.Max( max.z, vert.z ) );
                    }
                }
            }

            if ( MakeAI ) {
                foreach ( Lane lane in LeftLanes ) {
                    foreach ( Vector3 vert in lane.points ) {
                        min = new Vector3( Mathf.Min( min.x, vert.x ), Mathf.Min( min.y, vert.y ), Mathf.Min( min.z, vert.z ) );
                        max = new Vector3( Mathf.Max( max.x, vert.x ), Mathf.Max( max.y, vert.y ), Mathf.Max( max.z, vert.z ) );
                    }
                }
                foreach ( Lane lane in RightLanes ) {
                    foreach ( Vector3 vert in lane.points ) {
                        min = new Vector3( Mathf.Min( min.x, vert.x ), Mathf.Min( min.y, vert.y ), Mathf.Min( min.z, vert.z ) );
                        max = new Vector3( Mathf.Max( max.x, vert.x ), Mathf.Max( max.y, vert.y ), Mathf.Max( max.z, vert.z ) );
                    }
                }
            }

            for ( int i = 0; i < PathNumPoints; i++ ) {
                Vector3 vert = Path.Point( i );
                min = new Vector3( Mathf.Min( min.x, vert.x ), Mathf.Min( min.y, vert.y ), Mathf.Min( min.z, vert.z ) );
                max = new Vector3( Mathf.Max( max.x, vert.x ), Mathf.Max( max.y, vert.y ), Mathf.Max( max.z, vert.z ) );
            }

            bounds = new Bounds( ( min + max ) / 2, max - min );
            bounds.Expand( 1 );
            return bounds;
        }

        # endregion

        # region Road

        private Road road;
        public Road Road {
            get {
                if ( road == null )
                    road = transform.parent.GetComponent<Road>();
                return road;
            }
        }

        # endregion

        # region Properties

        [SerializeField, HideInInspector]
        private RoadSectionNameOperation nameOperation = RoadSectionNameOperation.None;
        [SerializeField, HideInInspector]
        private string roadSectionName;
        [SerializeField, HideInInspector]
        private RoadSectionSpeedOperation speedOperation = RoadSectionSpeedOperation.None;
        [SerializeField, HideInInspector]
        private int roadSectionSpeed;

        [SerializeField, HideInInspector]
        private RoadLines lines = RoadLines.Dashed;

        [SerializeField, HideInInspector]
        private List<Lane> lanesLeft;
        [SerializeField, HideInInspector]
        private List<Lane> lanesRight;

        [SerializeField, HideInInspector]
        private int lanesLeftAddInner = 0;
        [SerializeField, HideInInspector]
        private int lanesLeftAddOuter = 0;
        [SerializeField, HideInInspector]
        private int lanesRightAddInner = 0;
        [SerializeField, HideInInspector]
        private int lanesRightAddOuter = 0;

        [SerializeField, HideInInspector]
        private AnimationCurve centerWidthCurveLeft = new AnimationCurve( new Keyframe( 0, 0f ), new Keyframe( .5f, 0f ), new Keyframe( 1, 0f ) );
        [SerializeField, HideInInspector]
        private AnimationCurve centerWidthCurveRight = new AnimationCurve( new Keyframe( 0, 0f ), new Keyframe( .5f, 0f ), new Keyframe( 1, 0f ) );
        [SerializeField, HideInInspector]
        private bool centerWidthCurveMirror = true;

        [SerializeField, HideInInspector]
        private float extraRoadWidthAtStart = 0;
        [SerializeField, HideInInspector]
        private float extraRoadWidthAtStartDst = 5;
        [SerializeField, HideInInspector]
        private float extraRoadWidthAtEnd = 0;
        [SerializeField, HideInInspector]
        private float extraRoadWidthAtEndDst = 5;

        [SerializeField, HideInInspector]
        private float transitionDistance = 10;


        // if true the center mesh is hidden
        // intended for highways and freeways that have a wide grass area
        [SerializeField, HideInInspector]
        private bool splitSides = false;

        [SerializeField, HideInInspector]
        private bool reverseDirection = false;

        [SerializeField, HideInInspector]
        private bool makeMesh = true;
        [SerializeField, HideInInspector]
        private bool makeAI = true;

        [SerializeField, HideInInspector]
        bool locked = false;

        public bool IsLocked {
            get { return locked; }
            set {
                locked = value;
                Path.IsLocked = value;
            }
        }

        public Vector3 up {
            get { return Vector3.up; }
        }

        public string RoadSectionName {
            get { return roadSectionName; }
            set {
                if ( roadSectionName != value ) {
                    roadSectionName = value;
                    // NotifyModified();
                }
            }
        }

        public int RoadSectionSpeed {
            get { return roadSectionSpeed; }
            set {
                if ( roadSectionSpeed != value ) {
                    roadSectionSpeed = value;
                    // NotifyModified();
                }
            }
        }

        public RoadSectionNameOperation RoadSectionNameOperation {
            get { return nameOperation; }
            set {
                if ( nameOperation != value ) {
                    nameOperation = value;
                }
            }
        }

        public RoadSectionSpeedOperation RoadSectionSpeedOperation {
            get { return speedOperation; }
            set {
                if ( speedOperation != value ) {
                    speedOperation = value;
                }
            }
        }

        public RoadLines RoadLines {
            get { return lines; }
            set {
                if ( lines != value ) {
                    lines = value;
                    NotifyModified();
                }
            }
        }
        public bool IsSplit {
            get { return splitSides; }
            set {
                if ( splitSides != value ) {
                    splitSides = value;
                    NotifyModified();
                }
            }
        }

        public bool MakeMesh {
            get { return makeMesh; }
            set {
                if ( makeMesh != value ) {
                    makeMesh = value;

                    if ( !makeMesh ) {
                        TrySetMesh( null );
                        TrySetCollider( null );
                    }

                    NotifyModified();
                }
            }
        }

        public bool MakeAI {
            get { return makeAI; }
            set {
                if ( makeAI != value ) {
                    makeAI = value;

                    if ( !makeAI ) {
                        foreach ( Lane lane in LeftLanes )
                            lane.Clear();
                        foreach ( Lane lane in RightLanes )
                            lane.Clear();
                    }

                    NotifyModified();
                }
            }
        }

        public Lane LeftLane(int i) {
            return LeftLanes[ i ];
        }

        public Lane RightLane(int i) {
            return RightLanes[ i ];
        }

        public List<Lane> LeftLanes {
            get { return ReverseDirection ? lanesRight : lanesLeft; }
            private set {
                if ( ReverseDirection ) {
                    lanesRight = value;
                } else {
                    lanesLeft = value;
                }
            }
        }

        public List<Lane> RightLanes {
            get { return ReverseDirection ? lanesLeft : lanesRight; }
            private set {
                if ( ReverseDirection ) {
                    lanesLeft = value;
                } else {
                    lanesRight = value;
                }
            }
        }

        public int numLanesLeft {
            get { return LeftLanes.Count; }
        }

        public int numLanesRight {
            get { return RightLanes.Count; }
        }

        public int numLanesLeftInner {
            get { return ReverseDirection ? lanesRightAddInner : lanesLeftAddInner; }
            private set {
                if ( ReverseDirection ) {
                    lanesRightAddInner = value;
                } else {
                    lanesLeftAddInner = value;
                }
            }
        }

        public int numLanesLeftOuter {
            get { return lanesLeftAddOuter; }
            private set {
                if ( ReverseDirection ) {
                    lanesRightAddOuter = value;
                } else {
                    lanesLeftAddOuter = value;
                }
            }
        }

        public int numLanesRightInner {
            get { return ReverseDirection ? lanesLeftAddInner : lanesRightAddInner; }
            private set {
                if ( ReverseDirection ) {
                    lanesLeftAddInner = value;
                } else {
                    lanesRightAddInner = value;
                }
            }
        }

        public int numLanesRightOuter {
            get { return lanesRightAddOuter; }
            private set {
                if ( ReverseDirection ) {
                    lanesLeftAddOuter = value;
                } else {
                    lanesRightAddOuter = value;
                }
            }
        }

        public bool ReverseDirection {
            get { return reverseDirection; }
            set {
                if ( reverseDirection != value ) {
                    reverseDirection = value;
                    NotifyModified();
                }
            }
        }

        public AnimationCurve CenterWidthCurveLeft {
            get { return centerWidthCurveLeft; }
            set {
                centerWidthCurveLeft = value;
                NotifyModified();
            }
        }

        public AnimationCurve CenterWidthCurveRight {
            get { return centerWidthCurveRight; }
            set {
                centerWidthCurveRight = value;
                NotifyModified();
            }
        }

        public bool CenterWidthCurveMirror {
            get { return centerWidthCurveMirror; }
            set {
                centerWidthCurveMirror = value;
                NotifyModified();
            }
        }

        public float ExtraRoadWidthAtStart {
            get { return extraRoadWidthAtStart; }
            set {
                extraRoadWidthAtStart = value;
                NotifyModified();
            }
        }

        public float ExtraRoadWidthAtStartDst {
            get { return extraRoadWidthAtStartDst; }
            set {
                extraRoadWidthAtStartDst = value;
                NotifyModified();
            }
        }

        public float ExtraRoadWidthAtEnd {
            get { return extraRoadWidthAtEnd; }
            set {
                extraRoadWidthAtEnd = value;
                NotifyModified();
            }
        }

        public float ExtraRoadWidthAtEndDst {
            get { return extraRoadWidthAtEndDst; }
            set {
                extraRoadWidthAtEndDst = value;
                NotifyModified();
            }
        }

        public float RoadLift {
            get { return RoadwaySettings.RoadLift; }
        }

        // TODO use WrapUVs?
        public bool WrapUVs {
            get { return false; }
        }

        // TODO use UVsXValReverse?
        public bool UVsXValReverse {
            get { return false; }
        }

        public RoadTravelDirections TravelDirection {
            get { return Road.TravelDirection; }
        }

        public RoadDrivingSide DrivingSide {
            get { return RoadwaySettings.RoadDrivingSide; }
        }

        public Vector3[] CalculateEvenlySpacedPoints {
            get { return Path.CalculateEvenlySpacedPoints( RoadwayGUISettings.spacing, RoadwayGUISettings.resolution ); }
        }

        public float LaneWidth {
            get { return RoadwaySettings.LaneWidth; }
        }

        public bool IsDashed {
            get { return RoadLines == RoadLines.Dashed; }
        }

        public float TransitionDistance {
            get { return transitionDistance; }
            set {
                transitionDistance = value;
                NotifyModified();
            }
        }

        public RoadSection NextRoadSection {
            get {
                SplineSnapPoint ssp = SnapOfHandle( PathNumPoints - 1 );
                if ( ssp != null && ssp.IsOccupied && ssp.GetOther( this ) is RoadSection )
                    return (RoadSection)ssp.GetOther( this );
                return null;
            }
        }

        public RoadSection PreviousRoadSection {
            get {
                SplineSnapPoint ssp = SnapOfHandle( 0 );
                if ( ssp != null && ssp.IsOccupied && ssp.GetOther( this ) is RoadSection )
                    return (RoadSection)ssp.GetOther( this );
                return null;
            }
        }

        public Junction NextJunction {
            get {
                SplineSnapPoint ssp = SnapOfHandle( PathNumPoints - 1 );
                if ( ssp != null && ssp.IsOccupied && ssp.GetOther( this ) is Junction )
                    return (Junction)ssp.GetOther( this );
                return null;
            }
        }

        public Junction PreviousJunction {
            get {
                SplineSnapPoint ssp = SnapOfHandle( 0 );
                if ( ssp != null && ssp.IsOccupied && ssp.GetOther( this ) is Junction )
                    return (Junction)ssp.GetOther( this );
                return null;
            }
        }

        #endregion

        #region Mesh Control

        [SerializeField, HideInInspector]
        int textureRepeat = 1;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        bool TrySetCollider(Mesh mesh) {
            if ( GetComponent<MeshCollider>() == null ) {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            if ( meshCollider == null ) {
                meshCollider = GetComponent<MeshCollider>();
            }
            if ( meshCollider != null ) {
                meshCollider.sharedMesh = mesh;
                return true;
            }
            return false;
        }

        bool TrySetMesh(Mesh mesh) {
            if ( GetComponent<MeshFilter>() == null ) {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            if ( meshFilter == null ) {
                meshFilter = GetComponent<MeshFilter>();
            }
            if ( meshFilter != null ) {
                meshFilter.sharedMesh = mesh;
                return true;
            }
            return false;
        }

        bool TrySetMaterial(List<Material> materials) {
            if ( GetComponent<MeshRenderer>() == null ) {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            if ( meshRenderer == null ) {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            if ( meshRenderer != null ) {
                meshRenderer.sharedMaterials = materials.ToArray();
                return true;
            }
            return false;
        }

        # endregion

        public bool LaneFlowDirection(bool isLeft) {
            if ( RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Left )
                return !isLeft;
            // else RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Right
            return isLeft;
        }

        public void Init(RoadPreset preset, string name = "", int speed = int.MinValue) {
            Undo.undoRedoPerformed -= UndoCallback;
            Undo.undoRedoPerformed += UndoCallback;

            roadSectionName = name;
            if ( roadSectionName == "" && preset != null ) {
                roadSectionName = preset.roadName;
            }

            roadSectionSpeed = speed;
            if ( roadSectionSpeed == int.MinValue && preset != null ) {
                roadSectionSpeed = preset.roadSpeed;
            }

            int numLanesLeft = 1;
            int numLanesRight = 1;

            if ( preset != null ) {
                numLanesLeft = preset.numLanesLeft;
                numLanesRight = preset.numLanesRight;
                // centerWidthCurve = preset.centerWidthCurve; TODO centerWidth from preset
                splitSides = preset.splitSides;
                lines = preset.roadLine;
                makeMesh = preset.makeMesh;
                makeAI = preset.makeAI;
            }

            LeftLanes = new List<Lane>( numLanesLeft );
            for ( int i = 0; i < numLanesLeft; i++ )
                LeftLanes.Add( Lane.NewLane( LaneFlowDirection( true ) ) );

            RightLanes = new List<Lane>( numLanesRight );
            for ( int i = 0; i < numLanesRight; i++ )
                RightLanes.Add( Lane.NewLane( LaneFlowDirection( false ) ) );

            path = new SplineBezier();
            UnityEventTools.RemovePersistentListener( path.OnModified, OnPathModified );
            UnityEventTools.AddPersistentListener( path.OnModified, OnPathModified );
            path.OnModified.SetPersistentListenerState( path.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );
        }

        public void Init(RoadSection roadSection) {
            this.transform.position = roadSection.gameObject.transform.position;
            Undo.undoRedoPerformed -= UndoCallback;
            Undo.undoRedoPerformed += UndoCallback;

            nameOperation = roadSection.nameOperation;
            speedOperation = roadSection.speedOperation;
            roadSectionName = roadSection.roadSectionName;
            roadSectionSpeed = roadSection.roadSectionSpeed;
            lines = roadSection.lines;

            LeftLanes = new List<Lane>( roadSection.numLanesLeft );
            for ( int i = 0; i < roadSection.numLanesLeft; i++ )
                LeftLanes.Add( Lane.NewLane( LaneFlowDirection( true ) ) );

            RightLanes = new List<Lane>( roadSection.numLanesRight );
            for ( int i = 0; i < roadSection.numLanesRight; i++ )
                RightLanes.Add( Lane.NewLane( LaneFlowDirection( false ) ) );

            path = new SplineBezier( roadSection.path );
            UnityEventTools.RemovePersistentListener( path.OnModified, OnPathModified );
            UnityEventTools.AddPersistentListener( path.OnModified, OnPathModified );
            path.OnModified.SetPersistentListenerState( path.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );
        }

        public RoadWidths WidthAtHandle(int handleIndex) {
            return new RoadWidths( this, handleIndex );
        }

        public Vector3 SplineSnapPointPosition(int handleIndex) {
            SplineSnapPoint ssp = SnapOfHandle( handleIndex );
            if ( ssp == null )
                return Vector3.negativeInfinity;

            SnappableObject sspOtherObj = ssp.GetOther( this );
            if ( sspOtherObj == null )
                return Vector3.negativeInfinity;

            int otherHandleIndex = ssp.GetHandleIndex( sspOtherObj );

            if ( sspOtherObj is Junction ) {
                // Vector3 pos = ( (Junction)sspOtherObj ).Position;
                // Vector3 dir = ssp.snapPoint - pos;
                // return ssp.snapPoint - dir.normalized * dir.magnitude;
                return ( (Junction)sspOtherObj ).Position;
            }

            if ( sspOtherObj is SplineObject ) {
                if ( otherHandleIndex == 0 )
                    return ( (SplineObject)sspOtherObj ).Path.Point( 1 );
                else
                    return ( (SplineObject)sspOtherObj ).Path.PointReverse( 1 );
            }

            return Vector3.negativeInfinity;
        }

        public void CreateMesh() {
            if ( PathNumPoints < 3 || !MakeMesh )
                return;

            RoadMeshGenerator rmg = new RoadMeshGenerator( this );

            float pathLength = Path.EstimatePathLength();

            textureRepeat = Mathf.RoundToInt( RoadwaySettings.Tiling * pathLength * RoadwayGUISettings.spacing * .05f );

            TrySetMesh( rmg.mesh );
            TrySetCollider( rmg.mesh );

            List<Material> mats = new List<Material>();
            bool isReversed = false;
            foreach ( RoadMeshGenerator.TriTypes triType in rmg.triTypes ) {
                switch ( triType ) {
                    case RoadMeshGenerator.TriTypes.Center:
                        mats.Add( Roadway.RoadwayMaterialManager.GetCenterMaterial() );
                        break;
                    case RoadMeshGenerator.TriTypes.Left:
                        mats.Add( Roadway.RoadwayMaterialManager.GetLaneMaterial( numLanesLeft, IsDashed, IsSplit, true, isReversed ) );
                        break;
                    case RoadMeshGenerator.TriTypes.Right:
                        mats.Add( Roadway.RoadwayMaterialManager.GetLaneMaterial( numLanesRight, IsDashed, IsSplit, false, isReversed ) );
                        break;
                    case RoadMeshGenerator.TriTypes.LeftTransitionInner:
                        if ( NextRoadSection == null )
                            throw new System.Exception("The next RoadSection was found to be null but a Transition sub mesh was created");
                        mats.Add( Roadway.RoadwayMaterialManager.GetLaneMaterial( numLanesLeft, IsDashed, IsSplit, true, isReversed ) );
                        break;
                    case RoadMeshGenerator.TriTypes.LeftTransitionOuter:
                        if ( NextRoadSection == null )
                            throw new System.Exception("The next RoadSection was found to be null but a Transition sub mesh was created");
                        mats.Add( Roadway.RoadwayMaterialManager.GetLaneTransitionMaterial( numLanesLeft, NextRoadSection.numLanesLeft, IsDashed, IsSplit, true, isReversed ) );
                        break;
                    case RoadMeshGenerator.TriTypes.RightTransitionInner:
                        if ( NextRoadSection == null )
                            throw new System.Exception("The next RoadSection was found to be null but a Transition sub mesh was created");
                        mats.Add( Roadway.RoadwayMaterialManager.GetLaneMaterial( numLanesRight, IsDashed, IsSplit, false, isReversed ) );
                        break;
                    case RoadMeshGenerator.TriTypes.RightTransitionOuter:
                        if ( NextRoadSection == null )
                            throw new System.Exception("The next RoadSection was found to be null but a Transition sub mesh was created");
                        mats.Add( Roadway.RoadwayMaterialManager.GetLaneTransitionMaterial( numLanesRight, NextRoadSection.numLanesRight, IsDashed, IsSplit, false, isReversed ) );
                        break;
                }
                isReversed = !isReversed;
            }

            TrySetMaterial( mats );
        }

        public (float left, float right) CenterWidth(float t) {
            float centerLeft = CenterWidthCurveLeft.Evaluate( t );
            float centerRight = CenterWidthCurveRight.Evaluate( t );

            if ( CenterWidthCurveMirror ) {
                if ( RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Right ) {
                    return (centerRight, centerRight);
                } else if ( RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Left ) {
                    return (centerLeft, centerLeft);
                }
            }

            return (centerLeft, centerRight);
        }

        public void UpdateLanes() {
            if ( PathNumPoints < 3 || !MakeAI )
                return;

            Vector3[] pathPoints = Path.CalculateEvenlySpacedPoints( RoadwayGUISettings.spacing, RoadwayGUISettings.resolution );
            float laneWidth = RoadwaySettings.LaneWidth;

            // clear lanes
            foreach ( Lane lane in LeftLanes )
                lane.Clear();
            foreach ( Lane lane in RightLanes )
                lane.Clear();

            RoadTravelDirections td = Road.TravelDirection;
            bool isBoth = td == RoadTravelDirections.Bothways;
            bool isLeft = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Left && td == RoadTravelDirections.Oneway;
            bool isRight = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Right && td == RoadTravelDirections.Oneway;

            // update points
            for ( int i = 0; i < pathPoints.Length; i++ ) {
                SplinePointDirectionalVectors vectors = new SplinePointDirectionalVectors( pathPoints, i );

                (float left, float right) cWidth = CenterWidth( i / (float)pathPoints.Length );

                if ( isBoth ) {
                    for ( int l = 0; l < numLanesLeft; l++ ) {
                        Vector3 newPoint = pathPoints[ i ] + vectors.leftVector * ( cWidth.left + ( l * laneWidth ) + ( laneWidth / 2 ) );
                        LeftLanes[ l ].AddPoint( newPoint );
                    }
                    for ( int l = 0; l < numLanesRight; l++ ) {
                        Vector3 newPoint = pathPoints[ i ] + vectors.rightVector * ( cWidth.right + ( l * laneWidth ) + ( laneWidth / 2 ) );
                        RightLanes[ l ].AddPoint( newPoint );
                    }
                } else {

                    if ( isLeft ) {

                        float offset = ( numLanesLeft * laneWidth ) / 2;
                        for ( int l = 0; l < numLanesLeft; l++ ) {
                            Vector3 newPoint = pathPoints[ i ] + vectors.leftVector * ( ( l * laneWidth ) + ( laneWidth / 2 ) - offset );
                            LeftLanes[ l ].AddPoint( newPoint );
                        }

                    } else if ( isRight ) {

                        float offset = ( numLanesRight * laneWidth ) / 2;
                        for ( int l = 0; l < numLanesRight; l++ ) {
                            Vector3 newPoint = pathPoints[ i ] + vectors.rightVector * ( ( l * laneWidth ) + ( laneWidth / 2 ) - offset );
                            RightLanes[ l ].AddPoint( newPoint );
                        }

                    }

                }

            }

            // Finalize Lane
            foreach ( Lane lane in LeftLanes )
                lane.FinalizeLane( LaneFlowDirection( true ) );
            foreach ( Lane lane in RightLanes )
                lane.FinalizeLane( LaneFlowDirection( false ) );

        }

        void UpdateSnapPoints() {
            if ( PathNumPoints < 3 || path.IsClosed )
                return;

            Vector3[] pathPoints = Path.CalculateEvenlySpacedPoints( RoadwayGUISettings.spacing, RoadwayGUISettings.resolution );
            float laneWidth = RoadwaySettings.LaneWidth;

            if ( pathPoints.Length < 1 )
                return;

            RoadTravelDirections td = Road.TravelDirection;
            bool isBoth = td == RoadTravelDirections.Bothways;
            bool isLeft = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Left && td == RoadTravelDirections.Oneway;
            bool isRight = RoadwaySettings.RoadDrivingSide == RoadDrivingSide.Right && td == RoadTravelDirections.Oneway;

            SplinePointDirectionalVectors vectorsStart = new SplinePointDirectionalVectors( pathPoints, 0 );
            SplinePointDirectionalVectors vectorsEnd = new SplinePointDirectionalVectors( pathPoints, pathPoints.Length - 1 );

            // set all as unused
            for ( int i = 0; i < NumSnaps; i++ ) {
                Snap( i ).used = false;
            }

            if ( isBoth ) {
                Vector3 pointStart = Path.Point( 0 );
                Vector3 pointEnd = Path.PointReverse( 0 );
                AddUpdateRoadConnectionSnapPoint( pointStart, -1, -1, 0 );
                AddUpdateRoadConnectionSnapPoint( pointEnd, -1, -1, PathNumPoints - 1 );
            }

            if ( isBoth || isLeft ) {
                for ( int l = 0; l < numLanesLeft; l++ ) {
                    Vector3 pointStart = Path.Point( 0 ) + vectorsStart.leftVector * ( CenterWidth( 0 ).left + ( l * laneWidth ) + ( laneWidth / 2 ) );
                    Vector3 pointEnd = Path.PointReverse( 0 ) + vectorsEnd.leftVector * ( CenterWidth( 1 ).left + ( l * laneWidth ) + ( laneWidth / 2 ) );
                    AddUpdateRoadConnectionSnapPoint( pointStart, l, -1, 0 );
                    AddUpdateRoadConnectionSnapPoint( pointEnd, l, -1, PathNumPoints - 1 );
                }
            }

            if ( isBoth || isRight ) {
                for ( int l = 0; l < numLanesRight; l++ ) {
                    Vector3 pointStart = Path.Point( 0 ) + vectorsStart.rightVector * ( CenterWidth( 0 ).right + ( l * laneWidth ) + ( laneWidth / 2 ) );
                    Vector3 pointEnd = Path.PointReverse( 0 ) + vectorsEnd.rightVector * ( CenterWidth( 1 ).right + ( l * laneWidth ) + ( laneWidth / 2 ) );
                    AddUpdateRoadConnectionSnapPoint( pointStart, -1, l, 0 );
                    AddUpdateRoadConnectionSnapPoint( pointEnd, -1, l, PathNumPoints - 1 );
                }
            }

            // need to remove RoadConnection snaps
            // check if any are still marked as unused
            for ( int i = NumSnaps - 1; i >= 0; i-- ) {
                if ( !Snap( i ).used ) {
                    // make sure nothing else is using this snap
                    Unsnap( Snap( i ) );
                    snaps.RemoveAt( i );
                }
            }
        }

        public SnappableObject Unsnap(SplineSnapPoint ssp) {
            if ( ssp == null )
                return null;

            SnappableObject other = ssp.GetOther( this );

            SnappableObject a = ssp.anchorObject;
            SnappableObject s = ssp.snappedObject;

            if ( a != null )
                a.RemoveSnapPoint( ssp );
            if ( s != null )
                s.RemoveSnapPoint( ssp );

            return other;
        }

        public SnappableObject UnsnapHandle(int handleIndex) {
            return Unsnap( SnapOfHandle( handleIndex ) );
        }

        public void SnapHandle(int handleIndex, Vector3 pos, Vector3 dir, float dst, bool lockDST = true, bool suppressOnModify = false) {
            // only snap if point is true in snap list
            if ( SnapOfHandle( handleIndex ) == null || !SnapOfHandle( handleIndex ).IsOccupied )
                return;

            if ( handleIndex == 0 ) {
                Vector3 newPoint = path.Point( 1 );

                if ( lockDST ) {
                    newPoint = pos + dir * dst;
                } else {
                    dst = ( path.Point( 3 ) - pos ).magnitude * .3f;
                    newPoint = pos + dir * dst;
                }

                if ( path.Point( handleIndex ) != pos || path.Point( 1 ) != newPoint ) {
                    path.Point( handleIndex, pos );
                    path.Point( 1, newPoint );
                    if ( !suppressOnModify )
                        path.NotifyModified();
                }

            } else if ( handleIndex == PathNumPoints - 1 ) {
                Vector3 newPoint = path.PointReverse( 1 );

                if ( lockDST ) {
                    newPoint = pos + dir * dst;
                } else {
                    dst = ( path.PointReverse( 3 ) - pos ).magnitude * .3f;
                    newPoint = pos + dir * dst;
                }

                if ( path.Point( handleIndex ) != pos || path.PointReverse( 1 ) != newPoint ) {
                    path.Point( handleIndex, pos );
                    path.PointReverse( 1, newPoint );
                    if ( !suppressOnModify )
                        path.NotifyModified();
                }

            }

        }

        void UpdateSnappedObjects() {
            for ( int i = 0; i < NumSnaps; i++ ) {
                SplineSnapPoint ssp = Snap( i );

                if ( ssp == null || !ssp.IsOccupied )
                    continue;

                Vector3 controlHandleA = Vector3.zero;
                Vector3 dirA = Vector3.zero;
                if ( ssp.anchorObject is Junction ) {
                    controlHandleA = ( (Junction)ssp.anchorObject ).Position;
                    dirA = ( (Junction)ssp.anchorObject ).SnapPoint( ssp.anchorHandleIndex ) - controlHandleA;
                } else {
                    controlHandleA = ssp.anchorHandleIndex == 0 ? ssp.AnchorRoadSection.Path.Point( 1 ) : ssp.AnchorRoadSection.Path.PointReverse( 1 );
                    dirA = ssp.AnchorRoadSection.Path.Point( ssp.anchorHandleIndex ) - controlHandleA;
                }
                ssp.SnappedRoadSection.SnapHandle( ssp.snappedHandleIndex, ssp.snapPoint, dirA.normalized, dirA.magnitude, false );


                if ( ssp.snappedObject is RoadSection && ssp.anchorObject is RoadSection ) {
                    Vector3 controlHandleS = ssp.snappedHandleIndex == 0 ? ssp.SnappedRoadSection.Path.Point( 1 ) : ssp.SnappedRoadSection.Path.PointReverse( 1 );
                    Vector3 dirS = ssp.SnappedRoadSection.Path.Point( ssp.snappedHandleIndex ) - controlHandleS;
                    ssp.AnchorRoadSection.SnapHandle( ssp.anchorHandleIndex, ssp.snapPoint, dirS.normalized, dirS.magnitude, false );
                }

            }
        }

        void AddUpdateRoadConnectionSnapPoint(Vector3 point, int leftLaneIndex, int rightLaneIndex, int handleIndex) {
            for ( int i = 0; i < NumSnaps; i++ ) {
                if ( Snap( i ).AreSame( this, handleIndex, leftLaneIndex, rightLaneIndex ) ) {
                    Snap( i ).UpdateInfo( point );
                    return;
                }
            }
            // else didn't find a snap that matches
            // create a new snap
            SplineSnapPoint newSnap = SplineSnapPoint.NewSplineSnapPoint( this, handleIndex );
            newSnap.UpdateInfo( point, leftLaneIndex, rightLaneIndex );
            snaps.Add( newSnap );
        }

        public override bool AddSnapPoint(int snapIndex, SnappableObject snappableObject, int handleIndex) {
            if ( !( snappableObject is RoadSection ) )
                return false;

            RoadSection toSnap = (RoadSection)snappableObject;
            SplineSnapPoint ssp = Snap( snapIndex );

            if ( ssp.IsOccupied )
                return false;

            ssp.SetSnapped( toSnap, handleIndex );
            ssp.IsBlocked = true;

            toSnap.SnapOfHandle( handleIndex, ssp );
            toSnap.PathLocks( handleIndex, true );
            PathLocks( ssp.anchorHandleIndex, true );

            // as dumb as this is, it works
            // makes the meshes align
            UpdateSnappedObjects();
            toSnap.NotifyModified();
            NotifyModified();

            return true;
        }

        public override bool RemoveSnapPoint(SplineSnapPoint ssp) {
            if ( !snaps.Contains( ssp ) || !ssp.Contains( this ) )
                return false;

            int snapIndex = snaps.IndexOf( ssp );
            int handleIndex = ssp.IsAnchor( this ) ? ssp.anchorHandleIndex : ssp.snappedHandleIndex;
            SplineSnapPoint newSSP = SplineSnapPoint.NewSplineSnapPoint( this, handleIndex );

            if ( ssp.anchorObject != null && ssp.anchorObject is SplineObject ) {
                ( (SplineObject)ssp.anchorObject ).PathLocks( ssp.anchorHandleIndex, false );
                newSSP.UpdateInfo( ssp.snapPoint, ssp.leftLaneIndex, ssp.rightLaneIndex );
            }
            if ( ssp.snappedObject != null && ssp.snappedObject is SplineObject )
                ( (SplineObject)ssp.snappedObject ).PathLocks( ssp.snappedHandleIndex, false );

            snaps[ snapIndex ] = newSSP;

            return true;
        }

        public void SuppressedPathSnap(int i, Vector3 pos, Vector3 dir, float dst, bool lockDST = true) {
            SnapHandle( i, pos, dir, dst, lockDST, true );
            NotifyModified( true );
        }

        public void AddLane(RoadSide roadSide) {
            if ( ( roadSide == RoadSide.Left || roadSide == RoadSide.LeftInner ) && numLanesLeft < RoadwaySettings.MaxLanesPerRoadSide ) {
                LeftLanes.Insert( 0, Lane.NewLane( LaneFlowDirection( true ) ) );
                numLanesLeftInner++;
            } else if ( roadSide == RoadSide.LeftOuter && numLanesLeft < RoadwaySettings.MaxLanesPerRoadSide ) {
                LeftLanes.Add( Lane.NewLane( LaneFlowDirection( true ) ) );
                numLanesLeftOuter++;
            } else if ( ( roadSide == RoadSide.Right || roadSide == RoadSide.RightInner ) && numLanesRight < RoadwaySettings.MaxLanesPerRoadSide ) {
                RightLanes.Insert( 0, Lane.NewLane( LaneFlowDirection( false ) ) );
                numLanesRightInner++;
            } else if ( roadSide == RoadSide.RightOuter && numLanesRight < RoadwaySettings.MaxLanesPerRoadSide ) {
                RightLanes.Add( Lane.NewLane( LaneFlowDirection( false ) ) );
                numLanesRightOuter++;
            }

            NotifyModified();
        }

        public void RemoveLane(RoadSide roadSide) {
            if ( ( roadSide == RoadSide.Left || roadSide == RoadSide.LeftInner ) && numLanesLeft > 0 ) {
                LeftLanes.RemoveAt( 0 );
                numLanesLeftInner--;
            } else if ( roadSide == RoadSide.LeftOuter && numLanesLeft > 0 ) {
                LeftLanes.RemoveAt( numLanesLeft - 1 );
                numLanesLeftOuter--;
            } else if ( ( roadSide == RoadSide.Right || roadSide == RoadSide.RightInner ) && numLanesRight > 0 ) {
                RightLanes.RemoveAt( 0 );
                numLanesRightInner--;
            } else if ( roadSide == RoadSide.RightOuter && numLanesRight > 0 ) {
                RightLanes.RemoveAt( numLanesRight - 1 );
                numLanesRightOuter--;
            }

            NotifyModified();
        }

        public void Assimilate(int snapIndex, RoadSection roadSec, int handleIndex) {
            Path.Assimilate( Snap( snapIndex ).anchorHandleIndex, roadSec.Path, handleIndex );
            // NotifyModified();
        }

        void OnDestroy() {
            UnityEventTools.RemovePersistentListener( path.OnModified, OnPathModified );
        }

        void OnEnable() {
            UnityEventTools.RemovePersistentListener( path.OnModified, OnPathModified );
            UnityEventTools.AddPersistentListener( path.OnModified, OnPathModified );
            path.OnModified.SetPersistentListenerState( path.OnModified.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime );
        }

        void OnDisable() {
            UnityEventTools.RemovePersistentListener( path.OnModified, OnPathModified );
        }

        void OnDrawGizmos() {

        }

    }

}
