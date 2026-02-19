using UnityEngine;

namespace Character {
    [RequireComponent(typeof(Rigidbody))]
    public class MovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] protected float acceleration = 20f;
        [SerializeField] protected float maxVelocity = 5f;
        protected Rigidbody rb;
        protected Vector2 moveInput;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Move(Vector2 lateralInput)
        {
            moveInput = lateralInput;
        }

        public void Stop()
        {
            rb.linearVelocity = Vector3.zero;
            moveInput = Vector2.zero;
        }

        public virtual void Jump() { /* NO JUMP SUPPORT */ }

        public virtual float GetHorizontalSpeedPercent()
        {
            return moveInput == Vector2.zero ? 0f : 1f;
        }

        protected virtual void FixedUpdate()
        {
            SimpleMovement();
        }

        void SimpleMovement()
        {
            Vector3 movement = Vector3.zero;
            movement += transform.right * moveInput.x;
            movement += transform.forward * moveInput.y;
            movement.Normalize();
            movement *= Time.deltaTime * acceleration;
            rb.MovePosition(rb.position + movement);
        }
    }
}