using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using KontrolluCokus.Interaction;
using KontrolluCokus.Inventory;

namespace KontrolluCokus
{
    /// <summary>
    /// Manages the close-up interaction view.
    /// When player interacts with an object (e.g., cabinet), it shows a full-screen close-up.
    /// Containers list their items here so the player can take them into the inventory.
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance { get; private set; }

        [Header("Close-Up View UI")]
        [SerializeField] private GameObject closeUpPanel;
        [SerializeField] private Image closeUpImage;
        [SerializeField] private Button backButton;
        [SerializeField] private Text objectNameText;

        [Header("Container Items")]
        [SerializeField] private Transform itemListParent;
        [SerializeField] private Button itemButtonTemplate; // inactive template, cloned per item
        [SerializeField] private Text infoText;

        private Interactable currentInteractable;
        private bool isCloseUpActive;
        private readonly List<Button> spawnedItemButtons = new List<Button>();

        public bool IsCloseUpActive => isCloseUpActive;
        public int LastClosedFrame { get; private set; } = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (closeUpPanel != null)
                closeUpPanel.SetActive(false);

            if (itemButtonTemplate != null)
                itemButtonTemplate.gameObject.SetActive(false);

            if (backButton != null)
                backButton.onClick.AddListener(CloseCloseUpView);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (isCloseUpActive && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseCloseUpView();
            }
        }

        /// <summary>
        /// Open the close-up view for an interactable object.
        /// </summary>
        public void OpenCloseUpView(Interactable interactable)
        {
            if (isCloseUpActive) return;

            currentInteractable = interactable;
            isCloseUpActive = true;

            if (closeUpPanel != null)
                closeUpPanel.SetActive(true);

            if (closeUpImage != null)
            {
                // Real art if assigned, otherwise tint the placeholder with the object's color
                if (interactable.CloseUpSprite != null)
                {
                    closeUpImage.sprite = interactable.CloseUpSprite;
                    closeUpImage.color = Color.white;
                }
                else
                {
                    closeUpImage.color = interactable.BaseColor;
                }
            }

            if (objectNameText != null)
                objectNameText.text = interactable.InteractableName;

            SetInfo("");
            RebuildItemList();

            var player = FindAnyObjectByType<KontrolluCokus.Player.PlayerController>();
            if (player != null)
                player.SetCanMove(false);
        }

        /// <summary>
        /// Close the close-up view and return to normal gameplay.
        /// </summary>
        public void CloseCloseUpView()
        {
            if (!isCloseUpActive) return;

            isCloseUpActive = false;
            currentInteractable = null;
            LastClosedFrame = Time.frameCount;
            ClearItemList();

            if (closeUpPanel != null)
                closeUpPanel.SetActive(false);

            var player = FindAnyObjectByType<KontrolluCokus.Player.PlayerController>();
            if (player != null)
                player.SetCanMove(true);
        }

        public Interactable GetCurrentInteractable()
        {
            return currentInteractable;
        }

        private void RebuildItemList()
        {
            ClearItemList();

            var container = currentInteractable as ContainerInteractable;
            if (container == null || itemButtonTemplate == null || itemListParent == null) return;

            var items = container.ContainedItems;
            bool anyItem = false;
            for (int i = 0; i < (items?.Length ?? 0); i++)
            {
                if (items[i] == null) continue;
                anyItem = true;

                int index = i;
                Button button = Instantiate(itemButtonTemplate, itemListParent);
                button.gameObject.SetActive(true);
                var label = button.GetComponentInChildren<Text>();
                if (label != null) label.text = "Al: " + items[i].itemName;
                button.onClick.AddListener(() => TakeFromContainer(container, index));
                spawnedItemButtons.Add(button);
            }

            if (!anyItem) SetInfo("Burada alınacak bir şey yok.");
        }

        private void TakeFromContainer(ContainerInteractable container, int index)
        {
            var item = container.ContainedItems[index];
            if (item == null || InventorySystem.Instance == null) return;

            if (!InventorySystem.Instance.TryAddItem(item))
            {
                SetInfo("Envanter dolu!");
                return;
            }

            container.TakeItem(index);
            RebuildItemList();
            SetInfo(item.itemName + " alındı.");
        }

        private void ClearItemList()
        {
            foreach (var b in spawnedItemButtons)
                if (b != null)
                {
                    b.gameObject.SetActive(false); // Destroy is deferred to end of frame
                    Destroy(b.gameObject);
                }
            spawnedItemButtons.Clear();
        }

        private void SetInfo(string message)
        {
            if (infoText != null) infoText.text = message;
        }
    }
}
