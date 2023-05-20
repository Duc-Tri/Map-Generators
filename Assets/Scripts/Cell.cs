using UnityEngine;

namespace MapGenerator
{
    public class Cell
    {
        public bool isWater = false;

        public Color color;

        internal float noiseValue;
    }
}