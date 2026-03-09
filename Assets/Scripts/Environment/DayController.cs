using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Environment
{
    public class DayController : MonoBehaviour
    {
        [Header("Time Constraints")]
        public float dayLengthSeconds   = 60f;
        public float dayProgressSeconds = 0f;
        public int   currentDay         = 1;

        public int   CurrentDay         => currentDay;
        public float DayProgressSeconds => dayProgressSeconds;

        [Header("References")]
        public Light           sunLight;
        public TextMeshProUGUI dayText;

        [Header("Events")]
        public UnityEvent dayPassedEvent;

        private void Start()
        {
            // Auto-wire SeasonManager
            if (SeasonManager.Instance != null)
            {
                dayPassedEvent.RemoveListener(SeasonManager.Instance.OnDayPassed);
                dayPassedEvent.AddListener(SeasonManager.Instance.OnDayPassed);
            }

            // Restore saved day and time of day on scene load
            // if (GameManager.Instance != null && GameManager.Instance.savedDay > 0)
            // {
            //     ApplySavedState(GameManager.Instance.savedDay,
            //                     GameManager.Instance.savedDayProgressSeconds);
            // }
            if (dayText != null)
            {
                dayText.text = "Day: " + currentDay;
            }
        }

        void Update()
        {
            dayProgressSeconds += Time.deltaTime;

            // Continuously sync to GameManager so it's always
            // current when switching scenes mid-day
            if (GameManager.Instance != null)
            {
                GameManager.Instance.savedDay                = currentDay;
                GameManager.Instance.savedDayProgressSeconds = dayProgressSeconds;
            }

            if (sunLight != null)
            {
                float ratio = dayProgressSeconds / dayLengthSeconds;
                sunLight.transform.rotation = Quaternion.Euler((ratio * 360f) - 90f, -30f, 0f);
            }

            if (dayProgressSeconds >= dayLengthSeconds)
                AdvanceDay();
        }

        public void AdvanceDay()
        {
            dayProgressSeconds = 0f;
            currentDay++;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.savedDay                = currentDay;
                GameManager.Instance.savedDayProgressSeconds = 0f;
            }

            if (dayText != null)
                dayText.text = "Day: " + currentDay;

            if (RewardManager.Instance != null)
                RewardManager.Instance.CheckDayMilestones();

            dayPassedEvent?.Invoke();
        }

        public void ApplySavedState(int day, float progress)
        {
            currentDay         = day;
            dayProgressSeconds = progress;
            if (dayText != null)
                dayText.text = "Day: " + currentDay;
        }
    }
}