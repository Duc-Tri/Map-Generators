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

        public TerrainData terrainData;
        public NoiseData noiseData;
        public TextureData textureData;

        public Material terrainMaterial;

        [Range(0, 6)]
        public int editorPreviewLOD; // 1=no modification

        public bool autoUpdate;

        public float[,] falloffMap;

        Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        public int mapChunkSize
        {
            get
            {
                if (terrainData.useFlatShading)
                    return 95;
                else
                    return 239; // +2 factor 2, 4, 6, 8, 10, 12 nicely
            }
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

            // add MapData + callbac k to queue
            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);
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
            else if (drawMode == DrawMode.Mesh)
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
            else if (drawMode == DrawMode.FalloffMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFallofMap(mapChunkSize)));
        }

        private MapData GenerateMapData(Vector2 center)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

            if (terrainData.useFallof)
            {
                if (falloffMap == null)
                    falloffMap = FalloffGenerator.GenerateFallofMap(mapChunkSize + 2);

                for (int y = 0; y < mapChunkSize + 2; y++)
                {
                    for (int x = 0; x < mapChunkSize + 2; x++)
                    {
                        if (terrainData.useFallof)
                            noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }
                }
            }

            return new MapData(noiseMap);
        }

        void OnValuesUpdated()
        {
            if (!Application.isPlaying)
                DrawMapInEditor();
        }

        void OnTextureValuesUpdated()
        {
            textureData.ApplyToMaterial(terrainMaterial);
        }

        private void OnValidate()
        {
            if (terrainData != null)
            {
                terrainData.OnValuesUpdated -= OnValuesUpdated;
                terrainData.OnValuesUpdated += OnValuesUpdated;
            }

            if (noiseData != null)
            {
                noiseData.OnValuesUpdated -= OnValuesUpdated;
                noiseData.OnValuesUpdated += OnValuesUpdated;
            }

            if (textureData != null)
            {
                textureData.OnValuesUpdated -= OnTextureValuesUpdated;
                textureData.OnValuesUpdated += OnTextureValuesUpdated;
            }
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


    public struct MapData
    {
        public float[,] heightMap;
        //public Color[] colorMap;

        public MapData(float[,] heightMap)
        {
            this.heightMap = heightMap;
            //this.colorMap = colorMap;
        }
    }

}