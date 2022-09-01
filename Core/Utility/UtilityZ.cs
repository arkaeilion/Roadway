using UnityEngine;

namespace Roadway {

    public static class UtilityZ {

        public static T SafeDestroy<T>(T obj) where T : Object {
            if ( Application.isEditor )
                Object.DestroyImmediate( obj );
            else
                Object.Destroy( obj );
            return null;
        }

        public static T[] ConcatArray<T>(T[] array1, T[] array2) {
            T[] newArray = new T[ array1.Length + array2.Length ];
            System.Array.Copy( array1, newArray, array1.Length );
            System.Array.Copy( array2, 0, newArray, array1.Length, array2.Length );
            return newArray;
        }

    }

}