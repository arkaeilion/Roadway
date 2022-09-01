using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roadway {

    [CreateAssetMenu( fileName = "Road Preset", menuName = "Roadway/Road Preset" )]
    public class RoadPreset : ScriptableObject {

        public string presetName = "Road Preset";

        public string roadName = "";
        public int roadSpeed = 60;
        public RoadTravelDirections travelDirection = RoadTravelDirections.Bothways;

        public int numLanesLeft = 1;
        public int numLanesRight = 1;
        public float centerWidth = 0f;
        public bool splitSides = false;
        public RoadLines roadLine = RoadLines.Dashed;

        public bool makeMesh = true;
        public bool makeAI = true;

    }

}
