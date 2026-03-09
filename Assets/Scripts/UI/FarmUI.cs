using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Environment;

public class FarmUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI farmSeedsText;
    public TextMeshProUGUI cropCountText;
    public TextMeshProUGUI seasonText;      // Shows "Season: Spring"
    public TextMeshProUGUI dayOfWeekText;   // Shows "Monday"
    public TextMeshProUGUI seasonDayLabel;  // Shows "Spring - Monday Day: 3"

    [Header("Resource Bars")]
    public Image waterFillImage;
    public Image energyFillImage;

    void Start() => StartCoroutine(DelayedRefresh());

    private IEnumerator DelayedRefresh()
    {
        yield return null;
        yield return null;
        RefreshAll();
    }

    void LateUpdate() => RefreshAll();

    private void RefreshAll()
    {
        if (GameManager.Instance != null)
        {
            if (farmSeedsText != null)
                farmSeedsText.text = "Seeds: " + GameManager.Instance.seedCount;

            if (fundsText != null)
                fundsText.text = "Funds: $" + GameManager.Instance.currentFunds.ToString("F0");

            if (cropCountText != null)
                cropCountText.text = "Crops: " + GameManager.Instance.harvestedCrops;

            if (waterFillImage != null)
                waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;

            if (energyFillImage != null)
                energyFillImage.fillAmount = GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;
        }

        if (SeasonManager.Instance != null)
        {
            string season = SeasonManager.Instance.GetSeasonName();
            string day    = SeasonManager.Instance.GetDayName();
            int    dayNum = (GameManager.Instance != null) ? GameManager.Instance.savedDay : 1;

            if (seasonText != null)
                seasonText.text = "Season: " + season;

            if (dayOfWeekText != null)
                dayOfWeekText.text = day;

            if (seasonDayLabel != null)
                seasonDayLabel.text = season + " - " + day + "\nDay: " + dayNum;
        }
    }
}