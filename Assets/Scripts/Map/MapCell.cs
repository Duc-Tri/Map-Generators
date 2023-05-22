using UnityEngine;

namespace MapGenerator
{
    public class MapCell
    {
        public bool isWater = false;

        public Color color;

        internal float noiseValue;
    }
}