using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralLandmassGeneration
{
    [CustomEditor(typeof(MapGenerator))]
    public class MatGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            MapGenerator mapGen = (MapGenerator)target;

            // if modifications
            if (DrawDefaultInspector())
            {
                if (mapGen.autoUpdate)
                    mapGen.GenerateMap();
            }

            if (GUILayout.Button("Generate"))
            {
                mapGen.GenerateMap();
            }
        }

    }

}