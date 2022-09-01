using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Roadway.Spline;

namespace Roadway.Roads {

    [System.Serializable]
    public class Lane : ScriptableObject {

        public static Lane NewLane(bool flowDirection) {
            Lane lane = CreateInstance<Lane>();
            lane.Init( flowDirection );
            return lane;
        }

        [SerializeField]
        public List<Vector3> points;

        [SerializeField]
        public bool flowDirection;

        [SerializeField]
        public ArrowVector[] arrows = new ArrowVector[ 0 ];

        public void Init(bool flowDirection) {
            points = new List<Vector3>();
            this.flowDirection = flowDirection;
        }

        public void FinalizeLane(bool flowDirection) {
            this.flowDirection = flowDirection;
            
            if ( points.Count < 5 ) {
                arrows = new ArrowVector[ 0 ];
                return;
            } else if ( points.Count < 10 ) {
                CreateArrows( new float[] { .5f } );
            } else if ( points.Count < 20 ) {
                CreateArrows( new float[] { .2f, .5f, .8f } );
            } else {
                CreateArrows( new float[] { .1f, .2f, .3f, .4f, .5f, .6f, .7f, .8f, .9f } );
            }
        }

        private void CreateArrows(float[] arrowsToMake) {
            arrows = new ArrowVector[ arrowsToMake.Length ];
            for ( int i = 0; i < arrowsToMake.Length; i++ ) {
                arrows[ i ] = new ArrowVector( points[ Mathf.RoundToInt( points.Count * arrowsToMake[ i ] ) - 1 ],
                                                points[ Mathf.RoundToInt( points.Count * arrowsToMake[ i ] ) + 1 ],
                                                arrowsToMake[ i ], flowDirection, .3f );
            }
        }

        public void Clear() {
            points = new List<Vector3>();
        }

        public void AddPoint(Vector3 newPoint) {
            points.Add( newPoint );
        }


        [SerializeField]
        private bool colorSet = false;
        [SerializeField]
        private Color _color;
        public Color color {
            get {
                if ( !colorSet ) {
                    _color = Random.ColorHSV() - new Color( 0, 0, 0, .3f );
                    colorSet = true;
                }
                return _color;
            }
        }

    }

}
