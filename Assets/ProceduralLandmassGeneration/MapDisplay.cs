using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using UnityEngine;

namespace ProceduralLandmassGeneration
{

    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRender;

        public void DrawTexture(Texture2D texture)
        {
            textureRender.sharedMaterial.mainTexture = texture;
            textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }
    }
}