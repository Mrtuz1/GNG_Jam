using UnityEngine;

namespace KontrolluCokus.Interaction
{
    /// <summary>
    /// An interactable container (cabinet, drawer, shelf, etc.)
    /// that can hold items the player can pick up.
    /// </summary>
    public class ContainerInteractable : Interactable
    {
        [Header("Container Settings")]
        [SerializeField] private Inventory.ItemData[] containedItems;

        private bool hasBeenLooted;

        public Inventory.ItemData[] ContainedItems => containedItems;
        public bool HasBeenLooted => hasBeenLooted;

        public override void Interact()
        {
            base.Interact();
            // Container-specific logic will be handled by the close-up view UI
        }

        /// <summary>
        /// Remove an item from this container (after player picks it up).
        /// </summary>
        public Inventory.ItemData TakeItem(int index)
        {
            if (containedItems == null || index < 0 || index >= containedItems.Length)
                return null;

            var item = containedItems[index];
            containedItems[index] = null;

            // Check if all items have been taken
            bool allTaken = true;
            foreach (var i in containedItems)
            {
                if (i != null) { allTaken = false; break; }
            }
            if (allTaken) hasBeenLooted = true;

            return item;
        }
    }
}