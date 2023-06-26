using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralLandmassGeneration
{
    [CreateAssetMenu()]
    public class TextureData : UpdatableData
    {
        float savedMinHeight;
        float savedMaxHeight;

        public void ApplyToMaterial(Material mat)
        {
            UpdateMeshHeights(mat, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            savedMaxHeight = maxHeight;
            savedMinHeight = minHeight;
            //material.SetFloat("minHeight", minHeight);
            //material.SetFloat("maxHeight", maxHeight);
        }


    }
}
