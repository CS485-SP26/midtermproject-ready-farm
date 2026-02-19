using Character;
using UnityEngine;

namespace Character {
    public class AnimatedController : MonoBehaviour
    {
        [SerializeField] float moveSpeed; // useful to observe for debugging
        MovementController moveController;
        Animator animator;
        protected Animator Animator { get { return animator; } }
        void Start()
        {
            animator = GetComponent<Animator>();
            moveController = GetComponent<MovementController>();
        }

        public void SetTrigger(string name)
        {
            animator.SetTrigger(name);
        }

        void Update()
        {
            moveSpeed = moveController.GetHorizontalSpeedPercent();
            animator.SetFloat("Speed", moveSpeed);
        }
    }
}
