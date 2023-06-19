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
            if (GUILayout.Button("Generate") || DrawDefaultInspector() && mapGen.autoUpdate)
                mapGen.DrawMapInEditor();
        }

    }

}