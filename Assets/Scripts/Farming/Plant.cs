using UnityEngine;

namespace Farming
{
    public class Plant : MonoBehaviour
    {
        public enum GrowthState { NoPlant, Planted, Growing, Mature, Withered }

        [Header("Configuration")]
        [SerializeField] private int seedDays = 1;    // 1 day as a seed
        [SerializeField] private int growingDays = 1; // 1 day growing
        [SerializeField] private int matureDays = 3;  // 3 days mature before withering

        [Header("State")]
        public GrowthState state = GrowthState.NoPlant;
        public int daysSincePlanted = 0;

        public GrowthState CurrentState => state;
        public bool IsWithered => state == GrowthState.Withered;
        public bool CanHarvest => state == GrowthState.Mature;

        [Header("Visuals")]
        [SerializeField] private GameObject noPlantVisual;
        [SerializeField] private GameObject plantedVisual;
        [SerializeField] private GameObject growingVisual;
        [SerializeField] private GameObject matureVisual;
        [SerializeField] private GameObject witheredVisual;

        private void Awake()
        {
            ResetChildTransforms();
        }

        private void Start() 
        {
            UpdateVisuals();
        }

        public void ResetChildTransforms()
        {
            GameObject[] allVisuals = { noPlantVisual, plantedVisual, growingVisual, matureVisual, witheredVisual };
            
            foreach (GameObject visual in allVisuals)
            {
                if (visual != null)
                {
                    // Sets local Y to 0.2 so they aren't buried
                    visual.transform.localPosition = new Vector3(0, 0.2f, 0); 
                    visual.transform.localScale = Vector3.one;
                }
            }
        }

        public void BeginGrowing()
        {
            state = GrowthState.Planted;
            daysSincePlanted = 0;
            UpdateVisuals();
        }

        public void SetGrowthState(GrowthState newState, int daysSince)
        {
            state = newState;
            daysSincePlanted = daysSince;
            UpdateVisuals();
        }

        public void ResetPlant()
        {
            state = GrowthState.NoPlant;
            daysSincePlanted = 0;
            UpdateVisuals();
        }

        public void Wither()
        {
            state = GrowthState.Withered;
            UpdateVisuals();
        }

        public void OnDayPassed()
        {
            if (state == GrowthState.Withered || state == GrowthState.NoPlant) return;

            daysSincePlanted++;

            // Precise thresholds
            int growThreshold = seedDays;                              // Day 1
            int matureThreshold = seedDays + growingDays;              // Day 2
            int witherThreshold = seedDays + growingDays + matureDays; // Day 5

            if (daysSincePlanted >= witherThreshold)
                state = GrowthState.Withered;
            else if (daysSincePlanted >= matureThreshold)
                state = GrowthState.Mature;
            else if (daysSincePlanted >= growThreshold)
                state = GrowthState.Growing;
            else
                state = GrowthState.Planted;

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (noPlantVisual) noPlantVisual.SetActive(state == GrowthState.NoPlant);
            if (plantedVisual) plantedVisual.SetActive(state == GrowthState.Planted);
            if (growingVisual) growingVisual.SetActive(state == GrowthState.Growing);
            if (matureVisual) matureVisual.SetActive(state == GrowthState.Mature);
            if (witheredVisual) witheredVisual.SetActive(state == GrowthState.Withered);
        }
    }
}