using UnityEngine;

namespace Farming
{
    public class Plant : MonoBehaviour
    {
        public enum GrowthState { NoPlant, Planted, Growing, Mature, Withered }

        [Header("Configuration")]
        [Tooltip("Days before the plant starts visibly growing.")]
        [SerializeField] private int daysToGrow   = 1;
        [Tooltip("Total days until the plant is mature and ready to harvest.")]
        [SerializeField] private int daysToMature = 3;

        [Header("State")]
        [SerializeField] private GrowthState state           = GrowthState.NoPlant;
        [SerializeField] private int         daysSincePlanted = 0;

        public GrowthState CurrentState => state;
        public bool IsWithered => state == GrowthState.Withered;
        public bool CanHarvest => state == GrowthState.Mature;

        [Header("Visuals")]
        [Tooltip("Shown when the tile has no plant (cleared / just-harvested).")]
        [SerializeField] private GameObject noPlantVisual;
        [Tooltip("Shown immediately after a seed is placed.")]
        [SerializeField] private GameObject plantedVisual;
        [Tooltip("Shown while the plant is actively growing.")]
        [SerializeField] private GameObject growingVisual;
        [Tooltip("Shown when the plant is ready to harvest.")]
        [SerializeField] private GameObject matureVisual;
        [Tooltip("Shown when the plant has died from lack of water.")]
        [SerializeField] private GameObject witheredVisual;

        private void Start() => UpdateVisuals();

        /// <summary>Recursively resets any flattened scales in child transforms.</summary>
        private void ResetChildScales()
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child != transform)
                {
                    child.localScale = Vector3.one;
                }
            }
        }

        /// <summary>Call this after instantiating the prefab to move out of NoPlant.</summary>
        public void BeginGrowing()
        {
            ResetChildScales();
            state            = GrowthState.Planted;
            daysSincePlanted = 0;
            UpdateVisuals();
        }

        /// <summary>Called by FarmTile.OnDayPassed each in-game day.</summary>
        public void OnDayPassed()
        {
            if (state == GrowthState.Withered || state == GrowthState.NoPlant) return;

            daysSincePlanted++;

            if      (daysSincePlanted >= daysToMature) state = GrowthState.Mature;
            else if (daysSincePlanted >= daysToGrow)   state = GrowthState.Growing;

            UpdateVisuals();
        }

        /// <summary>Called by FarmTile when the soil dries out overnight.</summary>
        public void Wither()
        {
            state = GrowthState.Withered;
            UpdateVisuals();
        }

        public void ResetPlant()
        {
            state            = GrowthState.NoPlant;
            daysSincePlanted = 0;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (noPlantVisual)  noPlantVisual.SetActive(state  == GrowthState.NoPlant);
            if (plantedVisual)  plantedVisual.SetActive(state  == GrowthState.Planted);
            if (growingVisual)  growingVisual.SetActive(state  == GrowthState.Growing);
            if (matureVisual)   matureVisual.SetActive(state   == GrowthState.Mature);
            if (witheredVisual) witheredVisual.SetActive(state == GrowthState.Withered);
        }
    }
}