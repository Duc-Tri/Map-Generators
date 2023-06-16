using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    public class Noise
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            if (scale <= 0) scale = 0.001f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sampleX = x / scale;
                    float sampleY = y / scale;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseMap[x, y] = perlinValue;
                }
            }

            return noiseMap;
        }
    }
}