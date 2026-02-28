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

            //added by kian
            RestoreTileStates();
        }

        //method added by kian
        public void RestoreTileStates()
        {
            if (GameManager.Instance != null && GameManager.Instance.hasSavedFarmData)
            {
                FarmTile[] allTiles = FindObjectsByType<FarmTile>(FindObjectsSortMode.None);
                
                foreach (var savedData in GameManager.Instance.savedTiles)
                {
                    if (savedData.tileIndex < allTiles.Length)
                    {
                        allTiles[savedData.tileIndex].RestoreState(savedData.condition, 0);
                    }
                }
                
                Debug.Log($"Restored {GameManager.Instance.savedTiles.Count} tiles");
                
                // DON'T clear here - we need the data until the player leaves the farm again
                // GameManager.Instance.ClearFarmData(); // REMOVE THIS LINE
            }
        }

        private void Update()
        {
            UpdateWaterUI();
            CheckWinCondition();
        }

        public void InteractWithTile(FarmTile tile)
        {
            if (tile == null) return;
            CancelInvoke(nameof(StopFarmingAction));

            FarmTile.Condition condition = tile.GetCondition;
            Debug.Log($"Interacting with tile. Condition: {condition}");
            Debug.Log($"Current water: {GameManager.Instance.currentWater}, Max water: {GameManager.Instance.maxWater}");

            if (condition == FarmTile.Condition.Grass)
            {
                Debug.Log("Tile is Grass - tilling");
                if (gardenHoe != null) gardenHoe.SetActive(true);
                else Debug.LogError("gardenHoe is not assigned!");
                
                if (animatedController != null) animatedController.SetTrigger("Till");
                else Debug.LogError("animatedController is not assigned!");
                
                tile.Interact();
                Invoke(nameof(StopFarmingAction), hoeDuration);
            }
            else if (condition == FarmTile.Condition.Tilled)
            {
                Debug.Log("Tile is Tilled - attempting to water");
                
                // Check if GameManager exists
                if (GameManager.Instance == null)
                {
                    Debug.LogError("GameManager.Instance is null!");
                    return;
                }
                
                // Check water level
                if (GameManager.Instance.currentWater >= 10f)
                {
                    Debug.Log($"Watering tile. Water before: {GameManager.Instance.currentWater}");
                    
                    if (waterCan != null) waterCan.SetActive(true);
                    else Debug.LogError("waterCan is not assigned!");
                    
                    if (animatedController != null) animatedController.SetTrigger("Water");
                    else Debug.LogError("animatedController is not assigned!");
                    
                    tile.Interact();
                    
                    GameManager.Instance.currentWater -= 10f;
                    Debug.Log($"Water after: {GameManager.Instance.currentWater}");
                    
                    Invoke(nameof(StopFarmingAction), waterDuration);
                }
                else
                {
                    Debug.Log($"Not enough water! Need 10, have {GameManager.Instance.currentWater}");
                }
            }
            else if (condition == FarmTile.Condition.Watered)
            {
                Debug.Log("Tile is already Watered");
            }
        }
        private void CheckWinCondition()
        {
            FarmTile[] allTiles = FindObjectsByType<FarmTile>(FindObjectsSortMode.None);
            if (allTiles.Length == 0) return;

            bool allWatered = true;
            foreach (FarmTile tile in allTiles)
            {
                if (tile.GetCondition != FarmTile.Condition.Watered)
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
    }
}