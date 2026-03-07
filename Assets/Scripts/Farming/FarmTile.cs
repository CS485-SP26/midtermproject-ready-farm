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
        private Plant activePlant = null; 

        public Condition GetCondition => tileCondition;
        public int DaysSinceLastInteraction => daysSinceLastInteraction;
        public Plant ActivePlant => activePlant;

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
            if (minimapIconRenderer != null || minimapIconSprite != null) return;
            Transform icon = transform.Find("MinimapIcon") ?? transform.Find("MiniMapIcon"); 

            if (icon == null)
            {
                minimapIconSprite = GetComponentInChildren<SpriteRenderer>(true);
                if (minimapIconSprite == null) minimapIconRenderer = GetComponentInChildren<Renderer>(true);
                return;
            }

            minimapIconSprite = icon.GetComponent<SpriteRenderer>();
            if (minimapIconSprite == null) minimapIconRenderer = icon.GetComponent<Renderer>();
        }

        private void UpdateMinimapVisual()
        {
            CacheMinimapIcon();
            Color c = (tileCondition == Condition.Tilled && activePlant != null) ? minimapPlantedColor : GetMinimapColorForCondition(tileCondition);

            if (minimapIconSprite != null)
            {
                minimapIconSprite.color = c;
                return;
            }

            if (minimapIconRenderer != null)
            {
                if (minimapMPB == null) minimapMPB = new MaterialPropertyBlock();
                minimapIconRenderer.GetPropertyBlock(minimapMPB);
                minimapMPB.SetColor("_BaseColor", c); 
                minimapMPB.SetColor("_Color", c); 
                minimapIconRenderer.SetPropertyBlock(minimapMPB);
            }
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

        public void PlantSeed()
        {
            if (plantPrefabs == null || plantPrefabs.Length == 0) return;
            if (activePlant != null) return; 

            GameObject prefab = plantPrefabs[Random.Range(0, plantPrefabs.Length)];
            
            var go = Instantiate(prefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);
    
            activePlant = go.GetComponent<Plant>();
    
            if (activePlant != null) 
            {
                activePlant.BeginGrowing();
                tileCondition = Condition.Planted;
                UpdateVisual();
            }
        }

        public void Harvest()
        {
            if (activePlant != null)
            {
                if (GameManager.Instance != null) GameManager.Instance.currentFunds += 25f; 
                Destroy(activePlant.gameObject);
                activePlant = null;
            }
            tileCondition = Condition.Tilled;
            daysSinceLastInteraction = 0;
            UpdateVisual();
        }

        public void Till()
        {
            if (activePlant != null) { Destroy(activePlant.gameObject); activePlant = null; }
            tileCondition = Condition.Tilled;
            daysSinceLastInteraction = 0;
            UpdateVisual();
            tillAudio?.Play();
        }

        public void Water()
        {
            tileCondition = Condition.Watered;
            daysSinceLastInteraction = 0;
            UpdateVisual();
            waterAudio?.Play();
        }

        public void ApplyState(Condition condition, int daysSince)
        {
            tileCondition = condition;
            daysSinceLastInteraction = daysSince;
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
                    case Condition.Planted:  tileRenderer.material = wateredMaterial; break; 
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
        }

        public void OnDayPassed()
        {
            if (activePlant != null && tileCondition != Condition.Withered)
            {
                // If the tile isn't watered, the plant withers
                if (tileCondition != Condition.Watered && tileCondition != Condition.Planted)
                {
                    activePlant.Wither();
                    tileCondition = Condition.Withered;
                }
                else
                {
                    // Successful growth
                    activePlant.OnDayPassed();
                    
                    activePlant.UpdateVisuals(); 
                    
                    tileCondition = Condition.Planted; 
                }
                
                daysSinceLastInteraction = 0;
                UpdateVisual();
                return; 
            }

            daysSinceLastInteraction++;

            if (daysSinceLastInteraction >= 1)
            {
                if (tileCondition == Condition.Watered) tileCondition = Condition.Tilled;
                else if (tileCondition == Condition.Tilled) tileCondition = Condition.Grass;
            }

            UpdateVisual();
        }
    }
}