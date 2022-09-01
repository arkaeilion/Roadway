using UnityEngine;

using Roadway.Roads;
using Roadway.Junctions;

namespace Roadway.AI {

    [System.Serializable]
    public class Location {

        [SerializeField]
        public RoadSection roadSection;
        [SerializeField]
        public Lane lane;
        [SerializeField]
        public int pointIndex;

        public bool IsSet {
            get { return roadSection != null && lane != null && pointIndex != -1; }
        }

        public Vector3 Point {
            get { return lane.points[ pointIndex ]; }
        }

        public Location(RoadSection roadSection, Lane lane, int pointIndex) {
            this.roadSection = roadSection;
            this.lane = lane;
            this.pointIndex = pointIndex;
        }

        public Junction GetJunction() {
            return (Junction)roadSection.SnapOfHandle( lane.flowDirection ? 0 : roadSection.Path.NumPoints - 1 ).anchorObject;
        }
        
    }

}
