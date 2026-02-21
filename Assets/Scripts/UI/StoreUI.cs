using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

public class StoreUI : MonoBehaviour
{
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI seedsText; 
    public Image waterFillImage; 

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
        SceneManager.LoadScene("Scene1-FarmingSim"); 
    }
}