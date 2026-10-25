using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KontrolluCokus.Inventory;

namespace KontrolluCokus.DNA
{
    /// <summary>
    /// The player's 4-gene DNA. Drugs color genes; the goal is to match the wife's hidden DNA.
    /// If any gene turns black the collapse becomes uncontrolled (game over). The antidote resets all genes.
    /// </summary>
    public class DNASystem : MonoBehaviour
    {
        public static DNASystem Instance { get; private set; }

        public const int GENE_COUNT = 4;
        public static readonly string[] GeneNames = { "A", "B", "C", "D" };

        [Header("Puzzle")]
        [Tooltip("Used as-is when randomization is off (or fails).")]
        [SerializeField] private GeneColor[] targetGenes = { GeneColor.Mor, GeneColor.Sari, GeneColor.Turuncu, GeneColor.Mavi };

        [Header("Randomization")]
        [SerializeField] private bool randomizeEachRun = true;
        [Tooltip("0 = different every run. Any other value gives the same puzzle every time (for testing).")]
        [SerializeField] private int seed = 0;
        [Tooltip("Drugs whose gene targets get shuffled. Each keeps its own color and concentrated flag.")]
        [SerializeField] private ItemData[] drugPool;

        private readonly GeneColor[] genes = new GeneColor[GENE_COUNT];
        private Dictionary<ItemData, GeneEffect[]> runtimeEffects;
        private int doses;
        private int currentSeed;

        // Seed carried over a scene reload so "Tekrar Dene" replays the same puzzle
        private static int retrySeed;

        public event Action OnDNAChanged;
        /// <summary>Fired after every injection with a readable description of what changed.</summary>
        public event Action<string> OnInjected;
        public event Action OnWon;
        public event Action OnCollapsed;

        public int Doses => doses;
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (randomizeEachRun)
                GeneratePuzzle();
        }

        private void GeneratePuzzle()
        {
            if (seed != 0) currentSeed = seed;
            else if (retrySeed != 0) currentSeed = retrySeed;
            else currentSeed = new System.Random().Next(1, int.MaxValue);
            retrySeed = 0;

            var rng = new System.Random(currentSeed);
            var puzzle = PuzzleGenerator.Generate(drugPool ?? new ItemData[0], rng);
            if (puzzle == null)
            {
                Debug.LogWarning("[DNA] Could not generate a random puzzle, using the fixed one.");
                return;
            }

            runtimeEffects = puzzle.effects;
            targetGenes = puzzle.target;

            // Spoiler for testing
            var sb = new StringBuilder($"[DNA] Seed {currentSeed} | Target: ");
            for (int i = 0; i < GENE_COUNT; i++) sb.Append(GeneNames[i]).Append('=').Append(targetGenes[i]).Append(' ');
            sb.Append("| Solution: ");
            foreach (var d in puzzle.solution) sb.Append(d.itemName).Append(" -> ");
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Call before reloading the scene to keep the same puzzle (retry after collapse).
        /// </summary>
        public void KeepPuzzleForRetry()
        {
            retrySeed = currentSeed;
        }

        public GeneColor GetGene(int index) => genes[index];

        /// <summary>
        /// Inject a drug into the player. Returns false if nothing happened.
        /// </summary>
        public bool Inject(ItemData drug)
        {
            if (IsGameOver || drug == null) return false;

            var report = new StringBuilder(drug.itemName).Append(": ");
            if (drug.resetsDNA)
            {
                for (int i = 0; i < GENE_COUNT; i++) genes[i] = GeneColor.Gri;
                report.Append("tüm genler griye döndü");
            }

            GeneEffect[] effects = drug.geneEffects;
            if (runtimeEffects != null && runtimeEffects.TryGetValue(drug, out var randomized))
                effects = randomized;

            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    if (effect.gene < 0 || effect.gene >= GENE_COUNT) continue;
                    GeneColor before = genes[effect.gene];
                    genes[effect.gene] = effect.overwrite ? effect.color : GeneColors.Mix(before, effect.color);
                    GeneColor after = genes[effect.gene];

                    if (report[report.Length - 2] != ':') report.Append(",  ");
                    report.Append($"Gen {GeneNames[effect.gene]} <color={GeneColors.ToHex(after)}>{GeneColors.DisplayName(after)}</color>");
                    if (effect.overwrite && before != GeneColor.Gri && before != after) report.Append(" (konsantre: üstüne yazdı)");
                    else if (before == after) report.Append(" (değişmedi)");
                    else if (after == GeneColor.Siyah) report.Append(" (BOZULDU)");
                }
            }

            doses++;
            Debug.Log($"[DNA] Injected '{drug.itemName}' -> {DebugString()} (dose {doses})");
            OnDNAChanged?.Invoke();
            OnInjected?.Invoke(report.ToString());

            if (MatchesTarget())
            {
                IsGameOver = true;
                OnWon?.Invoke();
            }
            else if (HasRuinedGene())
            {
                IsGameOver = true;
                OnCollapsed?.Invoke();
            }
            return true;
        }

        public bool HasRuinedGene()
        {
            for (int i = 0; i < GENE_COUNT; i++)
                if (genes[i] == GeneColor.Siyah) return true;
            return false;
        }

        /// <summary>
        /// 0..1, how many genes are mixed (one more wrong color away from turning black).
        /// </summary>
        public float Instability
        {
            get
            {
                int mixed = 0;
                for (int i = 0; i < GENE_COUNT; i++)
                    if (genes[i] != GeneColor.Gri && !GeneColors.IsPrimary(genes[i])) mixed++;
                return (float)mixed / GENE_COUNT;
            }
        }

        public bool MatchesTarget()
        {
            for (int i = 0; i < GENE_COUNT; i++)
                if (genes[i] != targetGenes[i]) return false;
            return true;
        }

        /// <summary>
        /// Mastermind-style report: green = right color, yellow = color is in another gene of the target, red = not in target.
        /// </summary>
        public string BuildScanReport()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < GENE_COUNT; i++)
            {
                GeneColor c = genes[i];
                string verdict;
                if (c == GeneColor.Gri)
                    verdict = "<color=#999999>boş</color>";
                else if (c == targetGenes[i])
                    verdict = "<color=#44DD55>UYUMLU</color>";
                else if (Array.IndexOf(targetGenes, c) >= 0)
                    verdict = "<color=#EEDD33>başka bir gende var</color>";
                else
                    verdict = "<color=#EE4444>uyumsuz</color>";

                sb.Append($"Gen {GeneNames[i]}: <color={GeneColors.ToHex(c)}>{GeneColors.DisplayName(c)}</color>  →  {verdict}");
                if (i < GENE_COUNT - 1) sb.Append('\n');
            }
            return sb.ToString();
        }

        private string DebugString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < GENE_COUNT; i++) sb.Append(GeneNames[i]).Append('=').Append(genes[i]).Append(' ');
            return sb.ToString();
        }
    }
}
