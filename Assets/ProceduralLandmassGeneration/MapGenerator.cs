using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColorMap, Mesh };
        public DrawMode drawMode;

        [Range(0, 6)]
        public int levelOfDetail; // 1=no modification
        // Level Of Detail
        const int mapChunkSize = 241; // factor 2,4,6,8,10,12 nicely

        public float noiseScale;

        public int octaves;

        [Range(0f, 1f)]
        public float persistance;
        public float lacunarity;

        public int seed;
        public Vector2 offset;
        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;

        public bool autoUpdate;
        public TerrainType[] regions;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

            Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorMap[y * mapChunkSize + x] = regions[i].color;
                            break;
                        }
                    }
                }
            }

            MapDisplay display = FindAnyObjectByType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            else if (drawMode == DrawMode.ColorMap)
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.Mesh)
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                    TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }

        private void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;

            if (octaves < 0) octaves = 1;
        }
    }

    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }

}