using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI seedsText;
    public TextMeshProUGUI cropsText; // NEW: Displays how many crops you are carrying
    public TextMeshProUGUI dayText;

    [Header("Resource Visuals")]
    public Image waterFillImage;
    public Image energyFillImage; // NEW: Added energy sync for consistency

    [Header("Store Settings")]
    [SerializeField] private float seedPrice = 5f;
    [SerializeField] private float cropSellPrice = 25f;

    private void Start()
    {
        TryAutoFindDayLabel();
    }

    private void TryAutoFindDayLabel()
    {
        if (dayText != null) return;

        GameObject go = GameObject.Find("DayLabel");
        if (go != null)
            dayText = go.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            // 1. Update Currency and Inventory
            if (fundsText != null)
                fundsText.text = "Funds: $" + GameManager.Instance.currentFunds.ToString("F0");

            if (seedsText != null)
                seedsText.text = "Seeds: " + GameManager.Instance.seedCount.ToString();

            // Part 13: Display harvested crops count
            if (cropsText != null)
                cropsText.text = "Crops: " + GameManager.Instance.harvestedCrops.ToString();

            // 2. Update Resource Bars
            if (waterFillImage != null)
                waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;

            if (energyFillImage != null)
                energyFillImage.fillAmount = GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;

            // 3. Update Day Label (using the saved state from GameManager)
            if (dayText != null)
                dayText.SetText("Days: {0}", GameManager.Instance.savedDay);
        }
    }

    // --- BUTTON ACTIONS ---

    public void BuySeedsUI()
    {
        if (GameManager.Instance != null)
        {
            // Uses the central GameManager logic to deduct funds and add seeds
            GameManager.Instance.BuySeeds(1, seedPrice);
        }
    }

    public void RefillWaterUI()
    {
        if (GameManager.Instance != null)
        {
            // Refills the watering can to maxWater
            GameManager.Instance.RefillWater();
        }
    }

    // Part 13 Implementation: Selling Crops
    public void SellCropsUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.harvestedCrops > 0)
        {
            int amountToSell = GameManager.Instance.harvestedCrops;
            float totalProfit = amountToSell * cropSellPrice;

            GameManager.Instance.currentFunds += totalProfit;
            GameManager.Instance.harvestedCrops = 0; // Clear inventory after sale

            Debug.Log($"Sold {amountToSell} crops for ${totalProfit}!");
        }
        else
        {
            Debug.Log("No crops to sell!");
        }
    }

    public void ReturnToFarm()
    {
        // Important: Use GameManager.LoadScene to ensure the farm state is restored correctly
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("Scene1-FarmingSim");
        else
            SceneManager.LoadScene("Scene1-FarmingSim");
    }
}