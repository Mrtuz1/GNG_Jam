using UnityEngine;
using KontrolluCokus.DNA;

namespace KontrolluCokus.Interaction
{
    /// <summary>
    /// DNA scanner: opens the close-up view and compares the player's DNA with the wife's hidden DNA.
    /// </summary>
    public class ScannerInteractable : Interactable
    {
        public override void Interact()
        {
            base.Interact();

            var manager = InteractionManager.Instance;
            if (manager == null || !manager.IsCloseUpActive || DNASystem.Instance == null) return;

            manager.ShowMessage("Karının DNA'sıyla karşılaştırma:\n\n" + DNASystem.Instance.BuildScanReport());
        }
    }
}
