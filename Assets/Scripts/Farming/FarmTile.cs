using System.Collections.Generic;
using UnityEngine;

namespace Farming
{
    public class FarmTile : MonoBehaviour
    {
        public enum Condition { Grass, Tilled, Watered, Planted, Withered }

        [SerializeField] private Condition tileCondition = Condition.Grass;

        [Header("Visuals")]
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Material tilledMaterial;
        [SerializeField] private Material wateredMaterial;

        private MeshRenderer tileRenderer;

        [Header("Audio")]
        [SerializeField] private AudioSource stepAudio;
        [SerializeField] private AudioSource tillAudio;
        [SerializeField] private AudioSource waterAudio;

        private readonly List<Material> materials = new List<Material>();

        private int daysSinceLastInteraction = 0;
        private Plant activePlant = null; // tracked directly since plant isn't a child of this tile

        public Condition GetCondition => tileCondition;
        public int DaysSinceLastInteraction => daysSinceLastInteraction;
        public Plant ActivePlant => activePlant;

        // ===== Minimap Icon Coloring (NEW) =====
        [Header("Minimap Colors")]
        [SerializeField] private Color minimapGrassColor   = new Color(0.10f, 0.60f, 0.10f, 1f);
        [SerializeField] private Color minimapTilledColor  = new Color(0.78f, 0.70f, 0.45f, 1f);
        [SerializeField] private Color minimapWateredColor = new Color(0.45f, 0.25f, 0.12f, 1f);
        [SerializeField] private Color minimapPlantedColor  = new Color(0.20f, 0.80f, 0.20f, 1f);
        [SerializeField] private Color minimapWitheredColor = new Color(0.70f, 0.70f, 0.00f, 1f);

        [SerializeField] private Renderer minimapIconRenderer;
        [SerializeField] private SpriteRenderer minimapIconSprite;

        [Header("Planting")]
        [SerializeField] private GameObject[] plantPrefabs;

        public GameObject[] PlantPrefabs
        {
            get => plantPrefabs;
            set => plantPrefabs = value;
        }

        private MaterialPropertyBlock minimapMPB;

        private void Awake()
        {
            CacheRenderers();
            CacheMinimapIcon();
            UpdateVisual();
        }

        private void CacheRenderers()
        {
            if (tileRenderer == null)
                tileRenderer = GetComponent<MeshRenderer>();

            Debug.Assert(tileRenderer, "FarmTile requires a MeshRenderer");

            // Cache edge materials once (for highlight emission)
            if (materials.Count == 0)
            {
                foreach (Transform edge in transform)
                {
                    var mr = edge.gameObject.GetComponent<MeshRenderer>();
                    if (mr != null)
                        materials.Add(mr.material);
                }
            }
        }

        private void CacheMinimapIcon()
        {
            // If already assigned manually, keep it
            if (minimapIconRenderer != null || minimapIconSprite != null)
                return;

            // Your hierarchy shows "MinimapIcon" under each tile
            Transform icon = transform.Find("MinimapIcon");
            if (icon == null) icon = transform.Find("MiniMapIcon"); // fallback spelling

            if (icon == null)
            {
                // last fallback: look anywhere under this tile
                minimapIconSprite = GetComponentInChildren<SpriteRenderer>(true);
                if (minimapIconSprite != null) return;

                minimapIconRenderer = GetComponentInChildren<Renderer>(true);
                return;
            }

            minimapIconSprite = icon.GetComponent<SpriteRenderer>();
            if (minimapIconSprite != null) return;

            minimapIconRenderer = icon.GetComponent<Renderer>();
        }

        private Color GetMinimapColorForCondition(Condition c)
        {
            switch (c)
            {
                case Condition.Tilled:   return minimapTilledColor;
                case Condition.Watered:  return minimapWateredColor;
                case Condition.Planted:  return minimapPlantedColor;
                case Condition.Withered: return minimapWitheredColor;
                default:                 return minimapGrassColor;
            }
        }

        private void UpdateMinimapVisual()
        {
            CacheMinimapIcon();

            Color c = GetMinimapColorForCondition(tileCondition);

            // If it’s a SpriteRenderer icon, set its color directly
            if (minimapIconSprite != null)
            {
                minimapIconSprite.color = c;
                return;
            }

            // Otherwise tint the renderer via property block (safe with shared materials)
            if (minimapIconRenderer == null) return;

            if (minimapMPB == null)
                minimapMPB = new MaterialPropertyBlock();

            minimapIconRenderer.GetPropertyBlock(minimapMPB);

            // Try common shader color properties
            minimapMPB.SetColor("_BaseColor", c); // URP Lit/Unlit
            minimapMPB.SetColor("_Color", c);     // legacy / many shaders
            minimapMPB.SetColor("_TintColor", c); // some unlit shaders

            minimapIconRenderer.SetPropertyBlock(minimapMPB);
        }

        public void Interact()
        {
            // If a plant is present, forward to plant logic first
            if (activePlant != null)
            {
                if (activePlant.CanHarvest)
                    Harvest();
                else
                    WaterPlant();

                daysSinceLastInteraction = 0;
                return;
            }

            // No plant – interact with soil
            switch (tileCondition)
            {
                case Condition.Grass:    Till();      break;
                case Condition.Tilled:   Water();     break;
                case Condition.Watered:  PlantSeed(); break;
                case Condition.Withered: Till();      break;
            }
            daysSinceLastInteraction = 0;
        }

        public void PlantSeed()
        {
            if (plantPrefabs == null || plantPrefabs.Length == 0)
            {
                Debug.LogError("FarmTile: No plant prefabs assigned – drag one into the Plant Prefabs array on this tile (or on FarmTileManager).");
                return;
            }

            GameObject prefab = plantPrefabs[Random.Range(0, plantPrefabs.Length)];
            if (prefab == null) { Debug.LogError("FarmTile: Selected plant prefab is null."); return; }

            // Parent to the tile's parent (FarmTileManager) instead of the tile itself,
            // so the plant is never subject to the tile's non-uniform scale.
            Transform safeParent = transform.parent != null ? transform.parent : transform;
            var go = Instantiate(prefab, transform.position, Quaternion.identity, safeParent);
            activePlant = go.GetComponent<Plant>();
            if (activePlant != null) activePlant.BeginGrowing();
            tileCondition = Condition.Planted;
            daysSinceLastInteraction = 0;
            UpdateVisual();
        }

        public void WaterPlant()
        {
            tileCondition = Condition.Watered;
            daysSinceLastInteraction = 0;
            UpdateVisual();
            waterAudio?.Play();
        }

        public void Harvest()
        {
            if (activePlant != null)
            {
                Destroy(activePlant.gameObject);
                activePlant = null;
            }
            tileCondition = Condition.Tilled;
            UpdateVisual();
        }

        public void Till()
        {
            tileCondition = Condition.Tilled;
            UpdateVisual();
            tillAudio?.Play();
        }

        public void Water()
        {
            tileCondition = Condition.Watered;
            UpdateVisual();
            waterAudio?.Play();
        }

        // Apply saved state after scene reload
        public void ApplyState(Condition condition, int daysSince)
        {
            tileCondition = condition;
            daysSinceLastInteraction = Mathf.Max(0, daysSince);

            CacheRenderers();
            CacheMinimapIcon();
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (tileRenderer != null)
            {
                switch (tileCondition)
                {
                    case Condition.Grass:    tileRenderer.material = grassMaterial;   break;
                    case Condition.Tilled:   tileRenderer.material = tilledMaterial;  break;
                    case Condition.Watered:  tileRenderer.material = wateredMaterial; break;
                    case Condition.Planted:  tileRenderer.material = wateredMaterial; break; // reuse watered look under the plant
                    case Condition.Withered: tileRenderer.material = tilledMaterial;  break;
                }
            }
            UpdateMinimapVisual();
        }

        public void SetHighlight(bool active)
        {
            foreach (Material m in materials)
            {
                if (m == null) continue;

                if (active) m.EnableKeyword("_EMISSION");
                else m.DisableKeyword("_EMISSION");
            }

            if (active && stepAudio != null)
                stepAudio.Play();
        }

        public void OnDayPassed()
        {
            if (activePlant != null)
            {
                if (tileCondition != Condition.Watered)
                {
                    activePlant.Wither();
                    tileCondition = Condition.Withered;
                }
                activePlant.OnDayPassed();
                daysSinceLastInteraction++;
                UpdateVisual();
                return;
            }

            daysSinceLastInteraction++;

            if (daysSinceLastInteraction >= 1)
            {
                if (tileCondition == Condition.Watered)    tileCondition = Condition.Tilled;
                else if (tileCondition == Condition.Tilled) tileCondition = Condition.Grass;
            }

            UpdateVisual();
        }
    }
}