using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway.Junctions {

    public class JunctionConnection : ScriptableObject {

        public static JunctionConnection NewJunctionConnection(Junction junctionA, int junctionASnapIndex, Junction junctionB, int junctionBSnapIndex) {
            JunctionConnection jc = CreateInstance<JunctionConnection>();
            jc.Init( junctionA, junctionASnapIndex, junctionB, junctionBSnapIndex );
            return jc;
        }

        [SerializeField]
        public Junction junctionA;
        [SerializeField]
        public int junctionASnapIndex;
        [SerializeField]
        public Junction junctionB;
        [SerializeField]
        public int junctionBSnapIndex;
        [SerializeField]
        public float distance;

        // could get all RoadSections between Junctions and add the total path distance between junctions

        public void Init(Junction junctionA, int junctionASnapIndex, Junction junctionB, int junctionBSnapIndex) {
            this.junctionA = junctionA;
            this.junctionASnapIndex = junctionASnapIndex;
            this.junctionB = junctionB;
            this.junctionBSnapIndex = junctionBSnapIndex;
            this.distance = Vector3.Distance( junctionA.Position, junctionB.Position );
        }

        public bool AreSame(JunctionConnection other) {
            if ( this == other )
                return true;

            bool sameJunctionA = this.junctionA == other.junctionA;
            bool samejunctionASnapIndex = this.junctionASnapIndex == other.junctionASnapIndex;
            bool samejunctionB = this.junctionB == other.junctionB;
            bool samejunctionBSnapIndex = this.junctionBSnapIndex == other.junctionBSnapIndex;
            bool same = sameJunctionA && samejunctionASnapIndex && samejunctionB && samejunctionBSnapIndex;
            // or, junctions could be revered
            bool sameReversedJunctionA = this.junctionA == other.junctionB;
            bool sameReversedjunctionASnapIndex = this.junctionASnapIndex == other.junctionBSnapIndex;
            bool sameReversedjunctionB = this.junctionB == other.junctionA;
            bool sameReversedjunctionBSnapIndex = this.junctionBSnapIndex == other.junctionASnapIndex;
            bool sameReversed = sameReversedJunctionA && sameReversedjunctionASnapIndex && sameReversedjunctionB && sameReversedjunctionBSnapIndex;

            return same || sameReversed;
        }

        public Junction GetOther(Junction otherThenThisOne) {
            if ( junctionA == otherThenThisOne )
                return junctionB;
            else if ( junctionB == otherThenThisOne )
                return junctionA;
            return null;
        }

        public int GetSnapIndexForJunction(Junction junc) {
            if ( junctionA == junc )
                return junctionASnapIndex;
            else if ( junctionB == junc )
                return junctionBSnapIndex;
            return -1;
        }

        public bool Contains(Junction junc) {
            return junctionA == junc || junctionB == junc;
        }

    }

}
