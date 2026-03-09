using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusBarUI : MonoBehaviour
{
    [SerializeField] private Image energyBarFill;
    [SerializeField] private Image waterBarFill;

    void Start()
    {
        // Refresh immediately and again after 2 frames
        // to catch the GameManager snapshot restore
        UpdateBars();
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        yield return null;
        yield return null;
        UpdateBars();
    }

    // LateUpdate ensures we always read the final value after GameManager.Update()
    void LateUpdate() => UpdateBars();

    void UpdateBars()
    {
        if (GameManager.Instance == null) return;
        if (energyBarFill != null)
            energyBarFill.fillAmount = GameManager.Instance.currentEnergy / GameManager.Instance.maxEnergy;
        if (waterBarFill != null)
            waterBarFill.fillAmount  = GameManager.Instance.currentWater  / GameManager.Instance.maxWater;
    }
}