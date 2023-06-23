using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProceduralLandmassGeneration
{
    [CreateAssetMenu()]
    public class UpdatableData : ScriptableObject
    {
        public event System.Action OnValuesUpdated;

        public bool autoUpdate;

        protected virtual void OnValidate()
        {
            if (autoUpdate)
            {
                NotifyUpdatedValues();
            }
        }


        public void NotifyUpdatedValues()
        {
            if (OnValuesUpdated != null)
                OnValuesUpdated();
        }

    }
}
