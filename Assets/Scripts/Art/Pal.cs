using UnityEngine;

namespace Parking
{
    public static class Pal
    {
        public static Color32 C(string hex)
        {
            Color c;
            if (!ColorUtility.TryParseHtmlString(hex, out c)) return new Color32(255, 0, 255, 255);
            return c;
        }

        public static Color32 A(Color32 c, float a)
        {
            c.a = (byte)Mathf.Clamp(a * 255f, 0, 255);
            return c;
        }

        public static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            Color ca = a, cb = b;
            return Color.Lerp(ca, cb, t);
        }

        public static Color32 Dark(Color32 c, float f) => Lerp(c, new Color32(0, 0, 0, 255), f);
        public static Color32 Light(Color32 c, float f) => Lerp(c, new Color32(255, 255, 255, 255), f);

        public static readonly Color32 Ink = C("#0c0e14");
        public static readonly Color32 Ink2 = C("#151923");
        public static readonly Color32 Night = C("#1c212d");
        public static readonly Color32 White = C("#f4f2e6");
        public static readonly Color32 Cream = C("#e8e2cc");
        public static readonly Color32 Yellow = C("#f5c445");
        public static readonly Color32 Orange = C("#ef8b3c");
        public static readonly Color32 Red = C("#e0574f");
        public static readonly Color32 Green = C("#6fce8d");
        public static readonly Color32 Mint = C("#8ff0b4");
        public static readonly Color32 Blue = C("#5aa9e6");
        public static readonly Color32 Purple = C("#9b7bd8");
        public static readonly Color32 Gray = C("#9aa3b2");

        public static readonly Color32 Asphalt = C("#34383f");
        public static readonly Color32 AsphaltLight = C("#3b3f47");
        public static readonly Color32 AsphaltDark = C("#2c3036");
        public static readonly Color32 Paint = C("#e7e1cc");
        public static readonly Color32 PaintWorn = C("#b9b3a0");
        public static readonly Color32 Grass = C("#4b7442");
        public static readonly Color32 GrassLight = C("#578348");
        public static readonly Color32 GrassDark = C("#3d6136");
        public static readonly Color32 Concrete = C("#767b83");
        public static readonly Color32 ConcreteDark = C("#5b6068");
        public static readonly Color32 Curb = C("#c3bdae");
        public static readonly Color32 CurbDark = C("#6c6b67");
    }
}
