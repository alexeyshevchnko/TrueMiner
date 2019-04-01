using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace trasharia {
    public interface IRandom {
        void Init(int seed);
        float Range(float min, float max);
        int Range(int min, int max);
    }
}