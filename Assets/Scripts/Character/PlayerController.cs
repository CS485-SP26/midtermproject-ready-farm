using UnityEngine;
using UnityEngine.InputSystem;
using Farming;

namespace Character 
{
    [RequireComponent(typeof(PlayerInput))] // Input is required and we don't store a reference
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private TileSelector tileSelector;
        MovementController moveController;
        AnimatedController animatedController;

        void Start()
        {
            moveController = GetComponent<MovementController>();
            animatedController = GetComponent<AnimatedController>();

            // TODO: Consider Debug.Assert vs RequireComponent(typeof(...))
            Debug.Assert(animatedController, "PlayerController requires an animatedController");
            Debug.Assert(moveController, "PlayerController requires a MovementController");
            Debug.Assert(tileSelector, "PlayerController requires a TileSelector.");
        }
        public void OnMove(InputValue inputValue)
        {
            Vector2 inputVector = inputValue.Get<Vector2>();
            moveController.Move(inputVector);
        }

        public void OnJump(InputValue inputValue)
        {
            moveController.Jump();
        }

        public void OnInteract(InputValue value)
        {
            FarmTile tile = tileSelector.GetSelectedTile();
            if (tile != null)
            {
                tile.Interact(); // updates the condition, play the anim after
                switch (tile.GetCondition)
                {
                    case FarmTile.Condition.Tilled: animatedController.SetTrigger("Till"); break;
                    case FarmTile.Condition.Watered: animatedController.SetTrigger("Water"); break;
                    default: break;
                }
            }
        }
    }
}