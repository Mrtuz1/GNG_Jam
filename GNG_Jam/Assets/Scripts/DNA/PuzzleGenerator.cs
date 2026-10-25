using System.Collections.Generic;
using KontrolluCokus.Inventory;

namespace KontrolluCokus.DNA
{
    /// <summary>
    /// Builds a random DNA puzzle: which genes each drug colors, and the wife's target DNA.
    /// The target is produced by simulating a random drug sequence from grey, so every puzzle
    /// has at least one solution that never turns a gene black.
    /// </summary>
    public static class PuzzleGenerator
    {
        public class Result
        {
            public Dictionary<ItemData, GeneEffect[]> effects;
            public GeneColor[] target;
            public List<ItemData> solution;
        }

        private const int MaxAttempts = 5000;

        /// <summary>
        /// Each drug keeps its own color and concentrated flag (from its asset); only the genes it hits are randomized.
        /// Returns null if no valid puzzle was found.
        /// </summary>
        public static Result Generate(ItemData[] drugs, System.Random rng)
        {
            var pool = new List<ItemData>();
            foreach (var d in drugs)
                if (d != null && d.geneEffects != null && d.geneEffects.Length > 0) pool.Add(d);
            if (pool.Count < 4) return null;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var effects = new Dictionary<ItemData, GeneEffect[]>();
                foreach (var drug in pool)
                    effects[drug] = RandomEffects(drug, rng);

                int length = rng.Next(4, 6); // 4 or 5 doses
                var solution = PickDistinct(pool, length, rng);

                var target = Simulate(solution, effects);
                if (target == null || !IsInteresting(target)) continue;
                if (!OrderMatters(solution, effects, target)) continue;

                return new Result { effects = effects, target = target, solution = solution };
            }
            return null;
        }

        private static GeneEffect[] RandomEffects(ItemData drug, System.Random rng)
        {
            var template = drug.geneEffects[0];
            int count = rng.Next(0, 2) == 0 ? 1 : 2;
            int first = rng.Next(DNASystem.GENE_COUNT);
            var result = new List<GeneEffect> { new GeneEffect { gene = first, color = template.color, overwrite = template.overwrite } };
            if (count == 2)
            {
                int second = (first + 1 + rng.Next(DNASystem.GENE_COUNT - 1)) % DNASystem.GENE_COUNT;
                result.Add(new GeneEffect { gene = second, color = template.color, overwrite = template.overwrite });
            }
            return result.ToArray();
        }

        private static List<ItemData> PickDistinct(List<ItemData> pool, int count, System.Random rng)
        {
            var copy = new List<ItemData>(pool);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = copy[i]; copy[i] = copy[j]; copy[j] = tmp;
            }
            return copy.GetRange(0, count);
        }

        /// <summary>
        /// Final DNA after injecting the sequence from all-grey, or null if any gene turned black.
        /// </summary>
        public static GeneColor[] Simulate(IList<ItemData> sequence, Dictionary<ItemData, GeneEffect[]> effects)
        {
            var genes = new GeneColor[DNASystem.GENE_COUNT];
            foreach (var drug in sequence)
            {
                foreach (var e in effects[drug])
                {
                    genes[e.gene] = e.overwrite ? e.color : GeneColors.Mix(genes[e.gene], e.color);
                    if (genes[e.gene] == GeneColor.Siyah) return null;
                }
            }
            return genes;
        }

        // Every gene colored, at least two mixed colors so the player has to combine drugs
        private static bool IsInteresting(GeneColor[] target)
        {
            int mixed = 0;
            foreach (var g in target)
            {
                if (g == GeneColor.Gri) return false;
                if (!GeneColors.IsPrimary(g)) mixed++;
            }
            return mixed >= 2;
        }

        // At least one other ordering of the same drugs must fail, so the order is part of the puzzle
        private static bool OrderMatters(List<ItemData> solution, Dictionary<ItemData, GeneEffect[]> effects, GeneColor[] target)
        {
            foreach (var perm in Permutations(solution, 0))
            {
                var result = Simulate(perm, effects);
                if (result == null || !Same(result, target)) return true;
            }
            return false;
        }

        private static IEnumerable<List<ItemData>> Permutations(List<ItemData> list, int k)
        {
            if (k == list.Count - 1)
            {
                yield return new List<ItemData>(list);
                yield break;
            }
            for (int i = k; i < list.Count; i++)
            {
                var copy = new List<ItemData>(list);
                var tmp = copy[k]; copy[k] = copy[i]; copy[i] = tmp;
                foreach (var p in Permutations(copy, k + 1))
                    yield return p;
            }
        }

        private static bool Same(GeneColor[] a, GeneColor[] b)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
