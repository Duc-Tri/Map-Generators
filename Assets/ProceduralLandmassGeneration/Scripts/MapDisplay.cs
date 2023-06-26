using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRender;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public void DrawTexture(Texture2D texture)
        {
            textureRender.sharedMaterial.mainTexture = texture;
            textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }

        public void DrawMesh(MeshData meshData)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            //meshRenderer.sharedMaterial.mainTexture = texture;
            meshFilter.transform.localScale = Vector3.one * FindAnyObjectByType<MapGenerator>().terrainData.uniformScale;
        }

    }

}