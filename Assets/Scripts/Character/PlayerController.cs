using UnityEngine;
using UnityEngine.InputSystem;
using Farming;
using System.Collections;

namespace Character
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private TileSelector tileSelector;
        [SerializeField] private FarmingManager farmingManager;

        [Header("Tool Models")]
        [SerializeField] private GameObject hoeModel;        
        [SerializeField] private GameObject wateringCanModel; 

        private MovementController moveController;
        private Coroutine hideToolCoroutine;

        // kian added awake method for part 7
        private void Awake()
        {
            // Make sure the player persists when loading new scenes
            //DontDestroyOnLoad(gameObject);
            //Debug.Log("PlayerController Awake - Player will persist between scenes");
        }

        void Start()
        {
            moveController = GetComponent<MovementController>();
            // Baseline: Hide everything at the start
            SetTool(0); 
        }

        // Master function for rendering/derendering tools
        
        public void SetTool(int toolIndex)
        {
            // Cancel any active hide timer so tools don't flicker
            if (hideToolCoroutine != null) StopCoroutine(hideToolCoroutine);

            
            if (hoeModel != null) hoeModel.SetActive(toolIndex == 1);
            if (wateringCanModel != null) wateringCanModel.SetActive(toolIndex == 2);

            
            if (toolIndex != 0)
            {
                hideToolCoroutine = StartCoroutine(HideToolsAfterDelay(5.5f));
            }
        }

        public void DigWithTool()
        {
        // For digging animation, default to hoe (toolIndex 1)
        SetTool(1);
        }

        public void SetTool()  // Matches the animation event
        {
            SetTool(1); // Redirect to your method
        }

        public class AnimationEventTester : MonoBehaviour
        {
            public void SetTool()
            {
                Debug.Log("✅ Animation event found SetTool() on AnimationEventTester!");
            }
    
            public void SetTool(int index)
            {
                Debug.Log($"✅ Animation event found SetTool({index}) on AnimationEventTester!");
            }
        }
                // Add ALL of these methods to PlayerController.cs
        // Add ALL of these different NAMES to PlayerController.cs
        public void settool() { Debug.Log("✅ settool lowercase"); DigWithTool(); }
        public void SetToolNoParam() { Debug.Log("✅ SetToolNoParam"); DigWithTool(); }
        public void Set_Tool() { Debug.Log("✅ Set_Tool"); DigWithTool(); }
        public void StartTool() { Debug.Log("✅ StartTool"); DigWithTool(); }
        public void UseTool() { Debug.Log("✅ UseTool"); DigWithTool(); }
        public void ToolEvent() { Debug.Log("✅ ToolEvent"); DigWithTool(); }
        public void DigEvent() { Debug.Log("✅ DigEvent"); DigWithTool(); }
        public void OnDig() { Debug.Log("✅ OnDig"); DigWithTool(); }
        public void ActivateTool() { Debug.Log("✅ ActivateTool"); DigWithTool(); }
        
        private IEnumerator HideToolsAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (hoeModel != null) hoeModel.SetActive(false);
            if (wateringCanModel != null) wateringCanModel.SetActive(false);
        }

        public void OnMove(InputValue inputValue)
        {
            Vector2 inputVector = inputValue.Get<Vector2>();
            
            // If the player starts walking, instantly hide tools
            if (inputVector.magnitude > 0.1f) SetTool(0);

            if (moveController) moveController.Move(inputVector);
        }

        public void OnInteract(InputValue value)
        {
            if (!value.isPressed) return;

            FarmTile tile = tileSelector ? tileSelector.GetSelectedTile() : null;
            if (tile != null && farmingManager != null)
            {
                // This triggers the animations that call SetTool(1) or SetTool(2)
                farmingManager.InteractWithTile(tile);
            }
        }

        public void OnJump(InputValue inputValue)
        {
            if (moveController && inputValue.isPressed) moveController.Jump();
        }
    }
}