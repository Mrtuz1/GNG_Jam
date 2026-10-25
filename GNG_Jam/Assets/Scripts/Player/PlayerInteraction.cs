using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KontrolluCokus.Player
{
    /// <summary>
    /// Handles player interaction with nearby Interactable objects.
    /// Detects objects in range, highlights the closest one, and triggers interaction on click/E.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 1.8f;
        [SerializeField] private LayerMask interactableLayer;

        private Interactable currentTarget;
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            // Auto-detect Interactable layer if not assigned in inspector
            if (interactableLayer.value == 0)
            {
                int layer = LayerMask.NameToLayer("Interactable");
                if (layer != -1)
                {
                    interactableLayer = 1 << layer;
                }
                else
                {
                    interactableLayer = ~0;
                }
            }
        }

        private void Update()
        {
            FindClosestInteractable();
            HandleInteractionInput();
        }

        private void FindClosestInteractable()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

            Interactable closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                Interactable interactable = hit.GetComponent<Interactable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            // Update highlight state
            if (currentTarget != closest)
            {
                if (currentTarget != null)
                    currentTarget.SetHighlight(false);

                currentTarget = closest;

                if (currentTarget != null)
                    currentTarget.SetHighlight(true);
            }
        }

        private void HandleInteractionInput()
        {
            if (currentTarget == null) return;

            if (DNA.DNASystem.Instance != null && DNA.DNASystem.Instance.IsGameOver) return;

            // No world interaction while the close-up view is open (or on the frame it closed)
            var manager = InteractionManager.Instance;
            if (manager != null && (manager.IsCloseUpActive || manager.LastClosedFrame == Time.frameCount)) return;

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool ePressed = keyboard != null && keyboard.eKey.wasPressedThisFrame;
            bool mouseClicked = mouse != null && mouse.leftButton.wasPressedThisFrame && IsMouseOverTarget();

            if (ePressed || mouseClicked)
            {
                currentTarget.Interact();
            }
        }

        private bool IsMouseOverTarget()
        {
            if (currentTarget == null) return false;

            var mouse = Mouse.current;
            if (mouse == null) return false;

            // Ignore clicks that land on UI (inventory slots etc.)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false;

            Camera mainCam = Camera.main;
            if (mainCam == null) return false;

            Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouse.position.ReadValue());
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos, interactableLayer);

            return hitCollider != null && hitCollider.GetComponent<Interactable>() == currentTarget;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
