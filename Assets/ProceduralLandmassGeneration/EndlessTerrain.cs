using System.Collections.Generic;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    // THREADED
    public class EndlessTerrain : MonoBehaviour
    {
        public const float maxViewDst = 450;
        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        static MapGenerator mapGenerator;
        int chunkSize;
        int chunkVisibleInViewDst;

        Dictionary<Vector2, TerrainChunk> terrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
        List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

            //MapGenerator.reqestMapdata
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
        }

        void UpdateVisibleChunks()
        {
            for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
                terrainChunksVisibleLastUpdate[i].SetVisible(false);

            terrainChunksVisibleLastUpdate.Clear();

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
            {
                for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++)
                {
                    Vector2 viewChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (terrainChunkDic.ContainsKey(viewChunkCoord))
                    {
                        terrainChunkDic[viewChunkCoord].UpdateTerrainChunk();
                        if (terrainChunkDic[viewChunkCoord].IsVisible())
                            terrainChunksVisibleLastUpdate.Add(terrainChunkDic[viewChunkCoord]);
                    }
                    else
                    {
                        terrainChunkDic.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize, transform, mapMaterial));
                    }
                }
            }
        }

        private class TerrainChunk
        {
            Vector2 position;
            GameObject meshObject;
            Bounds bounds;
            MeshRenderer meshRenderer;
            MeshFilter meshFilter;

            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                position = coord * size;
                bounds = new Bounds(position, Vector2.one * size);
                Vector3 posV3 = new Vector3(position.x, 0, position.y);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshRenderer.sharedMaterial = material;
                meshObject.transform.position = posV3;

                meshObject.transform.parent = parent;
                SetVisible(false);

                mapGenerator.RequestMapData(OnMapDatReceived);
            }

            void OnMapDatReceived(MapData mapData)
            {
                mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            }

            void OnMeshDataReceived(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewDstFromNearestEdge <= maxViewDst;
                SetVisible(visible);
            }

            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

            public bool IsVisible()
            {
                return meshObject.activeSelf;
            }
        }

    }
}