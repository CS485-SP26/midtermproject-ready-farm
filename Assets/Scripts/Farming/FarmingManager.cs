using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 

namespace Farming
{
    public class FarmingManager : MonoBehaviour
    {
        [Header("Tools")]
        [SerializeField] private GameObject gardenHoe;
        [SerializeField] private GameObject waterCan;
        
        [Header("UI & Feedback")]
        [SerializeField] private GameObject winMessage; 

        [Header("Durations")]
        [SerializeField] private Character.AnimatedController animatedController;
        [SerializeField] private float hoeDuration   = 2.0f; 
        [SerializeField] private float waterDuration = 2.0f;

        private bool fundsAwarded = false;

        private void Start()
        {
            HideAllTools();
            if (winMessage) winMessage.SetActive(false);

            // Reset harvestedCrops to 0 on scene start to prevent stale values
            if (GameManager.Instance != null)
            {
                Debug.Log($"[FarmingManager] Start — harvestedCrops = {GameManager.Instance.harvestedCrops}");
            }
        }

        private void Update()
        {
            CheckWinCondition();
        }

        public void InteractWithTile(FarmTile tile)
        {
            if (tile == null) return;

            CancelInvoke(nameof(StopFarmingAction));
            HideAllTools();

            float energyCost = 10f;
            float waterCost  = 15f;

            var plant = tile.ActivePlant;
            FarmTile.Condition condition = tile.GetCondition;

            // 1. TILLING A PLANTED TILE
            if (plant != null)
            {
                if (GameManager.Instance.currentEnergy >= energyCost)
                {
                    GameManager.Instance.currentEnergy -= energyCost;
                    gardenHoe.SetActive(true);
                    animatedController.SetTrigger("Till");

                    if (plant.CanHarvest)
                    {
                        Debug.Log($"[FarmingManager] Harvesting — before: {GameManager.Instance.harvestedCrops}");
                        GameManager.Instance.harvestedCrops++;
                        Debug.Log($"[FarmingManager] Harvested — after: {GameManager.Instance.harvestedCrops}");

                        if (RewardManager.Instance != null)
                            RewardManager.Instance.CheckMilestones();
                    }
                    else
                    {
                        Debug.Log("Plant destroyed before maturity.");
                    }

                    Destroy(plant.gameObject);
                    tile.ApplyState(FarmTile.Condition.Tilled, 0);
                    Invoke(nameof(StopFarmingAction), hoeDuration);
                }
                return;
            }

            // 2. WATERING
            if ((condition == FarmTile.Condition.Tilled || condition == FarmTile.Condition.Planted) && condition != FarmTile.Condition.Watered)
            {
                if (GameManager.Instance.currentWater >= waterCost)
                {
                    GameManager.Instance.currentWater -= waterCost;
                    waterCan.SetActive(true);
                    animatedController.SetTrigger("Water");
                    tile.ApplyState(FarmTile.Condition.Watered, 0);
                    Invoke(nameof(StopFarmingAction), waterDuration);
                }
                else { Debug.Log("Out of water!"); }
                return;
            }

            // 3. PLANTING
            if ((condition == FarmTile.Condition.Tilled || condition == FarmTile.Condition.Watered) && plant == null)
            {
                if (GameManager.Instance.seedCount > 0)
                {
                    GameManager.Instance.seedCount--;
                    GameManager.Instance.currentEnergy -= energyCost;
                    HideAllTools();
                    animatedController.SetTrigger("Till");
                    tile.PlantSeed();
                    Invoke(nameof(StopFarmingAction), hoeDuration);
                }
                else { Debug.Log("No seeds left!"); }
                return;
            }

            // 4. TILLING BARE GROUND
            if (condition == FarmTile.Condition.Grass || condition == FarmTile.Condition.Withered)
            {
                if (GameManager.Instance.currentEnergy >= energyCost)
                {
                    GameManager.Instance.currentEnergy -= energyCost;
                    gardenHoe.SetActive(true);
                    animatedController.SetTrigger("Till");
                    tile.ApplyState(FarmTile.Condition.Tilled, 0);
                    Invoke(nameof(StopFarmingAction), hoeDuration);
                }
                return;
            }
        }

        private void CheckWinCondition()
        {
            FarmTile[] allTiles = Object.FindObjectsByType<FarmTile>(FindObjectsSortMode.None);
            if (allTiles.Length == 0) return;

            bool allWatered = true;
            foreach (FarmTile tile in allTiles)
            {
                var c = tile.GetCondition;
                if (c != FarmTile.Condition.Watered && c != FarmTile.Condition.Planted)
                {
                    allWatered = false;
                    break;
                }
            }

            if (allWatered && !fundsAwarded)
            {
                if (winMessage) winMessage.SetActive(true);
                GameManager.Instance.currentFunds += 100f;
                fundsAwarded = true;

                if (RewardManager.Instance != null)
                    RewardManager.Instance.OnAllTilesWatered();
            }
        }

        private void StopFarmingAction() => HideAllTools();

        public void HideAllTools()
        {
            if (gardenHoe) gardenHoe.SetActive(false);
            if (waterCan)  waterCan.SetActive(false);
            var player = Object.FindAnyObjectByType<Character.PlayerController>();
            if (player != null) player.SetTool(0);
        }
    }
}