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
    public float currentFunds  = 100f;
    public float currentWater  = 100f;
    public float maxWater      = 100f;
    public float currentEnergy = 100f;
    public float maxEnergy     = 100f;
    public int   seedCount     = 5;
    public float speedBonus   = 0f;   // Added by RewardManager speed grants

    [Header("Inventory")]
    public int harvestedCrops = 0;

    [Header("Day State")]
    public int   savedDay                = 1;
    public float savedDayProgressSeconds = 0f;
    private bool hasSavedDayState        = false;

    // ── Snapshot ──────────────────────────────────────────────────────────────
    private float _snapWater;
    private float _snapEnergy;
    private float _snapFunds;
    private int   _snapSeeds;
    private int   _snapCrops;
    private int   _snapDay;
    private float _snapDayProgress;
    private bool  _hasSnapshot    = false;
    private bool  _isTransitioning = false; // Pauses energy restore during transitions

    // ── Farm tile persistence ─────────────────────────────────────────────────
    [System.Serializable]
    private class FarmTileState
    {
        public string             name;
        public Vector3            localPosition;
        public FarmTile.Condition condition;
        public int                daysSinceLastInteraction;
        public Plant.GrowthState  plantState;
        public int                plantDaysSincePlanted;
    }

    private readonly Dictionary<string, FarmTileState> savedTilesByName
        = new Dictionary<string, FarmTileState>();
    private bool hasSavedFarmTiles = false;

    private const string FarmSceneName = "Scene1-FarmingSim";

    // ── Unity ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Update()
    {
        // Pause energy restore during scene transitions so snapshot isn't overwritten
        if (_isTransitioning) return;

        if (currentEnergy < maxEnergy)
            currentEnergy = Mathf.MoveTowards(currentEnergy, maxEnergy, Time.deltaTime * 2f);
    }

    // ── Scene Loading ─────────────────────────────────────────────────────────
    public void LoadScene(string sceneName)
    {
        _isTransitioning = true;
        TakeSnapshot();

        Scene active = SceneManager.GetActiveScene();
        if (active.name == FarmSceneName)
        {
            SaveDayState();
            SaveFarmTiles();
        }

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_hasSnapshot)
            RestoreSnapshot();

        if (scene.name == FarmSceneName && (hasSavedFarmTiles || hasSavedDayState))
            StartCoroutine(RestoreFarmStateNextFrame());
        else
            StartCoroutine(FinishTransition(hasSavedDayState)); 
    }

    private IEnumerator FinishTransition(bool restoreDay = false)
    {
        yield return null;
        yield return null;

        if (restoreDay)
        {
            var dc = FindDayController();
            if (dc != null)
            {
                dc.ApplySavedState(_snapDay, _snapDayProgress);
            }
        }

        _isTransitioning = false;
    }

    // ── Snapshot ──────────────────────────────────────────────────────────────
    private void TakeSnapshot()
    {
        _snapWater       = currentWater;
        _snapEnergy      = currentEnergy;
        _snapFunds       = currentFunds;
        _snapSeeds       = seedCount;
        _snapCrops       = harvestedCrops;
        _hasSnapshot     = true;

        // Also snapshot day from live DayController if available
        var dc = FindDayController();
        if (dc != null)
        {
            _snapDay         = savedDay;
            _snapDayProgress = savedDayProgressSeconds;
        }

        if (savedDay > 0)
        {
            hasSavedDayState = true;
        }

        _snapDay = savedDay;
        _snapDayProgress = savedDayProgressSeconds;

        Debug.Log($"[GameManager] Snapshot — Water={_snapWater:F1} Energy={_snapEnergy:F1} Funds={_snapFunds} Day={_snapDay}");
    }

    private void RestoreSnapshot()
    {
        currentWater   = _snapWater;
        currentEnergy  = _snapEnergy;
        currentFunds   = _snapFunds;
        seedCount      = _snapSeeds;
        harvestedCrops = _snapCrops;

        Debug.Log($"[GameManager] Restored — Water={currentWater:F1} Energy={currentEnergy:F1} Funds={currentFunds} Day={_snapDay}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
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
        savedDay                = dc.CurrentDay;
        savedDayProgressSeconds = dc.DayProgressSeconds;
        hasSavedDayState        = true;
    }

    // ── Tile & Plant Save/Restore ─────────────────────────────────────────────
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
                name                     = t.gameObject.name,
                localPosition            = t.transform.localPosition,
                condition                = t.GetCondition,
                daysSinceLastInteraction = t.DaysSinceLastInteraction,
                plantState               = Plant.GrowthState.NoPlant
            };

            if (t.ActivePlant != null)
            {
                state.plantState            = t.ActivePlant.CurrentState;
                state.plantDaysSincePlanted = t.ActivePlant.daysSincePlanted;
            }

            savedTilesByName[state.name] = state;
        }

        hasSavedFarmTiles = savedTilesByName.Count > 0;
        Debug.Log($"[GameManager] Saved {savedTilesByName.Count} tiles.");
    }

    private IEnumerator RestoreFarmStateNextFrame()
    {
        yield return null;
        yield return null;

        var dc = FindDayController();
        if (dc != null && hasSavedDayState)
            dc.ApplySavedState(_snapDay, _snapDayProgress);

        if (!hasSavedFarmTiles)
        {
            _isTransitioning = false;
            yield break;
        }

        FarmTile[] tiles = FindAllFarmTiles();
        if (tiles == null || tiles.Length == 0)
        {
            _isTransitioning = false;
            yield break;
        }

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

            if (match == null) continue;

            match.transform.localPosition = state.localPosition;
            match.ApplyState(state.condition, state.daysSinceLastInteraction);

            if (state.plantState != Plant.GrowthState.NoPlant)
            {
                if (match.ActivePlant == null)
                    match.PlantSeed();

                yield return null;

                if (match.ActivePlant != null)
                    match.ActivePlant.SetGrowthState(state.plantState, state.plantDaysSincePlanted);
            }

            used.Add(match);
        }

        _isTransitioning = false;
        Debug.Log("[GameManager] Farm state fully restored.");
    }

    // ── Store / Selling ───────────────────────────────────────────────────────
    public void BuySeeds(int amount, float cost)
    {
        if (currentFunds >= cost)
        {
            currentFunds -= cost;
            seedCount    += amount;
        }
        else Debug.Log("Not enough money!");
    }

    public void SellAllCrops(float pricePerCrop)
    {
        if (harvestedCrops > 0)
        {
            float earnings = harvestedCrops * pricePerCrop;
            currentFunds  += earnings;
            harvestedCrops = 0;
        }
    }

    public void RefillWater()
    {
        currentWater = maxWater;
        Debug.Log("Water Refilled!");
    }
}