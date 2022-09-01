using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway {

    [CreateAssetMenu( fileName = "Roadway Settings", menuName = "Roadway/Roadway Settings" )]
    public class RoadwaySettings : ScriptableObject {
        public Material RoadCenterMat;
        public Material RoadLaneMat;
        public Material RoadLaneTransitionMat;

        public Material IntersectionMat;

        public RoadDrivingSide RoadDrivingSide = RoadDrivingSide.Right;
        public RoadUnitOptions RoadUnitOptions = RoadUnitOptions.MilesPerHour;

        public float MaxRoadCenterWidth = 50;
        public float RoadLift = 0.01f;
        public float LaneWidth = 4f;
        public float Tiling = 3f;
        public Color RoadBaseColor = new Color( .42f, .41f, .42f );
        public Color RoadOuterLineColor = new Color( .9f, .9f, .9f );
        public Color RoadInnerLineColor = new Color( .77f, .5f, .12f );
        public Color RoadDashLineColor = new Color( .9f, .9f, .9f );
        public float RoadLineWidth = .03f;
        public float RoadOuterLineDistance = 0;
        public float RoadDashRatio = .5f;

        public float RoadCenterOuterLineDistance = .9f;


        public int MaxLanesPerRoadSide = 6;
    }

}
