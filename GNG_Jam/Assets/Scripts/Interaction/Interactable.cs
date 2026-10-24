using UnityEngine;
using UnityEngine.Events;

namespace KontrolluCokus
{
    /// <summary>
    /// Base class for all interactable objects in the game.
    /// Provides highlight functionality and interaction callbacks.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [SerializeField] private string interactableName = "Object";
        [SerializeField] private string interactionPrompt = "Interact";
        [SerializeField] private bool opensCloseUp = true;
        [SerializeField] private Sprite closeUpSprite; // The sprite shown in close-up view (placeholder used if empty)

        [Header("Events")]
        public UnityEvent OnInteracted;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isHighlighted;

        public bool CanInteract { get; set; } = true;
        public string InteractableName => interactableName;
        public string InteractionPrompt => interactionPrompt;
        public Sprite CloseUpSprite => closeUpSprite;
        public Color BaseColor => originalColor;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;

            // Ensure collider is a trigger for detection
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.isTrigger = false; // Keep as collider, detection is via OverlapCircle
        }

        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        public virtual void Interact()
        {
            if (!CanInteract) return;

            OnInteracted?.Invoke();

            if (opensCloseUp)
            {
                InteractionManager.Instance?.OpenCloseUpView(this);
            }
        }

        /// <summary>
        /// Highlight or unhighlight this object.
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            isHighlighted = highlight;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlight
                    ? new Color(originalColor.r + 0.3f, originalColor.g + 0.3f, originalColor.b + 0.1f, originalColor.a)
                    : originalColor;
            }
        }
    }
}
