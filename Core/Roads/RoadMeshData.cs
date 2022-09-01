using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway.Roads {

    [System.Serializable]
    public struct RoadMeshData {
        public int length;
        public Vector3[] verts;
        public Vector2[] uvs;

        public List<int[]> tris;
        public int[] trisCenter;
        public int[] trisLeft;
        public int[] trisRight;
        public int[] trisLeftTransition;
        public int[] trisRightTransition;

        public RoadMeshData(int length, Vector3[] verts, Vector2[] uvs,
            int[] trisCenter, int[] trisLeft, int[] trisRight, int[] trisLeftTransition, int[] trisRightTransition) {

            this.length = length;
            this.verts = verts;
            this.uvs = uvs;
            this.trisCenter = trisCenter;
            this.trisLeft = trisLeft;
            this.trisRight = trisRight;
            this.trisLeftTransition = trisLeftTransition;
            this.trisRightTransition = trisRightTransition;

            this.tris = new List<int[]>() ;
            if ( trisCenter.Length > 0 ) this.tris.Add( trisCenter );
            if ( trisLeft.Length > 0 ) this.tris.Add( trisLeft );
            if ( trisRight.Length > 0 ) this.tris.Add( trisRight );
            if ( trisLeftTransition.Length > 0 ) this.tris.Add( trisLeftTransition );
            if ( trisRightTransition.Length > 0 ) this.tris.Add( trisRightTransition );
        }
    }

}
