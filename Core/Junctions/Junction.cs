using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor.Events;

using Roadway.Roads;
using Roadway.Spline;

namespace Roadway.Junctions {

    [ExecuteInEditMode]
    public class Junction : SnappableObject {
        
        # region Getters

        public virtual Vector3 Position {
            get { return transform.position; }
            set {
                transform.position = value;
                NotifyModified();
            }
        }

        public virtual Quaternion Rotation {
            get { return transform.rotation; }
            set {
                transform.rotation = value;
                NotifyModified();
            }
        }

        public virtual Vector3 up {
            get { return Vector3.up; }
        }

        # endregion

        # region Generation

        [SerializeField, HideInInspector]
        private bool makeMesh = true;
        [SerializeField, HideInInspector]
        private bool makeAI = true;

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

                    }

                    NotifyModified();
                }
            }
        }

        # endregion

        # region Modify Event

        [SerializeField, HideInInspector]
        public UnityEvent OnModified = new UnityEvent();

        /// <summary> if suppressOnModify is false the event is triggered to modify those subscribed</summary>
        public virtual void NotifyModified(bool suppressOnModify = false) {
            UpdateLinks();
            UpdateBoundingBox();
            if ( !suppressOnModify && OnModified != null ) {
                OnModified.Invoke();
            }
        }

        protected void UndoCallback() {
            NotifyModified();
        }

        # endregion

        # region Bounds

        [SerializeField, HideInInspector]
        protected Bounds bounds;

        public Bounds Bounds {
            get { return bounds; }
        }

        protected Bounds UpdateBoundingBox() {

            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;

            for ( int s = 0; s < NumSnaps; s++ ) {
                Vector3 v = Snap( s ).snapPoint;

                if ( v != Vector3.zero ) {
                    min = new Vector3( Mathf.Min( min.x, v.x ), Mathf.Min( min.y, v.y ), Mathf.Min( min.z, v.z ) );
                    max = new Vector3( Mathf.Max( max.x, v.x ), Mathf.Max( max.y, v.y ), Mathf.Max( max.z, v.z ) );
                }
            }
            Vector3 p = transform.position;

            min = new Vector3( Mathf.Min( min.x, p.x ), Mathf.Min( min.y, p.y ), Mathf.Min( min.z, p.z ) );
            max = new Vector3( Mathf.Max( max.x, p.x ), Mathf.Max( max.y, p.y ), Mathf.Max( max.z, p.z ) );

            bounds = new Bounds( ( min + max ) / 2, max - min );
            bounds.Expand( 5 );
            return bounds;
        }

        # endregion

        # region lock

        [SerializeField, HideInInspector]
        bool locked = false;

        public bool IsLocked {
            get { return locked; }
            set { locked = value; }
        }

        #endregion

        # region Mesh Control

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;

        protected bool TrySetCollider(Mesh mesh) {
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

        protected bool TrySetMesh(Mesh mesh) {
            if ( GetComponent<MeshFilter>() == null ) {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if ( meshFilter == null ) {
                meshFilter = GetComponent<MeshFilter>();
            }

            if ( meshFilter != null ) {
                meshFilter.mesh = mesh;
                return true;
            }

            return false;
        }

        protected bool TrySetMaterial(Material mat) {
            if ( GetComponent<MeshRenderer>() == null )
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if ( meshRenderer == null )
                meshRenderer = GetComponent<MeshRenderer>();

            if ( meshRenderer != null ) {
                meshRenderer.sharedMaterial = mat;
                return true;
            }

            return false;
        }

        # endregion 

        # region Links

        [SerializeField, HideInInspector]
        protected List<JunctionLaneLink> links = new List<JunctionLaneLink>();

        public int numLinks {
            get { return links.Count; }
        }

        public JunctionLaneLink Link(int i) {
            return links[ i ];
        }

        public List<JunctionLaneLink> GetLinksFromOrigin(Lane lane) {
            List<JunctionLaneLink> foundLinks = new List<JunctionLaneLink>();
            for ( int i = 0; i < links.Count; i++ ) {
                if ( links[ i ].origin == lane ) {
                    foundLinks.Add( links[ i ] );
                }
            }
            return foundLinks;
        }

        public List<JunctionLaneLink> GetLinksBetween(int snapIndexInbound, int snapIndexOutBound) {
            List<JunctionLaneLink> foundLinks = new List<JunctionLaneLink>();

            foreach ( JunctionLaneLink link in links ) {
                if ( link.Match( snapIndexInbound, snapIndexOutBound ) )
                    foundLinks.Add( link );
            }

            return foundLinks;
        }

        public virtual void UpdateLinks() {

        }

        #endregion

        # region JunctionConnection

        [SerializeField, HideInInspector]
        protected List<JunctionConnection> junctionConnections = new List<JunctionConnection>();

        public int numJunctionConnections {
            get { return junctionConnections.Count; }
        }

        public JunctionConnection JunctionConnection(int i) {
            return junctionConnections[ i ];
        }

        public JunctionConnection GetJunctionConnection(Junction otherJunc) {
            foreach ( JunctionConnection junCon in junctionConnections ) {
                if ( junCon.Contains( otherJunc ) && junCon.Contains( this ) )
                    return junCon;
            }
            return null;
        }

        public void AddJunctionConnection(JunctionConnection jc, bool suppressDuplicateCheck = false) {
            if ( !suppressDuplicateCheck ) {
                foreach ( JunctionConnection junCon in junctionConnections ) {
                    if ( junCon.AreSame( jc ) )
                        return;
                }
                // else this is new
            }
            junctionConnections.Add( jc );
        }

        public List<Junction> GetConnected() {
            List<Junction> junctions = new List<Junction>();
            foreach ( JunctionConnection junCon in junctionConnections ) {
                Junction other = junCon.GetOther( this );
                if ( !junctions.Contains( other ) )
                    junctions.Add( other );
            }
            return junctions;
        }

        public float GetDistance(Junction otherJunc) {
            foreach ( JunctionConnection junCon in junctionConnections ) {
                if ( junCon.GetOther( this ) == otherJunc )
                    return junCon.distance;
            }
            return float.PositiveInfinity;
        }

        public void ClearJunctionConnection() {
            junctionConnections.Clear();
        }

        #endregion

        # region Snaps

        public override bool AddSnapPoint(int snapIndex, SnappableObject snappableObject, int handleIndex) {
            return false;
        }

        public override bool RemoveSnapPoint(SplineSnapPoint splineSnapPoint) {
            return false;
        }

        # endregion

    }

}
