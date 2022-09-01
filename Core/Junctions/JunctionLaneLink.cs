using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Roads;
using Roadway.Spline;

namespace Roadway.Junctions {

    [System.Serializable]
    public class JunctionLaneLink {

        [SerializeField]
        public Lane origin;
        [SerializeField]
        public bool originStart; // true = index 0 of points
        [SerializeField]
        public int originSnapIndex; // to refer back to the RoadSection

        [SerializeField]
        public Lane destination;
        [SerializeField]
        public bool destinationStart; // true = index 0 of points
        [SerializeField]
        public int destinationSnapIndex; // to refer back to the RoadSection


        // for gui
        readonly float[] arrowsToMake = { .3f, .7f };

        [SerializeField]
        public ArrowVector[] arrows;

        public JunctionLaneLink(Lane origin, bool originStart, int originSnapIndex, Lane destination, bool destinationStart, int destinationSnapIndex) {
            this.origin = origin;
            this.originStart = originStart;
            this.originSnapIndex = originSnapIndex;
            this.destination = destination;
            this.destinationStart = destinationStart;
            this.destinationSnapIndex = destinationSnapIndex;

            arrows = new ArrowVector[ arrowsToMake.Length ];
            for ( int i = 0; i < arrowsToMake.Length; i++ ) {
                arrows[ i ] = new ArrowVector( OriginPoint, DestinationPoint, arrowsToMake[ i ], false, .3f );
            }
        }

        public Vector3 OriginPoint {
            get { return origin.points[ originStart ? 0 : origin.points.Count - 1 ]; }
        }

        public Vector3 DestinationPoint {
            get { return destination.points[ destinationStart ? 0 : destination.points.Count - 1 ]; }
        }

        public Color color {
            get { return origin.color; }
        }

        public bool Match(int originSnapIndex, int destinationSnapIndex) {
            bool originMatch = this.originSnapIndex == originSnapIndex;
            bool destinationMatch = this.destinationSnapIndex == destinationSnapIndex;
            return originMatch && destinationMatch;
        }

    }

}
