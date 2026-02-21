using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float currentFunds = 100f; 
    public float currentWater = 100f;
    public float maxWater = 100f;
    public int seedCount = 5; 

    private void Awake()
    {
        // Singleton pattern to ensure only one manager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Handles purchasing in the Store Scene
    public void BuySeeds(int amount, float cost) 
    {
        if (currentFunds >= cost)
        {
            currentFunds -= cost;
            seedCount += amount; // Increases your inventory
            Debug.Log("Seeds bought! Total: " + seedCount);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    public void RefillWater()
    {
        currentWater = maxWater;
        Debug.Log("Water Refilled!");
    }
}