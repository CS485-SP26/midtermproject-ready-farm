using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] protected float acceleration = 20f;
        [SerializeField] protected float maxVelocity = 5f;

        [Header("Camera Relative Movement")]
        [SerializeField] private Transform cameraTransform; // optional override

        protected Rigidbody rb;
        protected Vector2 moveInput;

        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (!cameraTransform && Camera.main)
                cameraTransform = Camera.main.transform;
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

        protected Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (!cameraTransform && Camera.main)
                cameraTransform = Camera.main.transform;

            // Fallback: world-relative if no camera
            if (!cameraTransform)
                return new Vector3(input.x, 0f, input.y);

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 dir = right * input.x + forward * input.y;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            return dir;
        }

        void SimpleMovement()
        {
            Vector3 dir = GetCameraRelativeDirection(moveInput);
            Vector3 movement = dir * (Time.deltaTime * acceleration);
            rb.MovePosition(rb.position + movement);
        }
    }
}
