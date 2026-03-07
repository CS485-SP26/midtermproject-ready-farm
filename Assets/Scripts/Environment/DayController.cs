using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Environment
{
    public class DayController : MonoBehaviour
    {
        [Header("Time Constraints")]
        public float dayLengthSeconds = 60f; 
        public float dayProgressSeconds = 0f; 
        public int currentDay = 1; 

        // Properties for GameManager access
        public int CurrentDay => currentDay; 
        public float DayProgressSeconds => dayProgressSeconds;

        [Header("References")]
        public Light sunLight;
        public TextMeshProUGUI dayText;

        [Header("Events")]
        // Renamed from onDayPassed to dayPassedEvent to fix FarmTileManager errors
        public UnityEvent dayPassedEvent; 

        void Update()
        {
            dayProgressSeconds += Time.deltaTime;

            if (sunLight != null)
            {
                float ratio = dayProgressSeconds / dayLengthSeconds;
                // Standard sun rotation logic
                sunLight.transform.rotation = Quaternion.Euler((ratio * 360f) - 90f, -30f, 0f);
            }

            if (dayProgressSeconds >= dayLengthSeconds)
            {
                AdvanceDay();
            }
        }

        public void AdvanceDay()
        {
            dayProgressSeconds = 0f;
            currentDay++;

            if (dayText != null)
                dayText.text = "Day: " + currentDay;

            // Notifies the FarmTileManager and plants that a day has passed
            dayPassedEvent?.Invoke();
        }

        public void ApplySavedState(int day, float progress)
        {
            currentDay = day;
            dayProgressSeconds = progress;
            if (dayText != null) dayText.text = "Day: " + currentDay;
        }
    }
}