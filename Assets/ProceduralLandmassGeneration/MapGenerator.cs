using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralLandmassGeneration
{

    public class MapGenerator : MonoBehaviour
    {
        public int mapWidth;
        public int mapHeight;
        public float noiseScale;

        public bool autoUpdate;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

            MapDisplay display=FindAnyObjectByType<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
        }
    }
}