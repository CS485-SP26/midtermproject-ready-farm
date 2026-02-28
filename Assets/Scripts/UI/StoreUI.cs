using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI seedsText;
    public Image waterFillImage;

    // NEW: hook this up to Canvas -> DayLabel (TextMeshProUGUI)
    public TextMeshProUGUI dayText;

    private void TryAutoFindDayLabel()
    {
        // Optional safety: auto-find if you forgot to assign it in the Inspector
        if (dayText != null) return;

        GameObject go = GameObject.Find("DayLabel");
        if (go != null)
            dayText = go.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            // Updates Money
            if (fundsText != null)
                fundsText.text = "Funds: $" + GameManager.Instance.currentFunds.ToString("F0");

            // Updates Seeds
            if (seedsText != null)
                seedsText.text = "Seeds: " + GameManager.Instance.seedCount.ToString();

            // Updates Water Bar
            if (waterFillImage != null)
                waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;

            // NEW: Updates Day Label in the store
            TryAutoFindDayLabel();
            if (dayText != null)
                dayText.SetText("Days: {0}", GameManager.Instance.savedDay);
        }
    }

    public void BuySeedsUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BuySeeds(1, 5f);
    }

    public void RefillWaterUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RefillWater();
    }

    public void ReturnToFarm()
    {
        // Use GameManager so the farm restore logic stays consistent
        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("Scene1-FarmingSim");
        else
            SceneManager.LoadScene("Scene1-FarmingSim");
    }
}