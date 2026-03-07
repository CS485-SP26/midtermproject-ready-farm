using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Farming;
using Environment;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float currentFunds = 100f;
    public float currentWater = 100f;
    public float maxWater = 100f;
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;
    public int seedCount = 5;

    [Header("Inventory")]
    public int harvestedCrops = 0;

    [Header("Day State")]
    public int savedDay = 1;
    public float savedDayProgressSeconds = 0f;
    private bool hasSavedDayState = false;

    // ===== Farm tile persistence =====

    [System.Serializable]
    private class FarmTileState
    {
        public string name;
        public Vector3 localPosition;
        public FarmTile.Condition condition;
        public int daysSinceLastInteraction;
        public Plant.GrowthState plantState;
        public int plantDaysSincePlanted;
    }

    private readonly Dictionary<string, FarmTileState> savedTilesByName = new Dictionary<string, FarmTileState>();
    private bool hasSavedFarmTiles = false;

    private const string FarmSceneName = "Scene1-FarmingSim";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // GameManager persists across ALL scenes
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.MoveTowards(currentEnergy, maxEnergy, Time.deltaTime * 2f);
        }
    }

    public void LoadScene(string sceneName)
    {
        // Save farm state before leaving the farm scene
        Scene active = SceneManager.GetActiveScene();
        if (active.name == FarmSceneName)
        {
            SaveDayState();
            SaveFarmTiles();
        }

        // harvestedCrops, seedCount, currentFunds etc. are all fields on this
        // DontDestroyOnLoad object so they survive the scene load automatically.
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == FarmSceneName && (hasSavedFarmTiles || hasSavedDayState))
        {
            StartCoroutine(RestoreFarmStateNextFrame());
        }
    }

    // ===== Helpers =====

    private static FarmTile[] FindAllFarmTiles()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<FarmTile>(FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType<FarmTile>();
#endif
    }

    private static global::Environment.DayController FindDayController()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<global::Environment.DayController>();
#else
        return Object.FindObjectOfType<global::Environment.DayController>();
#endif
    }

    private void SaveDayState()
    {
        var dc = FindDayController();
        if (dc == null) return;

        savedDay = dc.CurrentDay;
        savedDayProgressSeconds = dc.DayProgressSeconds;
        hasSavedDayState = true;
    }

    // ===== Tile & Plant Save/Restore =====

    public void SaveFarmTiles()
    {
        savedTilesByName.Clear();

        FarmTile[] tiles = FindAllFarmTiles();
        for (int i = 0; i < tiles.Length; i++)
        {
            FarmTile t = tiles[i];
            if (t == null) continue;

            var state = new FarmTileState
            {
                name = t.gameObject.name,
                localPosition = t.transform.localPosition,
                condition = t.GetCondition,
                daysSinceLastInteraction = t.DaysSinceLastInteraction
            };

            if (t.ActivePlant != null)
            {
                state.plantState = t.ActivePlant.CurrentState;
                state.plantDaysSincePlanted = t.ActivePlant.daysSincePlanted;
            }
            else
            {
                state.plantState = Plant.GrowthState.NoPlant;
            }

            savedTilesByName[state.name] = state;
        }

        hasSavedFarmTiles = savedTilesByName.Count > 0;
    }

    private IEnumerator RestoreFarmStateNextFrame()
    {
        yield return null;

        var dc = FindDayController();
        if (dc != null && hasSavedDayState)
            dc.ApplySavedState(savedDay, savedDayProgressSeconds);

        if (!hasSavedFarmTiles) yield break;

        FarmTile[] tiles = FindAllFarmTiles();
        if (tiles == null || tiles.Length == 0) yield break;

        HashSet<FarmTile> used = new HashSet<FarmTile>();

        foreach (var pair in savedTilesByName)
        {
            FarmTileState state = pair.Value;
            FarmTile match = null;

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].gameObject.name == state.name)
                {
                    match = tiles[i];
                    break;
                }
            }

            if (match == null)
            {
                float best = float.PositiveInfinity;
                for (int i = 0; i < tiles.Length; i++)
                {
                    FarmTile t = tiles[i];
                    if (t == null || used.Contains(t)) continue;

                    float d = (t.transform.localPosition - state.localPosition).sqrMagnitude;
                    if (d < best) { best = d; match = t; }
                }
                if (match != null && best > 0.0001f) match = null;
            }

            if (match != null)
            {
                match.transform.localPosition = state.localPosition;
                match.ApplyState(state.condition, state.daysSinceLastInteraction);

                if (state.plantState != Plant.GrowthState.NoPlant && match.ActivePlant != null)
                    match.ActivePlant.SetGrowthState(state.plantState, state.plantDaysSincePlanted);

                used.Add(match);
            }
        }
    }

    // ===== Store / Selling logic =====

    public void BuySeeds(int amount, float cost)
    {
        if (currentFunds >= cost)
        {
            currentFunds -= cost;
            seedCount += amount;
            Debug.Log("Seeds bought! Total: " + seedCount);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    public void SellAllCrops(float pricePerCrop)
    {
        if (harvestedCrops > 0)
        {
            float earnings = harvestedCrops * pricePerCrop;
            currentFunds += earnings;
            Debug.Log($"Sold {harvestedCrops} crops for ${earnings}");
            harvestedCrops = 0;
        }
    }

    public void RefillWater()
    {
        currentWater = maxWater;
        Debug.Log("Water Refilled!");
    }
}