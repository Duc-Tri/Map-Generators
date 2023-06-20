using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    // THREADED
    public class EndlessTerrain : MonoBehaviour
    {
        const float scale = 10f;
        const float viewerMoveThresholdForChunkUpdate = 25f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

        public LODInfo[] detailLevels;
        public static float maxViewDst = -1;

        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        Vector2 viewerPositionOld = Vector2.negativeInfinity;
        static MapGenerator mapGenerator;
        int chunkSize;
        int chunkVisibleInViewDst;

        Dictionary<Vector2, TerrainChunk> terrainChunkDic = new Dictionary<Vector2, TerrainChunk>();
        static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

            if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                UpdateVisibleChunks();
            }
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
                    }
                    else
                    {
                        terrainChunkDic.Add(viewChunkCoord, new TerrainChunk(viewChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
            LODInfo[] detailLevels;
            LODMesh[] lodMeshes;
            MapData mapData;
            bool mapDataReceived;
            int previousLODIndex = -1;

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
            {
                this.detailLevels = detailLevels;
                position = coord * size;
                bounds = new Bounds(position, Vector2.one * size);
                Vector3 posV3 = new Vector3(position.x, 0, position.y);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshRenderer.sharedMaterial = material;
                meshObject.transform.position = posV3 * scale;
                meshObject.transform.localScale = Vector3.one * scale;

                meshObject.transform.parent = parent;
                SetVisible(false);

                lodMeshes = new LODMesh[detailLevels.Length];
                for (int i = 0; i < detailLevels.Length; i++)
                    lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);

                mapGenerator.RequestMapData(position, OnMapDatReceived);
            }

            void OnMapDatReceived(MapData mapData)
            {
                this.mapData = mapData;
                mapDataReceived = true;


                Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
                meshRenderer.material.mainTexture = texture;

                UpdateTerrainChunk();
            }

            void OnMeshDataReceived(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                if (mapDataReceived)
                {
                    float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                    bool visible = viewDstFromNearestEdge <= maxViewDst;
                    if (visible)
                    {
                        int lodIndex = 0;

                        for (int i = 0; i < detailLevels.Length - 1; i++)
                            if (viewDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                                lodIndex = i + 1;
                            else
                                break;

                        if (lodIndex != previousLODIndex)
                        {
                            LODMesh lodMesh = lodMeshes[lodIndex];
                            if (lodMesh.hasMesh)
                            {
                                previousLODIndex = lodIndex;
                                meshFilter.mesh = lodMesh.mesh;
                            }
                            else if (!lodMesh.hasRequestedMesh) // NOT REQUESTED YET !
                            {
                                lodMesh.RequestMesh(mapData);
                            }
                        }

                        terrainChunksVisibleLastUpdate.Add(this);
                    }
                    SetVisible(visible);
                }
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

        public class LODMesh
        {
            public Mesh mesh;
            public bool hasRequestedMesh;
            public bool hasMesh;
            int lod;
            Action updateCallback;

            public LODMesh(int lod, Action updateCallback)
            {
                this.lod = lod;
                this.updateCallback = updateCallback;
            }

            void OnMeshDataReceived(MeshData meshData)
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;
                updateCallback();
            }

            public void RequestMesh(MapData mapData)
            {
                hasRequestedMesh = true;
                mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
            }
        }

        [Serializable]
        public struct LODInfo
        {
            public int lod;
            public float visibleDstThreshold; // go next LOD when above
        }

    }
}