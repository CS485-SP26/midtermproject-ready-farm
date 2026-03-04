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
        [SerializeField] private Image waterFillImage;
        [SerializeField] private Image energyFillImage;
        [SerializeField] private GameObject winMessage; 

        [Header("Durations")]
        [SerializeField] private Character.AnimatedController animatedController;
        [SerializeField] private float hoeDuration = 5f;
        [SerializeField] private float waterDuration = 5.5f;

        private bool fundsAwarded = false;

        private void Start()
        {
            HideAllTools();
            if (winMessage) winMessage.SetActive(false);
        }

        private void Update()
        {
            UpdateWaterUI();
            UpdateEnergyUI();
            CheckWinCondition();
        }

        public void InteractWithTile(FarmTile tile)
        {
            if (tile == null) return;
            CancelInvoke(nameof(StopFarmingAction));

            // ── If there's a plant on the tile, handle it first ──────────────
            var plant = tile.ActivePlant;
            if (plant != null)
            {
                if (plant.CanHarvest)
                {
                    if (GameManager.Instance.currentEnergy >= 5f)
                    {
                        GameManager.Instance.currentEnergy -= 5f;
                        gardenHoe.SetActive(true);
                        animatedController.SetTrigger("Till");
                        tile.Interact(); // calls Harvest()
                        GameManager.Instance.currentFunds += 25f;
                        Invoke(nameof(StopFarmingAction), hoeDuration);
                    }
                    else { Debug.Log("Not enough energy to harvest!"); }
                }
                else
                {
                    if (GameManager.Instance.currentWater >= 10f)
                    {
                        waterCan.SetActive(true);
                        animatedController.SetTrigger("Water");
                        tile.Interact(); // calls WaterPlant()
                        GameManager.Instance.currentWater -= 10f;
                        UpdateWaterUI();
                        Invoke(nameof(StopFarmingAction), waterDuration);
                    }
                    else { Debug.Log("Not enough water to water the plant!"); }
                }
                return;
            }

            // ── No plant – interact with bare soil ────────────────────────────
            FarmTile.Condition condition = tile.GetCondition;

            if (condition == FarmTile.Condition.Grass)
            {
                if (GameManager.Instance.currentEnergy >= 10f)
                {
                    GameManager.Instance.currentEnergy -= 10f;
                    gardenHoe.SetActive(true);
                    animatedController.SetTrigger("Till");
                    tile.Interact();
                    Invoke(nameof(StopFarmingAction), hoeDuration);
                }
                else { Debug.Log("Not enough energy to till the soil!"); }
            }
            else if (condition == FarmTile.Condition.Tilled)
            {
                if (GameManager.Instance.currentWater >= 10f)
                {
                    waterCan.SetActive(true);
                    animatedController.SetTrigger("Water");
                    tile.Interact();
                    GameManager.Instance.currentWater -= 10f;
                    UpdateWaterUI();
                    Invoke(nameof(StopFarmingAction), waterDuration);
                }
                else { Debug.Log("Not enough water to water the soil!"); }
            }
            else if (condition == FarmTile.Condition.Watered)
            {
                // Plant a seed
                if (GameManager.Instance.currentEnergy >= 10f)
                {
                    GameManager.Instance.currentEnergy -= 10f;
                    gardenHoe.SetActive(true);
                    animatedController.SetTrigger("Till");
                    tile.Interact(); // calls PlantSeed()
                    Invoke(nameof(StopFarmingAction), hoeDuration);
                }
                else { Debug.Log("Not enough energy to plant a seed!"); }
            }
            else if (condition == FarmTile.Condition.Withered)
            {
                // Till over the dead plant remnants
                if (GameManager.Instance.currentEnergy >= 10f)
                {
                    GameManager.Instance.currentEnergy -= 10f;
                    gardenHoe.SetActive(true);
                    animatedController.SetTrigger("Till");
                    tile.Interact();
                    Invoke(nameof(StopFarmingAction), hoeDuration);
                }
                else { Debug.Log("Not enough energy to till the soil!"); }
            }
        }

        private void CheckWinCondition()
        {
            FarmTile[] allTiles = FindObjectsByType<FarmTile>(FindObjectsSortMode.None);
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
                Debug.Log("Win Condition Met: Funds Awarded!");
            }
        }

        private void StopFarmingAction()
        {
            HideAllTools();
        }

        public void HideAllTools()
        {
            if (gardenHoe) gardenHoe.SetActive(false);
            if (waterCan) waterCan.SetActive(false);
        }

        private void UpdateWaterUI()
        {
            // Syncs the UI bar with the GameManager's persistent water level
            if (waterFillImage != null && GameManager.Instance != null)
                waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;
        }

        private void UpdateEnergyUI()
        {
            // Syncs the UI bar with the GameManager's persistent energy level
            if (energyFillImage != null && GameManager.Instance != null)
                energyFillImage.fillAmount = GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;
        }
    }
}