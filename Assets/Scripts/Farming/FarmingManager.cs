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

            if (condition == FarmTile.Condition.Grass)
            {
                gardenHoe.SetActive(true);
                animatedController.SetTrigger("Till");
                tile.Interact();
                Invoke(nameof(StopFarmingAction), hoeDuration);
            }
            else if (condition == FarmTile.Condition.Tilled)
            {
                // Pull water from GameManager Singleton
                if (GameManager.Instance.currentWater >= 10f)
                {
                    waterCan.SetActive(true);
                    animatedController.SetTrigger("Water");
                    tile.Interact();
                    
                    GameManager.Instance.currentWater -= 10f;
                    Invoke(nameof(StopFarmingAction), waterDuration);
                }
            }
        }

        private void CheckWinCondition()
        {
            FarmTile[] allTiles = FindObjectsOfType<FarmTile>();
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