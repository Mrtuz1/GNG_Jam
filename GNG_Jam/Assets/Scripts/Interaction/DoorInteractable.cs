using UnityEngine;

namespace KontrolluCokus.Interaction
{
    /// <summary>
    /// A door that moves the player to a spawn point in another room.
    /// </summary>
    public class DoorInteractable : Interactable
    {
        [Header("Door Settings")]
        [SerializeField] private Transform targetSpawnPoint;

        public override void Interact()
        {
            if (!CanInteract || targetSpawnPoint == null) return;

            OnInteracted?.Invoke();

            var player = FindAnyObjectByType<Player.PlayerController>();
            if (player != null)
                player.TeleportTo(targetSpawnPoint.position);

            var cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null)
                cam.SnapToTarget();
        }
    }
}
