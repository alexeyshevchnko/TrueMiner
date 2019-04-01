using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace trasharia {
    public class Random : IRandom {
        private System.Random random;
        public void Init(int seed) {
            random = new System.Random(seed);
        }

        public int Range(int min, int max) {
            if (random == null) Init(1);

            return random.Next(min, max);
        }

        public float Range(float min, float max) {
            if (random == null) Init(1);

            var randomNumber = ((float)random.NextDouble());
            return min + (max - min) * randomNumber;
        }
        
    }
}

