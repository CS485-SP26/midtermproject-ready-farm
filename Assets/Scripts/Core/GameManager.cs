using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Add this at the top with other usings
using Farming; // Add this to access FarmTile.Condition

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float currentFunds = 100f; 
    public float currentWater = 100f;
    public float maxWater = 100f;
    public int seedCount = 5; 

    // ADD THIS - Farm persistence data
    [System.Serializable]
    public class TileSaveData
    {
        public int tileIndex;
        public FarmTile.Condition condition;
        
        public TileSaveData(int index, FarmTile.Condition condition)
        {
            tileIndex = index;
            this.condition = condition;
        }
    }
    
    public List<TileSaveData> savedTiles = new List<TileSaveData>();
    public bool hasSavedFarmData = false;

    // This runs when the script is first loaded (even before scenes)
    /*
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // If no GameManager exists, create one
        if (Instance == null)
        {
            GameObject gameManagerGO = new GameObject("GameManager");
            Instance = gameManagerGO.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerGO);
            Debug.Log("GameManager created by initializer");
        }
    }
*/
     private void Awake()
    {
        
        
        // If this is the Instance, keep it
        if (Instance == this)
        {
            return;
        }
        
        // If there's already an Instance and it's not this, destroy this
        if (Instance != null)
        {
            
            Destroy(gameObject);
            return;
        }
        
        // If we get here, there is no Instance, so this becomes the Instance
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    private void OnDestroy()
    {
        
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    //added by kian
    

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