using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;

namespace Roadway.Junctions {

    [System.Serializable]
    public class IntersectionVectors {

        //    forward
        //      rA ( roadA )
        //   vA --- vB ( vectorB )
        //    |     |
        // rC |     | rD
        //    |     |
        //   vC --- vD
        //      rB

        public Vector3 rA;
        public Vector3 rB;
        public Vector3 rC;
        public Vector3 rD;
        public Vector3[] rs;

        public Vector3 vA;
        public Vector3 vB;
        public Vector3 vC;
        public Vector3 vD;

        public Vector3 rADir;
        public Vector3 rBDir;
        public Vector3 rCDir;
        public Vector3 rDDir;

        public float rADst;
        public float rBDst;
        public float rCDst;
        public float rDDst;

        public Vector3[] dirs;
        public float[] dsts;

        public IntersectionVectors(Vector3 position, Vector3 forwardVector, Intersection inter) {
            Vector3 backVector = -forwardVector;
            Vector3 leftVector = new Vector3( -forwardVector.z, forwardVector.y, forwardVector.x );
            Vector3 rightVector = new Vector3( forwardVector.z, forwardVector.y, -forwardVector.x );

            RoadWidths roadAWidth = inter.RoadWidth( 0 );
            RoadWidths roadBWidth = inter.RoadWidth( 1 );
            RoadWidths roadCWidth = inter.RoadWidth( 2 );
            RoadWidths roadDWidth = inter.RoadWidth( 3 );

            rADst = Mathf.Max( roadCWidth.leftSide, roadDWidth.rightSide );
            rBDst = Mathf.Max( roadCWidth.rightSide, roadDWidth.leftSide );
            rCDst = Mathf.Max( roadAWidth.rightSide, roadBWidth.leftSide );
            rDDst = Mathf.Max( roadAWidth.leftSide, roadBWidth.rightSide );
            dsts = new float[] { rADst, rBDst, rCDst, rDDst };

            vA = position + ( forwardVector * rADst ) + ( leftVector * rCDst );
            vB = position + ( forwardVector * rADst ) + ( rightVector * rDDst );
            vC = position + ( backVector * rBDst ) + ( leftVector * rCDst );
            vD = position + ( backVector * rBDst ) + ( rightVector * rDDst );

            rA = position + forwardVector * rADst;
            rB = position + -forwardVector * rBDst;
            rC = position + leftVector * rCDst;
            rD = position + rightVector * rDDst;
            rs = new Vector3[] { rA, rB, rC, rD };


            Vector3 tmp = ( vB - vA ).normalized;
            rADir = new Vector3( -tmp.z, tmp.y, tmp.x );

            tmp = ( vC - vD ).normalized;
            rBDir = new Vector3( -tmp.z, tmp.y, tmp.x );

            tmp = ( vA - vC ).normalized;
            rCDir = new Vector3( -tmp.z, tmp.y, tmp.x );

            tmp = ( vD - vB ).normalized;
            rDDir = new Vector3( -tmp.z, tmp.y, tmp.x );

            dirs = new Vector3[] { rADir, rBDir, rCDir, rDDir };
        }
    }

}
