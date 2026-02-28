using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Farming;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float currentFunds = 100f;
    public float currentWater = 100f;
    public float maxWater = 100f;
    public int seedCount = 5;

    // ===== Day persistence (NEW) =====
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
    }

    private readonly Dictionary<string, FarmTileState> savedTilesByName = new Dictionary<string, FarmTileState>();
    private bool hasSavedFarmTiles = false;

    private const string FarmSceneName = "Scene1-FarmingSim";

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        // If we are leaving the farm, snapshot day + tile states first
        Scene active = SceneManager.GetActiveScene();
        if (active.name == FarmSceneName)
        {
            SaveDayState();     // NEW
            SaveFarmTiles();
        }

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == FarmSceneName && (hasSavedFarmTiles || hasSavedDayState))
        {
            // sceneLoaded happens before Start on scene objects, so wait a frame
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

    // ===== Day Save/Restore (NEW) =====

    private void SaveDayState()
    {
        var dc = FindDayController();
        if (dc == null) return;

        savedDay = dc.CurrentDay;
        savedDayProgressSeconds = dc.DayProgressSeconds;
        hasSavedDayState = true;
    }

    // ===== Tile Save/Restore =====

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

            savedTilesByName[state.name] = state;
        }

        hasSavedFarmTiles = savedTilesByName.Count > 0;
    }

    private IEnumerator RestoreFarmStateNextFrame()
    {
        yield return null;

        // 1) Restore DayController first (NEW)
        var dc = FindDayController();
        if (dc != null && hasSavedDayState)
        {
            dc.ApplySavedState(savedDay, savedDayProgressSeconds);
        }

        // 2) Restore Tiles
        if (!hasSavedFarmTiles) yield break;

        FarmTile[] tiles = FindAllFarmTiles();
        if (tiles == null || tiles.Length == 0) yield break;

        HashSet<FarmTile> used = new HashSet<FarmTile>();

        foreach (var pair in savedTilesByName)
        {
            FarmTileState state = pair.Value;
            FarmTile match = null;

            // (A) Match by name first
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].gameObject.name == state.name)
                {
                    match = tiles[i];
                    break;
                }
            }

            // (B) Fallback: match by local position
            if (match == null)
            {
                float best = float.PositiveInfinity;
                for (int i = 0; i < tiles.Length; i++)
                {
                    FarmTile t = tiles[i];
                    if (t == null || used.Contains(t)) continue;

                    float d = (t.transform.localPosition - state.localPosition).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        match = t;
                    }
                }

                // Reject if it’s not actually the same location
                if (match != null && best > 0.0001f)
                    match = null;
            }

            if (match != null)
            {
                match.transform.localPosition = state.localPosition;
                match.ApplyState(state.condition, state.daysSinceLastInteraction);
                used.Add(match);
            }
        }
    }

    // ===== Store / inventory logic =====

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

    public void RefillWater()
    {
        currentWater = maxWater;
        Debug.Log("Water Refilled!");
    }
}