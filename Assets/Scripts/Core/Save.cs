using UnityEngine;

namespace Parking
{
    public static class Save
    {
        const string P = "parkit_";

        public static int Unlocked = 1;
        public static readonly float[] Best = new float[LevelCatalog.Count];
        public static readonly int[] Stars = new int[LevelCatalog.Count];
        public static bool Muted;

        public static void Load()
        {
            Unlocked = Mathf.Clamp(PlayerPrefs.GetInt(P + "unlocked", 1), 1, LevelCatalog.Count);
            for (int i = 0; i < Best.Length; i++)
            {
                Best[i] = PlayerPrefs.GetFloat(P + "best_" + i, -1f);
                Stars[i] = PlayerPrefs.GetInt(P + "stars_" + i, 0);
            }
            Muted = PlayerPrefs.GetInt(P + "muted", 0) == 1;
        }

        public static void SaveAll()
        {
            PlayerPrefs.SetInt(P + "unlocked", Unlocked);
            for (int i = 0; i < Best.Length; i++)
            {
                PlayerPrefs.SetFloat(P + "best_" + i, Best[i]);
                PlayerPrefs.SetInt(P + "stars_" + i, Stars[i]);
            }
            PlayerPrefs.SetInt(P + "muted", Muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            Unlocked = 1;
            for (int i = 0; i < Best.Length; i++) { Best[i] = -1f; Stars[i] = 0; }
            SaveAll();
        }

        public static int ClearedCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Stars.Length; i++) if (Stars[i] > 0) n++;
                return n;
            }
        }

        public static int TotalStars
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Stars.Length; i++) n += Stars[i];
                return n;
            }
        }

        public static int Record(int level, float time, int bumps)
        {
            var def = LevelCatalog.Get(level);
            int stars = 1;
            if (bumps == 0 && time <= def.Par) stars = 3;
            else if (bumps <= 2 && time <= def.Par * 1.65f) stars = 2;

            if (Best[level] < 0f || time < Best[level]) Best[level] = time;
            if (stars > Stars[level]) Stars[level] = stars;
            if (level + 1 < LevelCatalog.Count) Unlocked = Mathf.Max(Unlocked, level + 2);
            SaveAll();
            return stars;
        }
    }
}
