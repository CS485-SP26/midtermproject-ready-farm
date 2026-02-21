using UnityEngine;

namespace Character
{
    public class AnimatedController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float maxSpeed = 5f;

        void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (rb == null) rb = GetComponentInParent<Rigidbody>();
        }

        void Update()
        {
            if (animator == null || rb == null) return;

            // Simple movement speed update for the Blend Tree
            float speed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", speed / maxSpeed, 0.1f, Time.deltaTime);
        }

        public void SetTrigger(string triggerName)
        {
            if (animator != null) animator.SetTrigger(triggerName);
        }
    }
}