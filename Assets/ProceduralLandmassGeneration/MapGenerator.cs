using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    // THREADED
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap };
        public DrawMode drawMode;

        public Noise.NormalizeMode normalizeMode;

        [Range(0, 6)]
        public int editorPreviewLOD; // 1=no modification
        // Level Of Detail
        public const int mapChunkSize = 239; // +2 factor 2, 4, 6, 8, 10, 12 nicely

        public float noiseScale;

        public int octaves;

        [Range(0f, 1f)]
        public float persistance;
        public float lacunarity;

        public int seed;
        public Vector2 offset;

        public bool useFallof;

        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;

        public bool autoUpdate;
        public TerrainType[] regions;
        public float[,] falloffMap;

        Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        private void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFallofMap(mapChunkSize);
        }

        public void RequestMapData(Vector2 center, Action<MapData> callback)
        {
            // START MapData THREAD
            ThreadStart threadStart = delegate { MapDataThread(center, callback); };

            new Thread(threadStart).Start();
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            // START MapData THREAD
            ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };

            new Thread(threadStart).Start();
        }

        void MapDataThread(Vector2 center, Action<MapData> callback)
        {
            // get MapData
            MapData mapData = GenerateMapData(center);

            // add MapData + callback to queue
            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
            lock (meshDataThreadInfoQueue)
            {
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            if (mapDataThreadInfoQueue.Count > 0) // CALLBACK WITH MapData
                for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }

            if (meshDataThreadInfoQueue.Count > 0) // CALLBACK WITH MapData
                for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
        }

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData(Vector2.zero);
            MapDisplay display = FindAnyObjectByType<MapDisplay>();

            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            else if (drawMode == DrawMode.ColorMap)
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.Mesh)
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD),
                    TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.FalloffMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFallofMap(mapChunkSize)));
        }

        private MapData GenerateMapData(Vector2 center)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);

            Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    if (useFallof)
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);

                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight >= regions[i].height)
                            colorMap[y * mapChunkSize + x] = regions[i].color;
                        else
                            break;
                    }
                }
            }

            return new MapData(noiseMap, colorMap);
        }

        private void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;

            if (octaves < 0) octaves = 0;

            falloffMap = FalloffGenerator.GenerateFallofMap(mapChunkSize);
        }

        struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }


        }

    } // end of  class MapGenerator ===============================================================

    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }

    public struct MapData
    {
        public float[,] heightMap;
        public Color[] colorMap;

        public MapData(float[,] heightMap, Color[] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }

}