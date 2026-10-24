using UnityEngine;
using UnityEngine.InputSystem;

namespace KontrolluCokus.Player
{
    /// <summary>
    /// Top-down 2D player movement controller.
    /// WASD/Arrow keys for movement. Uses new Input System.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4f;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private bool canMove = true;

        // Last facing direction for interaction purposes
        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            if (!canMove)
            {
                moveInput = Vector2.zero;
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            moveInput = Vector2.zero;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                moveInput.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                moveInput.y -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                moveInput.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                moveInput.x -= 1f;

            // Normalize diagonal movement
            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();

            // Update facing direction (only when moving)
            if (moveInput.sqrMagnitude > 0.01f)
                FacingDirection = moveInput.normalized;
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        /// <summary>
        /// Instantly move the player (used by doors to change rooms).
        /// </summary>
        public void TeleportTo(Vector2 position)
        {
            rb.position = position;
            transform.position = position;
            rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// Enable or disable player movement (used during interactions/menus).
        /// </summary>
        public void SetCanMove(bool value)
        {
            canMove = value;
            if (!canMove)
            {
                rb.linearVelocity = Vector2.zero;
                moveInput = Vector2.zero;
            }
        }
    }
}
