using System;

namespace Roadway {

    public enum RoadDrivingSide { Right = 0, Left = 1 };
    public enum RoadUnitOptions { MilesPerHour = 0, KilometersPerHour = 1 }; // MetersPerSecond, Knots 
    public enum RoadLines { Dashed = 0, Solid = 1 };

    public enum RoadSectionNameOperation { None = 0, Replace = 1, Prefix = 2, Suffix = 3 };
    public enum RoadSectionSpeedOperation { None = 0, Replace = 1 };
    public enum RoadTravelDirections { Bothways = 0, Oneway = 1  };

    public enum ControlMode { Aligned = 0, Free = 1, Automatic = 2 };

    public enum RoadwayQueueActions { None = 0, CreateRoad = 1, CreateRoadSection = 2, CreateIntersection = 3 };

    public enum RoadSide { Left = 0, LeftInner = 1, LeftOuter = 2, 
                            Right = 3, RightInner = 4, RightOuter = 5 }

    public static class RoadwayEnums {

        public static readonly string[] RoadDrivingSideNames = { "Right", "Left" };
        public static readonly string[] RoadUnitOptionsNames = { "Miles Per Hour", "Kilometers Per Hour" };
        public static readonly string[] RoadLinesNames = { "Dashed", "Solid" };

        public static readonly string[] RoadSectionNameOperationNames = { "None - Same as Road", "Replace", "Prefix", "Suffix" };
        public static readonly string[] RoadSectionSpeedOperationNames = { "None - Same as Road", "Replace" };
        public static readonly string[] RoadTravelDirectionsNames = { "Bothways", "Oneway" };

        public static readonly string[] ControlModeNames = { "Aligned", "Free", "Automatic" };

        public static readonly string[] RoadwayQueueActionsNames = { "None", "CreateRoad", "CreateRoadSection", "CreateIntersection" };

        public static int IndexOf<T>(T value) {
            return Array.IndexOf( Enum.GetValues( value.GetType() ), value );
        }

    }

}
