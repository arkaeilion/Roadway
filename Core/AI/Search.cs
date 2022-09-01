using System;
using System.Collections.Generic;

namespace Roadway.AI {

    [System.Serializable]
    public static class Search {

        public static List<T> AStarSearch<T>(List<T> starts, List<T> goals, Func<T, float> Heuristic, Func<T, T, float> WeightOfEdge, Func<T, List<T>> Neighbors) {
            List<T> openSet = new List<T>();
            Dictionary<T, float> gScore = new Dictionary<T, float>();
            Dictionary<T, float> fScore = new Dictionary<T, float>();
            Dictionary<T, T> cameFrom = new Dictionary<T, T>();

            foreach ( T start in starts ) {
                openSet.Add( start );
                gScore.Add( start, 0 );
                fScore.Add( start, Heuristic( start ) );
            }

            while ( openSet.Count > 0 ) {
                T current = GetLowestFScoreFromOpenSet( openSet, fScore );
                if ( goals.Contains( current ) )
                    return ReconstructPath( current, cameFrom );

                openSet.Remove( current );
                List<T> neighbors = Neighbors( current );
                foreach ( T neighbor in neighbors ) {
                    if ( !gScore.ContainsKey( neighbor ) )
                        gScore.Add( neighbor, float.PositiveInfinity );
                    if ( !fScore.ContainsKey( neighbor ) )
                        fScore.Add( neighbor, Heuristic( neighbor ) );
                }

                foreach ( T neighbor in neighbors ) {
                    float tentativeGScore = gScore[ current ] + WeightOfEdge( current, neighbor );
                    if ( tentativeGScore < gScore[ neighbor ] ) {
                        cameFrom[ neighbor ] = current;
                        gScore[ neighbor ] = tentativeGScore;
                        fScore[ neighbor ] = tentativeGScore + Heuristic( neighbor );
                        if ( !openSet.Contains( neighbor ) )
                            openSet.Add( neighbor );
                    }
                }
            }

            return new List<T>();
        }

        private static T GetLowestFScoreFromOpenSet<T>(List<T> openSet, Dictionary<T, float> fScore) {
            // could use queue
            T lowest = openSet[ 0 ];
            float score = fScore[ lowest ];

            foreach ( T t in openSet ) {
                if ( fScore[ t ] < score ) {
                    lowest = t;
                    score = fScore[ t ];
                }
            }

            return lowest;
        }

        private static List<T> ReconstructPath<T>(T current, Dictionary<T, T> cameFrom) {
            List<T> path = new List<T>() { current };
            while ( cameFrom.ContainsKey( current ) ) {
                current = cameFrom[ current ];
                path.Insert( 0, current );
            }
            return path;
        }
    }

}
