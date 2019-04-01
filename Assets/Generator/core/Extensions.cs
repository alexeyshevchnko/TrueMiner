using System;
using UnityEngine;
using System.Collections;

namespace trasharia {
    public static partial class Extensions {
        public static void TryCall<T0, T1>(this Action<T0, T1> target, T0 param0, T1 param1) {
            var local = target;
            if (local != null) {
                local(param0, param1);
            }
        }

        public static void TryCall<T>(this Action<T> target, T param) {
            var local = target;
            if (local != null) {
                local(param);
            }
        }

        public static void TryCall(this Action target) {
            var local = target;
            if (local != null) {
                local();
            }
        }

        public static GameObject SetInParent(this GameObject go, Transform parent) {
            var transform = go.transform;
            transform.position = parent.position;
            transform.rotation = parent.rotation;
            return go;
        }
    }
}