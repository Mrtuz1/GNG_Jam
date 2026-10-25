using UnityEngine;

namespace KontrolluCokus.DNA
{
    /// <summary>
    /// Colors a gene can have. Primaries mix into secondaries, a third color ruins the gene.
    /// </summary>
    public enum GeneColor
    {
        Gri,
        Kirmizi,
        Mavi,
        Sari,
        Mor,
        Turuncu,
        Yesil,
        Siyah
    }

    public static class GeneColors
    {
        public static bool IsPrimary(GeneColor c)
        {
            return c == GeneColor.Kirmizi || c == GeneColor.Mavi || c == GeneColor.Sari;
        }

        /// <summary>
        /// Result of adding a color on top of the current one.
        /// Grey takes the new color, two different primaries mix, anything else turns black.
        /// </summary>
        public static GeneColor Mix(GeneColor current, GeneColor added)
        {
            if (current == GeneColor.Gri) return added;
            if (current == added) return current;

            if (IsPrimary(current) && IsPrimary(added))
            {
                bool r = current == GeneColor.Kirmizi || added == GeneColor.Kirmizi;
                bool b = current == GeneColor.Mavi || added == GeneColor.Mavi;
                bool y = current == GeneColor.Sari || added == GeneColor.Sari;
                if (r && b) return GeneColor.Mor;
                if (r && y) return GeneColor.Turuncu;
                if (b && y) return GeneColor.Yesil;
            }

            return GeneColor.Siyah;
        }

        public static Color ToColor(GeneColor c)
        {
            switch (c)
            {
                case GeneColor.Kirmizi: return new Color(0.9f, 0.2f, 0.2f);
                case GeneColor.Mavi: return new Color(0.2f, 0.4f, 0.95f);
                case GeneColor.Sari: return new Color(0.95f, 0.85f, 0.2f);
                case GeneColor.Mor: return new Color(0.6f, 0.25f, 0.85f);
                case GeneColor.Turuncu: return new Color(0.95f, 0.55f, 0.15f);
                case GeneColor.Yesil: return new Color(0.25f, 0.8f, 0.3f);
                case GeneColor.Siyah: return new Color(0.05f, 0.05f, 0.05f);
                default: return new Color(0.5f, 0.5f, 0.5f);
            }
        }

        public static string DisplayName(GeneColor c)
        {
            switch (c)
            {
                case GeneColor.Kirmizi: return "Kırmızı";
                case GeneColor.Sari: return "Sarı";
                case GeneColor.Yesil: return "Yeşil";
                default: return c.ToString();
            }
        }

        public static string ToHex(GeneColor c)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(ToColor(c));
        }
    }
}
