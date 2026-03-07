using UnityEngine;
using TMPro;

public class VendingMachine : MonoBehaviour
{
    [Header("Snack Settings")]
    [SerializeField] private float snackCost = 10f;
    [SerializeField] private float energyRestoreAmount = 50f;

    [Header("UI")]
    [SerializeField] private GameObject promptUI;        // "Press E to buy snack - $10"
    [SerializeField] private TextMeshProUGUI promptText; // Optional: shows cost dynamically
    [SerializeField] private GameObject cannotAffordUI;  // "Not enough funds!" message

    private bool playerInRange = false;

    private void Start()
    {
        if (promptUI) promptUI.SetActive(false);
        if (cannotAffordUI) cannotAffordUI.SetActive(false);

        if (promptText != null)
            promptText.text = "Buy Snack - $" + snackCost.ToString("F0") + "\n[Press E]";
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryBuySnack();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI) promptUI.SetActive(false);
            if (cannotAffordUI) cannotAffordUI.SetActive(false);
        }
    }

    private void TryBuySnack()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentFunds >= snackCost)
        {
            GameManager.Instance.currentFunds -= snackCost;
            GameManager.Instance.currentEnergy = Mathf.Min(
                GameManager.Instance.currentEnergy + energyRestoreAmount,
                GameManager.Instance.maxEnergy
            );

            Debug.Log($"Snack bought! Energy restored by {energyRestoreAmount}. Funds left: {GameManager.Instance.currentFunds}");

            if (cannotAffordUI) cannotAffordUI.SetActive(false);
        }
        else
        {
            Debug.Log("Not enough funds to buy a snack!");
            if (cannotAffordUI) cannotAffordUI.SetActive(true);
        }
    }

    // Call this from a UI Button's OnClick if you prefer a button over pressing E
    public void OnBuyButtonPressed()
    {
        TryBuySnack();
    }
}