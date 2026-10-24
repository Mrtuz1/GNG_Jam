using System;
using UnityEngine;

namespace KontrolluCokus.Inventory
{
    /// <summary>
    /// Simple 2-slot inventory system.
    /// The player can hold at most 2 items at a time.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        public const int MAX_SLOTS = 2;

        private ItemData[] slots = new ItemData[MAX_SLOTS];

        /// <summary>
        /// Fired when any inventory slot changes. (slotIndex, newItem)
        /// </summary>
        public event Action<int, ItemData> OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Try to add an item to the first available slot.
        /// Returns true if successfully added.
        /// </summary>
        public bool TryAddItem(ItemData item)
        {
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = item;
                    OnInventoryChanged?.Invoke(i, item);
                    Debug.Log($"[Inventory] Added '{item.itemName}' to slot {i}");
                    return true;
                }
            }
            Debug.Log("[Inventory] No empty slots!");
            return false;
        }

        /// <summary>
        /// Remove item from a specific slot.
        /// </summary>
        public ItemData RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return null;

            var item = slots[slotIndex];
            slots[slotIndex] = null;
            OnInventoryChanged?.Invoke(slotIndex, null);
            return item;
        }

        /// <summary>
        /// Get item in a slot (null if empty).
        /// </summary>
        public ItemData GetItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return null;
            return slots[slotIndex];
        }

        /// <summary>
        /// Use (consume) an item from a slot.
        /// Returns the item that was used, or null if slot was empty.
        /// </summary>
        public ItemData UseItem(int slotIndex)
        {
            var item = GetItem(slotIndex);
            if (item == null) return null;

            if (item.isConsumable)
            {
                RemoveItem(slotIndex);
                Debug.Log($"[Inventory] Used '{item.itemName}' from slot {slotIndex}");
            }

            return item;
        }

        public bool HasEmptySlot()
        {
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (slots[i] == null) return true;
            }
            return false;
        }

        public bool IsFull()
        {
            return !HasEmptySlot();
        }
    }
}