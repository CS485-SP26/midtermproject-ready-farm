using UnityEngine;
using UnityEngine.Events;

namespace Environment
{
    public class SeasonManager : MonoBehaviour
    {
        public static SeasonManager Instance { get; private set; }

        public enum DayOfWeek { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
        public enum Season    { Spring, Summer, Autumn, Winter }

        [Header("Season Settings")]
        [SerializeField] private int daysPerWeek    = 7;
        [SerializeField] private int weeksPerSeason = 4;
        private int DaysPerSeason => daysPerWeek * weeksPerSeason;

        [Header("Wither Rates per Season (0-1)")]
        [SerializeField] private float springWitherRate = 0.05f;
        [SerializeField] private float summerWitherRate = 0.10f;
        [SerializeField] private float autumnWitherRate = 0.20f;
        [SerializeField] private float winterWitherRate = 0.40f;

        [Header("Sun Tint per Season")]
        [SerializeField] private Color springTint = new Color(0.85f, 1.00f, 0.85f);
        [SerializeField] private Color summerTint = new Color(1.00f, 0.95f, 0.70f);
        [SerializeField] private Color autumnTint = new Color(1.00f, 0.75f, 0.45f);
        [SerializeField] private Color winterTint = new Color(0.75f, 0.88f, 1.00f);

        [Header("Events")]
        public UnityEvent onWeekPassed;
        public UnityEvent onSeasonChanged;

        public Season    CurrentSeason       { get; private set; } = Season.Spring;
        public DayOfWeek CurrentDayOfWeek    { get; private set; } = DayOfWeek.Monday;
        public int       CurrentWeekInSeason { get; private set; } = 1;
        public float     CurrentWitherRate   { get; private set; }

        private Light         _sunLight;
        private DayController _dayController;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _dayController = Object.FindFirstObjectByType<DayController>();
            _sunLight      = Object.FindFirstObjectByType<Light>();

            int startDay = (GameManager.Instance != null && GameManager.Instance.savedDay > 0)
                ? GameManager.Instance.savedDay : 1;
            UpdateFromAbsoluteDay(startDay);
        }

        private void OnEnable()  => UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            _dayController = Object.FindFirstObjectByType<DayController>();
            _sunLight      = Object.FindFirstObjectByType<Light>();
            ApplySeasonEffects();

            int day = (GameManager.Instance != null) ? GameManager.Instance.savedDay : 1;
            UpdateFromAbsoluteDay(day);
        }

        public void OnDayPassed()
        {
            int absoluteDay = (_dayController != null) ? _dayController.CurrentDay : 1;
            UpdateFromAbsoluteDay(absoluteDay);
        }

        private void UpdateFromAbsoluteDay(int absoluteDay)
        {
            int totalDaysPerYear = DaysPerSeason * 4;
            int dayInYear        = (absoluteDay - 1) % totalDaysPerYear;
            int seasonIndex      = dayInYear / DaysPerSeason;
            Season newSeason     = (Season)seasonIndex;

            int dayInSeason  = dayInYear % DaysPerSeason;
            int newWeek      = (dayInSeason / daysPerWeek) + 1;
            int dayInWeek    = dayInSeason % daysPerWeek;
            CurrentDayOfWeek = (DayOfWeek)dayInWeek;

            if (newWeek != CurrentWeekInSeason)
            {
                CurrentWeekInSeason = newWeek;
                onWeekPassed?.Invoke();
            }

            if (newSeason != CurrentSeason)
            {
                CurrentSeason = newSeason;
                ApplySeasonEffects();
                onSeasonChanged?.Invoke();
                Debug.Log("Season changed to: " + CurrentSeason);
            }
        }

        private void ApplySeasonEffects()
        {
            if (_sunLight != null)
                _sunLight.color = GetSunTint(CurrentSeason);
            CurrentWitherRate = GetWitherRate(CurrentSeason);
        }

        public float GetWitherRate(Season season)
        {
            switch (season)
            {
                case Season.Spring: return springWitherRate;
                case Season.Summer: return summerWitherRate;
                case Season.Autumn: return autumnWitherRate;
                case Season.Winter: return winterWitherRate;
                default:            return summerWitherRate;
            }
        }

        private Color GetSunTint(Season season)
        {
            switch (season)
            {
                case Season.Spring: return springTint;
                case Season.Summer: return summerTint;
                case Season.Autumn: return autumnTint;
                case Season.Winter: return winterTint;
                default:            return summerTint;
            }
        }

        public string GetSeasonName() => CurrentSeason.ToString();
        public string GetDayName()    => CurrentDayOfWeek.ToString();
    }
}