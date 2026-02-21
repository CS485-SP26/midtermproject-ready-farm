using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class FarmUI : MonoBehaviour
{
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI farmSeedsText; 
    public Image waterFillImage; 

    void Update()
    {
        if (GameManager.Instance != null)
        {
        
            if(farmSeedsText != null)
                farmSeedsText.text = "Seeds: " + GameManager.Instance.seedCount.ToString();
            
            if (fundsText != null)
                fundsText.text = "Funds: $" + GameManager.Instance.currentFunds.ToString("F0");

            if (waterFillImage != null)
            {
                waterFillImage.fillAmount = GameManager.Instance.currentWater / GameManager.Instance.maxWater;
            }
        }
    }
}