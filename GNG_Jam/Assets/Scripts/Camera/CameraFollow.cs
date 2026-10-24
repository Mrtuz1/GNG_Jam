using UnityEngine;

namespace KontrolluCokus
{
    /// <summary>
    /// Smooth camera follow for top-down 2D.
    /// Follows the player with adjustable smoothing.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Bounds (Optional)")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        private void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            // Clamp to bounds if enabled
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothed;
        }

        /// <summary>
        /// Jump straight to the target without smoothing (e.g. after a room change).
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = target.position + offset;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
