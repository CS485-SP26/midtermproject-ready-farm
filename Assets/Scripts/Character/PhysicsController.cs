using UnityEngine;

// TODO: Consider the benefits of refactoring to namespace Movement
namespace Character
{
    public class PhysicsMovement : MovementController
    {
        [SerializeField] float drag = 0.5f;
        [SerializeField] float rotationSpeed = 0.1f;
        
        protected override void Start()
        {
            base.Start();
            rb.linearDamping = drag;
        }

        public override float GetHorizontalSpeedPercent()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            return Mathf.Clamp01(horizontalVelocity.magnitude / maxVelocity);;
        }

        public override void Jump() 
        { 
            // TODO: integrate jump support from week 2-3    
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate(); // TODO: remove base.FixedUpdate() when starting your integration
            ApplyMovement();
            ClampVelocity();
            ApplyRotation();
            ApplyJump();
        }
        
        void ApplyMovement()
        {
            // TODO integrate your physics from week 2-3 
        }

        void ApplyJump()
        {
            // TODO integrate your jump logic from week 2-3 
        }

        // TODO integrate collision support from week 2-3 
        
        void ClampVelocity()
        {
            // Clamp horizontal velocity while preserving vertical (for jumping/falling)
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            if (horizontalVelocity.magnitude > maxVelocity)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxVelocity;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }

        void ApplyRotation()
        {
            Vector3 direction = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (direction.magnitude > 0.5f)
            {
                // 1. Calculate the target rotation (where we WANT to look)
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

                // 2. Smoothly rotate from our current rotation toward the target
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.fixedDeltaTime
                );
            }
        }
    }
}
