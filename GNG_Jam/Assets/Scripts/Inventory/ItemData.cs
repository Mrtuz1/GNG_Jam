using UnityEngine;

namespace KontrolluCokus.Inventory
{
    /// <summary>
    /// ScriptableObject defining an item in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Kontrollu Cokus/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info")]
        public string itemName = "New Item";
        [TextArea(2, 4)]
        public string description = "An item.";
        public Sprite icon;
        [Tooltip("Color used for the slot while there is no icon art yet.")]
        public Color placeholderColor = Color.white;

        [Header("Item Properties")]
        public ItemType itemType = ItemType.Generic;
        public bool isConsumable = false;

        [Header("DNA Effect (for drugs)")]
        [Tooltip("Genes this drug colors when injected, applied in order.")]
        public DNA.GeneEffect[] geneEffects;
        [Tooltip("Antidote: turns every gene back to grey before applying effects.")]
        public bool resetsDNA = false;

        [Header("Effect (for drugs/medicine)")]
        [Tooltip("The visual/mental effect ID this item triggers when used.")]
        public string effectId = "";
    }

    public enum ItemType
    {
        Generic,
        Medicine,
        Key,
        Note,
        Tool
    }
}

namespace KontrolluCokus.DNA
{
    [System.Serializable]
    public struct GeneEffect
    {
        [Range(0, DNASystem.GENE_COUNT - 1)] public int gene;
        public GeneColor color;
        [Tooltip("Concentrated: replaces the gene color instead of mixing with it.")]
        public bool overwrite;
    }
}
