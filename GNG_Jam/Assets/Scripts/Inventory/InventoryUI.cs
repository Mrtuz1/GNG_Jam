using UnityEngine;
using UnityEngine.UI;

namespace KontrolluCokus.Inventory
{
    /// <summary>
    /// UI display for the 2-slot inventory.
    /// Shows item icons at the bottom of the screen.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Slot References")]
        [SerializeField] private Image slot1Icon;
        [SerializeField] private Image slot2Icon;
        [SerializeField] private Button slot1Button;
        [SerializeField] private Button slot2Button;
        [SerializeField] private Text slot1Label;
        [SerializeField] private Text slot2Label;
        [SerializeField] private Sprite emptySlotSprite;

        [Header("Visual Settings")]
        [SerializeField] private Color emptySlotColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] private Color filledSlotColor = Color.white;

        private void Start()
        {
            if (InventorySystem.Instance != null)
                InventorySystem.Instance.OnInventoryChanged += UpdateSlotUI;

            if (slot1Button != null)
                slot1Button.onClick.AddListener(() => OnSlotClicked(0));
            if (slot2Button != null)
                slot2Button.onClick.AddListener(() => OnSlotClicked(1));

            RefreshAllSlots();
        }

        private void OnDestroy()
        {
            if (InventorySystem.Instance != null)
                InventorySystem.Instance.OnInventoryChanged -= UpdateSlotUI;
        }

        private void UpdateSlotUI(int slotIndex, ItemData item)
        {
            Image icon = slotIndex == 0 ? slot1Icon : slot2Icon;
            if (icon == null) return;

            Text label = slotIndex == 0 ? slot1Label : slot2Label;
            if (label != null)
                label.text = item != null ? item.itemName : "Boş";

            if (item != null)
            {
                icon.sprite = item.icon != null ? item.icon : emptySlotSprite;
                icon.color = item.icon != null ? filledSlotColor : item.placeholderColor;
            }
            else
            {
                icon.sprite = emptySlotSprite;
                icon.color = emptySlotColor;
            }
        }

        private void RefreshAllSlots()
        {
            if (InventorySystem.Instance == null) return;

            for (int i = 0; i < InventorySystem.MAX_SLOTS; i++)
            {
                UpdateSlotUI(i, InventorySystem.Instance.GetItem(i));
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (InventorySystem.Instance == null) return;

            var item = InventorySystem.Instance.GetItem(slotIndex);
            if (item == null) return;

            // Drugs are injected into the player's DNA
            if (item.itemType == ItemType.Medicine && DNA.DNASystem.Instance != null)
            {
                if (!DNA.DNASystem.Instance.Inject(item)) return;
            }

            // Use item (if consumable, it gets removed)
            var usedItem = InventorySystem.Instance.UseItem(slotIndex);
            if (usedItem != null)
            {
                Debug.Log($"[InventoryUI] Player used: {usedItem.itemName}");
                // TODO: Trigger item effect via usedItem.effectId (drug injection, etc.)
            }
        }
    }
}