using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class FarmUI : MonoBehaviour
{
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI farmSeedsText; 
    public TextMeshProUGUI cropCountText;  // Drag your CropCount label here
    public Image waterFillImage;
    public Image energyFillImage; 

    void Update()
    {
        if (GameManager.Instance != null)
        {
            if (farmSeedsText != null)
                farmSeedsText.text = "Seeds: " + GameManager.Instance.seedCount.ToString();
            
            if (fundsText != null)
                fundsText.text = "Funds: $" + GameManager.Instance.currentFunds.ToString("F0");

            if (cropCountText != null)
                cropCountText.text = "Crops: " + GameManager.Instance.harvestedCrops.ToString();

            if (waterFillImage != null)
                waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;

            if (energyFillImage != null)
                energyFillImage.fillAmount = GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;
        }
    }
}