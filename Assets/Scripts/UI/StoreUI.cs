using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Environment;

public class StoreUI : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI seedsText;
    public TextMeshProUGUI cropsText;
    public TextMeshProUGUI dayText;

    [Header("Resource Visuals")]
    public Image waterFillImage;
    public Image energyFillImage;

    [Header("Store Settings")]
    [SerializeField] private float seedPrice     = 5f;
    [SerializeField] private float cropSellPrice = 25f;

    private DayController _dayController;

    private void Start()
    {
        _dayController = Object.FindFirstObjectByType<DayController>();

        // Force the fill images to the correct values immediately
        // then again after 2 frames to catch the snapshot restore
        RefreshAll();
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        yield return null;
        yield return null;
        RefreshAll();
    }

    // LateUpdate runs after all Updates — reads final GameManager values
    void LateUpdate() => RefreshAll();

    private void RefreshAll()
    {
        if (GameManager.Instance == null) return;

        if (fundsText != null)
            fundsText.text = "Funds: $" + GameManager.Instance.currentFunds.ToString("F0");

        if (seedsText != null)
            seedsText.text = "Seeds: " + GameManager.Instance.seedCount;

        if (cropsText != null)
            cropsText.text = "Crops: " + GameManager.Instance.harvestedCrops;

        if (waterFillImage != null)
            waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;

        if (energyFillImage != null)
            energyFillImage.fillAmount = GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;

        if (dayText != null && _dayController != null)
            dayText.SetText("Days: {0}", _dayController.CurrentDay);
    }

    // --- BUTTON ACTIONS ---
    public void BuySeedsUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BuySeeds(1, seedPrice);
    }

    public void RefillWaterUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RefillWater();
    }

    public void SellCropsUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.harvestedCrops <= 0)
        {
            Debug.Log("No crops to sell!");
            return;
        }

        int   amountToSell = GameManager.Instance.harvestedCrops;
        float totalProfit  = amountToSell * cropSellPrice;

        GameManager.Instance.currentFunds  += totalProfit;
        GameManager.Instance.harvestedCrops = 0;

        Debug.Log($"Sold {amountToSell} crops for ${totalProfit}!");
    }

    public void ReturnToFarm()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("Scene1-FarmingSim");
        else
            SceneManager.LoadScene("Scene1-FarmingSim");
    }
}