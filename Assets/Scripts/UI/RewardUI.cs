using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RewardUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject      rewardPanel;
    [SerializeField] private CanvasGroup     canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image           iconImage;

    [Header("Grant Rows")]
    [SerializeField] private Transform  grantContainer;
    [SerializeField] private GameObject grantRowPrefab;

    [Header("Timing")]
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeSpeed       = 4f;

    private readonly Queue<RewardData> _queue = new Queue<RewardData>();
    private bool _showing = false;

    private struct RewardData
    {
        public string       title;
        public string       desc;
        public Sprite       icon;
        public List<string> grants;
    }

    private void Awake()
    {
        // Hide panel via CanvasGroup — keep GameObject ACTIVE so coroutines work
        if (canvasGroup != null)
        {
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (rewardPanel != null)     rewardPanel.SetActive(false);
        if (titleText != null)       titleText.gameObject.SetActive(false);
        if (descriptionText != null) descriptionText.gameObject.SetActive(false);
    }

    public void ShowReward(string title, string desc, Sprite icon, List<string> grantLines)
    {
        Debug.Log($"[RewardUI] ShowReward called: {title}");
        _queue.Enqueue(new RewardData { title = title, desc = desc, icon = icon, grants = grantLines });
        if (!_showing)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _showing = true;
        while (_queue.Count > 0)
            yield return StartCoroutine(DisplayReward(_queue.Dequeue()));
        _showing = false;
    }

    private IEnumerator DisplayReward(RewardData data)
    {
        Debug.Log($"[RewardUI] Displaying: {data.title}");

        if (titleText       != null) titleText.text       = data.title;
        if (descriptionText != null) descriptionText.text = data.desc;

        if (iconImage != null)
        {
            iconImage.sprite  = data.icon;
            iconImage.enabled = data.icon != null;
        }

        if (grantContainer != null)
        {
            foreach (Transform child in grantContainer)
                Destroy(child.gameObject);

            if (data.grants != null)
            {
                foreach (string line in data.grants)
                {
                    if (grantRowPrefab == null) continue;
                    GameObject row = Instantiate(grantRowPrefab, grantContainer);
                    var tmp = row.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = line;
                }
            }
        }

        // Activate
        if (rewardPanel != null) rewardPanel.SetActive(true);
        if (titleText != null)   titleText.gameObject.SetActive(true);
        if (descriptionText != null) descriptionText.gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;

            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(displayDuration);

        // Fade out
        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (titleText != null)   titleText.gameObject.SetActive(false);
        if (descriptionText != null) descriptionText.gameObject.SetActive(false);
    }
}