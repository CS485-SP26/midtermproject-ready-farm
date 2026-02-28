using UnityEngine;
using Farming;
using UnityEngine.SceneManagement;
public class ShopEntrance : MonoBehaviour
{
    [SerializeField] private GameObject enterButtonUI; 
    [SerializeField] private FarmTileManager FarmTileManager; 


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
        Debug.Log("Going to store - saving farm data");
        
        if(FarmTileManager != null)
        {
            FarmTileManager.SaveFarmTiles();
        }
       
        
        // Load store scene
        GameManager.Instance.LoadScene("Scene2-Store");
    }
    public void ReturnToFarm()
    {
        Debug.Log("Returning to farm");
        
        // Load farm scene by NAME
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("Scene1-FarmingSim"); // Use exact scene name
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1-FarmingSim");
        }
    }
}