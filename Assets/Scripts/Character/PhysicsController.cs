using UnityEngine;

namespace Character
{
    public class PhysicsController : MovementController
    {
        [Header("Physics & Braking")]
        [SerializeField] private float brakingForce = 15f;
        [SerializeField] private float rotationSpeed = 720f;

        protected override void Start()
        {
            base.Start();
            
            // Standard Physics Setup
            rb.linearDamping = 2f; 
            rb.freezeRotation = true; // Prevents the capsule from tipping over
            
            // CRITICAL: Set Interpolate to 'Interpolate' for smooth camera following
            rb.interpolation = RigidbodyInterpolation.Interpolate; 
        }

        protected override void FixedUpdate()
        {
            // If we have movement input, move. Otherwise, apply brakes.
            if (moveInput.sqrMagnitude > 0.01f)
            {
                ApplyMovement();
                ApplyRotation();
            }
            else
            {
                ApplyBraking();
            }

            ClampVelocity();
        }

        void ApplyMovement()
        {
            // Calculate direction once using the Camera
            Vector3 moveDir = GetCameraRelativeDirection(moveInput);
            rb.AddForce(moveDir * acceleration, ForceMode.Acceleration);
        }

        void ApplyRotation()
        {
            Vector3 moveDir = GetCameraRelativeDirection(moveInput);
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        void ApplyBraking()
        {
            // Counter-force to stop the "drifting away" behavior
            Vector2 horizontalVel = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
            Vector3 counterForce = new Vector3(-horizontalVel.x, 0, -horizontalVel.y) * brakingForce;
            rb.AddForce(counterForce, ForceMode.Acceleration);
        }

        void ClampVelocity()
        {
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVel.magnitude > maxVelocity)
            {
                horizontalVel = horizontalVel.normalized * maxVelocity;
                rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
            }
        }
    }
}