using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralLandmassGeneration
{
    [CreateAssetMenu()]
    public class NoiseData : UpdatableData
    {
        public Noise.NormalizeMode normalizeMode;

        public float noiseScale;

        public int octaves;

        [Range(0f, 1f)]
        public float persistance;
        public float lacunarity;

        public int seed;
        public Vector2 offset;

        protected override void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;

            if (octaves < 0) octaves = 0;

            base.OnValidate();
        }

    }

}