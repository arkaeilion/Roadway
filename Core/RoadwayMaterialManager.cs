using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;

namespace Roadway {

    [System.Serializable]
    public class RoadwayMaterialManager {
        
        [SerializeField]
        private RoadwayManager rm;

        [SerializeField]
        private Dictionary<(int, bool, bool, bool, bool), Material> laneMaterials;

        [SerializeField]
        private Dictionary<(int, int, bool, bool, bool, bool), Material> laneTransitionMaterials;

        public RoadwayMaterialManager(RoadwayManager roadwayManager) {
            rm = roadwayManager;
            laneMaterials = new Dictionary<(int, bool, bool, bool, bool), Material>();
        }

        public Material GetLaneMaterial(int numLanes, bool isDashed, bool isSplit, bool isLeft, bool uvsXValReverse) {
            Material mat;

            if ( laneMaterials == null )
                laneMaterials = new Dictionary<(int, bool, bool, bool, bool), Material>();

            if ( laneMaterials.TryGetValue((numLanes, isDashed, isSplit, isLeft, uvsXValReverse), out mat) )
                return mat;

            // didn't get, make new
            mat = new Material( rm.RoadwaySettings.RoadLaneMat );

            Color outer = isSplit ? rm.RoadwaySettings.RoadOuterLineColor : rm.RoadwaySettings.RoadInnerLineColor;
            Color outerLeft = isLeft ? rm.RoadwaySettings.RoadOuterLineColor : outer;
            Color outerRight = !isLeft ? rm.RoadwaySettings.RoadOuterLineColor : outer;

            if ( uvsXValReverse ) {
                Color tmp = outerLeft;
                outerLeft = outerRight;
                outerRight = tmp;
            }

            mat.SetColor( "_Base_Color", rm.RoadwaySettings.RoadBaseColor );
            mat.SetColor( "_Outer_Lines_Left_Color", outerLeft );
            mat.SetColor( "_Outer_Lines_Right_Color", outerRight );
            mat.SetColor( "_Dashed_Lines_Color", rm.RoadwaySettings.RoadDashLineColor );
            mat.SetFloat( "_Line_Width", rm.RoadwaySettings.RoadLineWidth );
            mat.SetFloat( "_Outer_Lines_Distance", rm.RoadwaySettings.RoadOuterLineDistance );
            mat.SetFloat( "_Dashes_Ratio", isDashed ? rm.RoadwaySettings.RoadDashRatio : 1 );
            mat.SetFloat( "_Number_Lanes", numLanes );
            // mat.SetFloat( "_Tiling", textureRepeat );

            laneMaterials.Add( (numLanes, isDashed, isSplit, isLeft, uvsXValReverse), mat );
            // Debug.Log("making new mat");
            return mat;
        }

        public Material GetLaneTransitionMaterial(int numLanesA, int numLanesB, bool isDashed, bool isSplit, bool isLeft, bool uvsXValReverse) {
            Material mat;

            if ( laneTransitionMaterials == null )
                laneTransitionMaterials = new Dictionary<(int, int, bool, bool, bool, bool), Material>();

            if ( laneTransitionMaterials.TryGetValue((numLanesA, numLanesB, isDashed, isSplit, isLeft, uvsXValReverse), out mat) )
                return mat;

            // didn't get, make new
            mat = new Material( rm.RoadwaySettings.RoadLaneTransitionMat );

            Color outer = isSplit ? rm.RoadwaySettings.RoadOuterLineColor : rm.RoadwaySettings.RoadInnerLineColor;
            Color outerLeft = isLeft ? rm.RoadwaySettings.RoadOuterLineColor : outer;
            Color outerRight = !isLeft ? rm.RoadwaySettings.RoadOuterLineColor : outer;

            if ( uvsXValReverse ) {
                Color tmp = outerLeft;
                outerLeft = outerRight;
                outerRight = tmp;
            }

            mat.SetColor( "_Base_Color", rm.RoadwaySettings.RoadBaseColor );
            mat.SetColor( "_Outer_Lines_Left_Color", outerLeft );
            mat.SetColor( "_Outer_Lines_Right_Color", outerRight );
            mat.SetColor( "_Dashed_Lines_Color", rm.RoadwaySettings.RoadDashLineColor );
            mat.SetFloat( "_Line_Width", rm.RoadwaySettings.RoadLineWidth );
            mat.SetFloat( "_Outer_Lines_Distance", rm.RoadwaySettings.RoadOuterLineDistance );
            mat.SetFloat( "_Dashes_Ratio", isDashed ? rm.RoadwaySettings.RoadDashRatio : 1 );
            mat.SetFloat( "_Number_Lanes_A", numLanesA );
            mat.SetFloat( "_Number_Lanes_B", numLanesB );
            // mat.SetFloat( "_Tiling", textureRepeat );

            laneTransitionMaterials.Add( (numLanesA, numLanesB, isDashed, isSplit, isLeft, uvsXValReverse), mat );
            // Debug.Log("making new mat");
            return mat;
        }
        
        public Material GetCenterMaterial() {
            return rm.RoadwaySettings.RoadCenterMat;
        }
        
        public Material GetIntersectionMaterial() {
            return rm.RoadwaySettings.IntersectionMat;
        }

    }

}
