using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapGenerator
{

    [ExecuteInEditMode]
    public class TestManhattanChunks : MonoBehaviour
    {

        [SerializeField]
        [Range(1, 1000)]
        int chunkWidth = 10;
        int old_chunkWidth;
        List<GameObject> cubes = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            CreateCubes();
            
        }

        private void OnDrawGizmos()
        {
            if(old_chunkWidth != chunkWidth)
                CreateCubes();
            
        }

        private void CreateCubes()
        {
            old_chunkWidth = chunkWidth;

            for (int i = cubes.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(cubes[i]);
            }
            cubes.Clear();

            for (int x = -chunkWidth; x < chunkWidth; x++)
                for (int z = -chunkWidth; z < chunkWidth; z++)
                {
                    if (Math.Abs(x) + Math.Abs(z) < chunkWidth)
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.position = new Vector3(x, 0, z);
                        cubes.Add(go);
                    }
                }
        }
    }

}