using UnityEngine;

public class ShopEntrance : MonoBehaviour
{
    [SerializeField] private GameObject enterButtonUI; 

    private void Start()
    {
        // Hide the button when the game starts
        if (enterButtonUI != null) enterButtonUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the X Bot player entered the zone
        if (other.CompareTag("Player"))
        {
            if (enterButtonUI != null) enterButtonUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Hide the button when the player walks away
        if (other.CompareTag("Player"))
        {
            if (enterButtonUI != null) enterButtonUI.SetActive(false);
        }
    }

    // This function will be called by the Button's OnClick event
    public void GoToStore()
    {
        if (GameManager.Instance != null)
        {
            // Must match the name in Build Settings exactly
            GameManager.Instance.LoadScene("Scene2-Store");
        }
    }
    public void ReturnToFarm()
    {
        if (GameManager.Instance != null)
        {
            // Make sure "Scene1-FarmingSim" matches your Build Profile exactly
            GameManager.Instance.LoadScene("Scene1-FarmingSim");
        }
    }
}