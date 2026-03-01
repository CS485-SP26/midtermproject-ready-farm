using UnityEngine;


namespace Farming
{
    public class Plant : MonoBehaviour
    {
        public bool isWithered = false;
        [SerializeField] private GameObject healthyVisual;
        [SerializeField] private GameObject witheredVisual;

        public void WaterPlant()
        {
            if (isWithered) return;
            Debug.Log("Plant was watered!");
            // You could add growth stages here
        }

        public void Wither()
        {
            isWithered = true;
            if (healthyVisual) healthyVisual.SetActive(false);
            if (witheredVisual) witheredVisual.SetActive(true);
        }
    }
}