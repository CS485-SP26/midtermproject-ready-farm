using UnityEngine;
using TMPro; // Important for TextMeshPro
using UnityEngine.Events;
using Farming;

namespace Environment
{
    public class DayController : MonoBehaviour
    {
        [Header("Object References")]
        [SerializeField] private Light sunLight;
        [SerializeField] private TMP_Text dayLabel;

        [Header("Time Constraints")]
        [SerializeField] private float dayLengthSeconds = 60f;
        [SerializeField] private float dayProgressSeconds = 0f; // good for debugging from the editor
        [SerializeField] private int currentDay = 1; // Good for debugging from the editor

        // Properties
        public float DayProgressPercent => Mathf.Clamp01(dayProgressSeconds / dayLengthSeconds);
        public int CurrentDay { get { return currentDay; } }

        // NEW: expose these so GameManager can save/restore day state
        public float DayProgressSeconds => dayProgressSeconds;
        public float DayLengthSeconds => dayLengthSeconds;

        public UnityEvent dayPassedEvent = new UnityEvent(); // Invoke() at end of day

        private void Start()
        {
            RefreshLabel();
            UpdateVisuals();
        }

        private void RefreshLabel()
        {
            if (dayLabel)
            {
                dayLabel.SetText("Days: {0}", currentDay);
            }
        }

        // NEW: called by GameManager when returning to the farm scene
        public void ApplySavedState(int savedDay, float savedProgressSeconds)
        {
            currentDay = Mathf.Max(1, savedDay);
            dayProgressSeconds = Mathf.Clamp(savedProgressSeconds, 0f, dayLengthSeconds);

            RefreshLabel();
            UpdateVisuals();
        }

        public void AdvanceDay()
        {
            Debug.Assert(sunLight, "DayController requires a 'Sun'");
            if (dayLabel == null) Debug.Log("DayController does not have a label to update");

            dayProgressSeconds = 0f; // Reset to start a new day
            currentDay++;

            if (dayLabel)
            {
                dayLabel.SetText("Days: {0}", currentDay);
            }

            dayPassedEvent.Invoke(); //make announcement to all listeners
        }

        public void UpdateVisuals()
        {
            // Calculate sun's rotation based on time of day
            // 0 degrees for sunrise, 180 for sunset, 360 for next sunrise
            float sunRotationX = Mathf.Lerp(0f, 360f, DayProgressPercent);

            // Apply rotation to the sun light
            sunLight.transform.rotation = Quaternion.Euler(sunRotationX, 0f, 0f);
        }

        void Update()
        {
            dayProgressSeconds += Time.deltaTime;

            if (dayProgressSeconds >= dayLengthSeconds)
            {
                AdvanceDay();
            }

            UpdateVisuals();
        }
    }
}