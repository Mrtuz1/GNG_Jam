using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KontrolluCokus.DNA
{
    /// <summary>
    /// HUD for the player's DNA, dose counter, collapse distortion and the win / collapse screens.
    /// </summary>
    public class DNAUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private Image[] geneImages = new Image[DNASystem.GENE_COUNT];
        [SerializeField] private Text doseText;
        [Tooltip("Shows what the last injection did to the genes.")]
        [SerializeField] private Text effectText;
        [SerializeField] private float effectTextDuration = 6f;

        [Header("Collapse Distortion")]
        [SerializeField] private Image distortionOverlay;
        [SerializeField] private float maxOverlayAlpha = 0.45f;

        [Header("End Screen")]
        [SerializeField] private GameObject endPanel;
        [SerializeField] private Text endTitle;
        [SerializeField] private Text endBody;
        [SerializeField] private Button restartButton;

        private float pulseTimer;
        private float effectTextTimer;

        private void Start()
        {
            var dna = DNASystem.Instance;
            if (dna != null)
            {
                dna.OnDNAChanged += Refresh;
                dna.OnInjected += ShowEffect;
                dna.OnWon += ShowWin;
                dna.OnCollapsed += ShowCollapse;
            }

            if (endPanel != null) endPanel.SetActive(false);
            if (restartButton != null)
                restartButton.onClick.AddListener(() =>
                {
                    DNASystem.Instance?.KeepPuzzleForRetry();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                });

            if (effectText != null) effectText.text = "";
            Refresh();
        }

        private void OnDestroy()
        {
            var dna = DNASystem.Instance;
            if (dna != null)
            {
                dna.OnDNAChanged -= Refresh;
                dna.OnInjected -= ShowEffect;
                dna.OnWon -= ShowWin;
                dna.OnCollapsed -= ShowCollapse;
            }
        }

        private void Update()
        {
            if (effectTextTimer > 0f)
            {
                effectTextTimer -= Time.deltaTime;
                if (effectTextTimer <= 0f && effectText != null) effectText.text = "";
            }

            // Overlay pulses harder the more genes are mixed (close to turning black)
            var dna = DNASystem.Instance;
            if (distortionOverlay == null || dna == null) return;

            float t = dna.Instability;
            pulseTimer += Time.deltaTime * (1f + t * 4f);
            float pulse = 1f + Mathf.Sin(pulseTimer) * 0.25f * t;
            var c = distortionOverlay.color;
            c.a = Mathf.Clamp01(t * maxOverlayAlpha * pulse);
            distortionOverlay.color = c;
        }

        private void Refresh()
        {
            var dna = DNASystem.Instance;
            if (dna == null) return;

            for (int i = 0; i < geneImages.Length && i < DNASystem.GENE_COUNT; i++)
            {
                if (geneImages[i] != null)
                    geneImages[i].color = GeneColors.ToColor(dna.GetGene(i));
            }

            if (doseText != null)
                doseText.text = $"Doz: {dna.Doses}";
        }

        private void ShowEffect(string message)
        {
            if (effectText == null) return;
            effectText.text = message;
            effectTextTimer = effectTextDuration;
        }

        private void ShowWin()
        {
            ShowEnd("Artık aynı dünyadayız.", "DNA'n karınınkiyle eşleşti. Çöküşü kontrol ettin.");
        }

        private void ShowCollapse()
        {
            ShowEnd("Kontrolsüz çöküş", "Genlerinden biri siyaha döndü. Vücudun bu karışımı kaldıramadı.");
        }

        private void ShowEnd(string title, string body)
        {
            InteractionManager.Instance?.CloseCloseUpView();

            var player = FindAnyObjectByType<Player.PlayerController>();
            if (player != null) player.SetCanMove(false);

            if (endTitle != null) endTitle.text = title;
            if (endBody != null) endBody.text = body;
            if (endPanel != null) endPanel.SetActive(true);
        }
    }
}
