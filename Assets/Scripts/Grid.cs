using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    Cell[,] grid;
    float[,] noiseMap;

    [SerializeField]
    [Range(3, 500)]
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

    // Start is called before the first frame update
    void Start()
    {
        GenerateNoisemap();
        GenerateGrid();
        DrawTerrainMesh();
    }

    void DrawTerrainMesh(bool recreateComponents = true)
    {
        if (size < 3) return;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell c = grid[x, y];
                if (!c.isWater)
                {
                    Vector3 v1 = new Vector3(x - .5f, 0, y + .5f);
                    Vector3 v2 = new Vector3(x + .5f, 0, y + .5f);
                    Vector3 v3 = new Vector3(x - .5f, 0, y - .5f);
                    Vector3 v4 = new Vector3(x + .5f, 0, y - .5f);

                    Vector3[] v = new Vector3[] { v1, v2, v3, v2, v4, v3 };


                    Vector2 uv1 = new Vector2(x / (float)size, y / (float)size);
                    Vector2 uv2 = new Vector2((x + 1) / (float)size, y / (float)size);
                    Vector2 uv3 = new Vector2(x / (float)size, (y + 1) / (float)size);
                    Vector2 uv4 = new Vector2((x + 1) / (float)size, (y + 1) / (float)size);


                    Vector2[] uv = new Vector2[] { uv1, uv2, uv3, uv2, uv4, uv3 };

                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[k]);
                    }

                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        
        if (recreateComponents)
             meshFilter = gameObject.AddComponent<MeshFilter>();

        meshFilter.mesh = mesh;

        if (recreateComponents)
             meshRenderer = gameObject.AddComponent<MeshRenderer>();

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
        {
            for (int x = 0; x < size; x++)
            {
                // FALLOFF calculation =============
                float xv = x / (float)size * 2f - 1f;
                float yv = y / (float)size * 2f - 1f;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));

                falloffMap[x, y] = Mathf.Pow(v, 3f) /
                    (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));

                // NOISEVALUE =============================
                float noiseVal = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseVal - falloffMap[x, y];
            }
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
        {
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
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (old_scale != scale || old_size != size)
        {
            GenerateNoisemap(true);
            GenerateGrid(true);
            DrawTerrainMesh(false);
        }

        DrawGizmos(-heightScale - 1);
    }

    private void DrawGizmos(float yOffset = 0)
    {
        Vector3 pos = new Vector3();
        for (int yGrid = 0; yGrid < size; yGrid++)
        {
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
                    Gizmos.color = Color.blue;
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
        }

    }
}

