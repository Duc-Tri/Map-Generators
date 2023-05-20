
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

// Generate a terrain (island), with water and trees, all based on Mathf.PerlinNoise
namespace MapGenerator
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Map : MonoBehaviour
    {
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        [SerializeField]
        private GameObject[] treePrefab;

        [SerializeField]
        [Range(0.001f, 0.5f)]
        public float treeNoiseScale = 0.5f;
        float old_treeNoiseScale = float.MinValue;

        [SerializeField]
        [Range(0f, 1f)]
        public float treeDensity = 0.5f;
        float old_treeDensity = float.MinValue;

        [SerializeField]
        bool showGizmos = true;

        Cell[,] grid;
        float[,] noiseMap;

        [SerializeField]
        [Range(5, 500)]
        int size = 100;
        int old_size = int.MinValue;

        [SerializeField]
        [Range(0.001f, 0.2f)]
        private float scale = 0.04f;
        private float old_scale = float.MaxValue;

        private const float WATER_NOISE_LEVEL = 0.4f;

        [SerializeField]
        [Range(1f, 150f)]
        private float heightScale = 20f;
        private float old_heightScale = float.MaxValue;

        float[,] falloffMap;

        [SerializeField]
        Material terrainMaterial;

        Color RandomGreen => new Color(Random.value * 0.2f, 0.6f - Random.value * 0.1f, Random.value * 0.2f);

        Color RandomBlue => new Color(Random.value * 0.1f, Random.value * 0.1f, 1f - Random.value * 0.1f);


        // Start is called before the first frame update
        void Start()
        {
            TreePooler.SetTreePrefabs(treePrefab.ToList<GameObject>());
            InitMesh();
            RedrawAll();
        }

        private void InitMesh()
        {
            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshFilter = gameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }

        private void RedrawAll(bool recreateArrays = false)
        {
            // main noise map ---------------------------------
            GenerateNoisemap(recreateArrays);
            GenerateGrid(recreateArrays);

            // mesh -------------------------------------------
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            ComputetTerrainData(vertices, triangles, uvs);
            ComputeEdgeData(vertices, triangles, uvs);
            DrawCompletedMesh(vertices, triangles, uvs);
            DrawTexture();

            // trees -----------------------------------------
            TreePooler.SaveAllTrees(this);
            GenerateTrees();
        }

        private void GenerateTrees()
        {
            old_treeDensity = treeDensity;
            old_treeNoiseScale = treeNoiseScale;

            float xOffset = Random.Range(-1000f, 10000f);
            float yOffset = Random.Range(-1000f, 10000f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Cell cell = grid[x, y];

                    // NOISE VALUE ==================
                    float noiseVal = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);

                    if (!cell.isWater)
                    {
                        float ran = Random.Range(0f, treeDensity);

                        if (noiseVal < ran) TreePooler.GetTree(this, x, y);
                    }
                }

        }

        private void DrawCompletedMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
        }

        void DrawTexture()
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] colorMap = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Cell c = grid[x, y];

                    colorMap[y * size + x] = c.isWater ? RandomBlue : RandomGreen;
                    //Color.blue : Color.green;          
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.SetPixels(colorMap);
            texture.Apply();

            meshRenderer.material = terrainMaterial;
            meshRenderer.material.mainTexture = texture;
        }


        void ComputetTerrainData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            if (size < 3) return;

            int height = 0;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if (!grid[x, y].isWater)
                    {
                        //height = grid[x, y].isWater ? -1 : 0;

                        Vector3 vertA = new Vector3(x - .5f, height, y + .5f);
                        Vector3 vertB = new Vector3(x + .5f, height, y + .5f);
                        Vector3 vertC = new Vector3(x - .5f, height, y - .5f);
                        Vector3 vertD = new Vector3(x + .5f, height, y - .5f);

                        Vector3[] v = { vertA, vertB, vertC, vertB, vertD, vertC };


                        Vector2 uvA = new Vector2(x / (float)size, y / (float)size);
                        Vector2 uvB = new Vector2((x + 1) / (float)size, y / (float)size);
                        Vector2 uvC = new Vector2(x / (float)size, (y + 1) / (float)size);
                        Vector2 uvD = new Vector2((x + 1) / (float)size, (y + 1) / (float)size);

                        Vector2[] uv = { uvA, uvB, uvC, uvB, uvD, uvC };

                        for (int k = 0; k < 6; k++)
                        {
                            vertices.Add(v[k]);
                            triangles.Add(triangles.Count);
                            uvs.Add(uv[k]);
                        }
                    }

        }

        void ComputeEdgeData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Cell c = grid[x, y];
                    if (!c.isWater)
                    {
                        Vector3 vert_top_left = new Vector3(x - 0.5f, 0, y + 0.5f);
                        Vector3 vert_top_right = new Vector3(x + 0.5f, 0, y + 0.5f);
                        Vector3 vert_bottom_left = new Vector3(x - 0.5f, 0, y - 0.5f);
                        Vector3 vert_bottom_right = new Vector3(x + 0.5f, 0, y - 0.5f);

                        // left edge
                        if (x > 0 && grid[x - 1, y].isWater) AddWaterEdgeData(vert_top_left, vert_bottom_left, vertices, triangles, uvs);

                        // right edge
                        if (x < size - 1 && grid[x + 1, y].isWater) AddWaterEdgeData(vert_bottom_right, vert_top_right, vertices, triangles, uvs);

                        // bottom edge
                        if (y > 0 && grid[x, y - 1].isWater) AddWaterEdgeData(vert_bottom_left, vert_bottom_right, vertices, triangles, uvs);

                        // top edge
                        if (y < size - 1 && grid[x, y + 1].isWater) AddWaterEdgeData(vert_top_right, vert_top_left, vertices, triangles, uvs);
                    }
                }

        }

        private void AddWaterEdgeData(Vector3 a, Vector3 b, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            // vertices a & b are given, we compute c & d
            // a-b
            // |/|
            // c-d

            // a & b are at y=0, c & d will be at y=-1
            Vector3 c = a; // new struct, same values
            Vector3 d = b; // new struct, same values
            c.y = -10;
            d.y = -10;

            Vector3[] v = { a, b, c, b, d, c };

            // TODO : decide for the real UVs
            Vector2 uvDummy = Vector2.zero;
            Vector2[] uv = { uvDummy, uvDummy, uvDummy, uvDummy, uvDummy, uvDummy };

            for (int k = 0; k < 6; k++)
            {
                vertices.Add(v[k]);
                triangles.Add(triangles.Count);
                uvs.Add(uv[k]);
            }
        }

        private void GenerateNoisemap(bool force = false)
        {
            if (size < 3) return;

            if (force || noiseMap == null)
            {
                falloffMap = new float[size, size];
                noiseMap = new float[size, size];
            }

            old_scale = scale;

            float xOffset = Random.Range(-1000f, 10000f);
            float yOffset = Random.Range(-1000f, 10000f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // FALLOFF calculation ==========
                    float xv = x / (float)size * 2f - 1f;
                    float yv = y / (float)size * 2f - 1f;
                    float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));

                    falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));

                    // NOISE VALUE ==================
                    float noiseVal = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                    noiseMap[x, y] = noiseVal - falloffMap[x, y];
                }

        }

        private void GenerateGrid(bool force = false)
        {
            if (size < 3) return;

            if (force || grid == null)
                grid = new Cell[size, size];

            old_size = size;
            float noiseval;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Cell c = new Cell();
                    noiseval = noiseMap[x, y];
                    c.isWater = noiseval < WATER_NOISE_LEVEL;
                    c.noiseValue = noiseval;
                    float colorVal = (noiseval - WATER_NOISE_LEVEL) * (1f / (1f - WATER_NOISE_LEVEL));
                    c.color = new Color(colorVal, colorVal, colorVal, 1);
                    grid[x, y] = c;
                }

        }



        private void Update()
        {
            if (old_scale != scale || old_size != size)
            {
                RedrawAll(true);
            }

            if (old_treeDensity != treeDensity || old_treeNoiseScale != treeNoiseScale)
            {
                TreePooler.SaveAllTrees(this);
                GenerateTrees();
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !showGizmos) return;

            DrawGizmos(-heightScale - 1);
        }

        private void DrawGizmos(float yOffset = 0)
        {
            Vector3 pos = new Vector3();
            for (int yGrid = 0; yGrid < size; yGrid++)
                for (int xGrid = 0; xGrid < size; xGrid++)
                {
                    pos.x = xGrid;
                    pos.z = yGrid;

                    // DISPLAY FALLOFF ===================================
                    /*
                    float f = falloffMap[x, y];
                    Gizmos.color = new Color(0.5f, f, f);
                    pos.y = -100f + f * heightScale*0.5f;
                    Gizmos.DrawSphere(pos, 1f);
                    */

                    // DISPLAY MAP =======================================
                    Cell c = grid[xGrid, yGrid];

                    if (c.isWater)
                    {
                        Gizmos.color = Color.magenta;
                        pos.y = 0;
                    }
                    else
                    {
                        Gizmos.color = c.color;
                        pos.y = (c.noiseValue - WATER_NOISE_LEVEL) * heightScale;
                    }

                    pos.y += yOffset;
                    Gizmos.DrawCube(pos, Vector3.one);
                }

            //test vector3 struct
            Vector3 v01 = Vector3.up * 10f;
            Vector3 v02 = v01;
            v02.y = 1.5f;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(v01, 2);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(v02, 2);
        }
    }

}