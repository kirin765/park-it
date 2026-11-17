using System.Collections.Generic;
using UnityEngine;

namespace Parking
{
    public static class Fmt
    {
        public static string Time(float t)
        {
            if (t < 0f) return "--:--";
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            int d = Mathf.FloorToInt(t * 10f) % 10;
            return m + ":" + s.ToString("00") + "." + d;
        }

        public static string Clock(float t)
        {
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            return m + ":" + s.ToString("00");
        }

        public static string Stars(int n)
        {
            return new string('*', Mathf.Clamp(n, 0, 3));
        }
    }

    static class MenuArt
    {
        public static void Backdrop(UiScreen s, float dim, bool cars = true)
        {
            var def = LevelCatalog.Get(0);
            var g = UiKit.RawImage(s.Root, GroundPainter.Paint(def, 3), Color.white, -200, "menu_ground");
            g.transform.localPosition = Vector3.zero;

            if (cars)
            {
                foreach (var c in def.Cars)
                {
                    var sr = UiKit.RawImage(s.Root, Art.Car(c.body, false), Color.white, 300 - Mathf.RoundToInt(c.y * 8f), "menu_car");
                    sr.transform.localPosition = new Vector3(c.x, c.y, 0f);
                    sr.transform.localRotation = Quaternion.Euler(0f, 0f, -c.ang);
                }
            }

            UiKit.Dim(s.Root, new Color(0.02f, 0.03f, 0.055f, dim), 1000);

            var v = UiKit.RawImage(s.Root, Art.VignetteSprite, new Color(1f, 1f, 1f, 0.95f), 1001, "vignette");
            v.transform.localScale = new Vector3(GroundPainter.WorldW / 4f, GroundPainter.WorldH / 7.125f, 1f);
        }

        public static void Stars(Transform parent, int filled, Vector2 pos, float size, float spacing, int order)
        {
            for (int i = 0; i < 3; i++)
            {
                UiKit.Image(parent, new Vector2(pos.x + (i - 1) * spacing, pos.y), new Vector2(size, size),
                    Art.Star, i < filled ? (Color)Pal.Yellow : new Color(0.28f, 0.32f, 0.4f, 1f), order);
            }
        }
    }

    // --------------------------------------------------------------------------- title
    public class TitleScreen : UiScreen
    {
        Transform _titleCar;
        PixelText _blink;
        float _t;

        static readonly Color Gray = new Color(0.72f, 0.76f, 0.83f);

        public override void Build()
        {
            MenuArt.Backdrop(this, 0.58f, false);

            UiKit.Text(Root, "PARK", new Vector2(0f, 7.05f), 5, Pal.White, 0.5f, 0.5f, 2100, true);
            UiKit.Text(Root, "IT!", new Vector2(0f, 4.68f), 5, Pal.Yellow, 0.5f, 0.5f, 2100, true);
            UiKit.Text(Root, "PIXEL PARKING", new Vector2(0f, 3.0f), 2, Gray, 0.5f, 0.5f, 2095, false);

            UiKit.Text(Root, "CLEARED " + Save.ClearedCount + "/" + LevelCatalog.Count +
                "   STARS " + Save.TotalStars + "/" + (LevelCatalog.Count * 3),
                new Vector2(0f, 1.55f), 1, Gray, 0.5f, 0.5f, 2095, false);

            var car = UiKit.RawImage(Root, Art.Car(LevelCatalog.PlayerBody, true), Color.white, 1500, "title_car");
            car.transform.localPosition = new Vector3(-8f, -0.7f, 0f);
            car.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            car.transform.localScale = Vector3.one * 1.1f;
            _titleCar = car.transform;

            Buttons.Add(UiKit.Button(Root, new Vector2(0f, -4.8f), new Vector2(8.0f, 2.0f), "START",
                Pal.C("#4f9e63"), 2020, 3, Art.Play, b => App.ShowSelect()));

            _blink = UiKit.Text(Root, "SPIN WHEEL - PEDALS DRIVE", new Vector2(0f, -6.9f), 1,
                new Color(0.8f, 0.82f, 0.86f, 1f), 0.5f, 0.5f, 2095, false);

            UiKit.Text(Root, "A PIXEL PARKING GAME", new Vector2(0f, -8.7f), 1,
                new Color(0.62f, 0.66f, 0.74f), 0.5f, 0.5f, 2095, false);
        }

        void DecoCar(Color32 body, Vector2 pos, float ang)
        {
            var sr = UiKit.RawImage(Root, Art.Car(body, false), Color.white, 300 - Mathf.RoundToInt(pos.y * 8f), "deco_car");
            sr.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            sr.transform.localRotation = Quaternion.Euler(0f, 0f, -ang);
        }

        public override void OnTick(float dt)
        {
            _t += dt;
            if (_titleCar != null)
            {
                float x = Mathf.Repeat(_t * 3.1f, 16.5f) - 8.25f;
                _titleCar.localPosition = new Vector3(x, -0.7f + Mathf.Sin(_t * 2.6f) * 0.05f, 0f);
            }
            if (_blink != null)
            {
                float a = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(_t * 1.8f));
                _blink.SetAlpha(a);
            }
            if (App.Inputs.Confirm) App.ShowSelect();
        }
    }

    // --------------------------------------------------------------------------- select
    public class LevelSelectScreen : UiScreen
    {
        PixelText _toast;
        float _toastT;

        static readonly Color CardA = new Color(0.15f, 0.18f, 0.25f, 1f);
        static readonly Color CardB = new Color(0.18f, 0.21f, 0.28f, 1f);

        public override void Build()
        {
            MenuArt.Backdrop(this, 0.52f);

            UiKit.Text(Root, "CHOOSE A LOT", new Vector2(0f, 9.05f), 2, Pal.White, 0.5f, 0.5f, 2100, true);
            UiKit.Text(Root, "STARS " + Save.TotalStars + "/" + (LevelCatalog.Count * 3), new Vector2(0f, 8.25f), 1,
                new Color(0.72f, 0.76f, 0.83f), 0.5f, 0.5f, 2095, false);

            float[] ys = { 6.85f, 4.35f, 1.85f, -0.65f, -3.15f, -5.65f };
            for (int i = 0; i < LevelCatalog.Count; i++)
                BuildCard(i, ys[i]);

            Buttons.Add(UiKit.Button(Root, new Vector2(-4.75f, -8.65f), new Vector2(1.4f, 1.4f), "",
                new Color(0.24f, 0.27f, 0.35f, 1f), 2020, 2, Art.ArrowLeft, b => App.ShowTitle()));

            var snd = UiKit.Button(Root, new Vector2(4.35f, -8.65f), new Vector2(1.4f, 1.35f), "",
                new Color(0.24f, 0.27f, 0.35f, 1f), 2020, 2, Save.Muted ? Art.SoundOff : Art.Sound, null);
            snd.PlayClickSound = true;
            snd.OnClick = b =>
            {
                Save.Muted = !Save.Muted;
                Save.SaveAll();
                App.Audio.Muted = Save.Muted;
                b.SetIcon(Save.Muted ? Art.SoundOff : Art.Sound);
            };
            Buttons.Add(snd);

            UiKit.Text(Root, "TAP A LOT TO DRIVE", new Vector2(0f, -8.65f), 1,
                new Color(0.62f, 0.66f, 0.74f), 0.5f, 0.5f, 2095, false);

            _toast = UiKit.Text(Root, "", new Vector2(0f, -7.25f), 2, Pal.Yellow, 0.5f, 0.5f, 2200, true);
            _toast.SetVisible(false);
        }

        void BuildCard(int i, float y)
        {
            var def = LevelCatalog.Get(i);
            bool unlocked = i < Save.Unlocked;
            int stars = Save.Stars[i];

            var color = i % 2 == 0 ? CardA : CardB;
            var card = UiKit.Button(Root, new Vector2(0f, y), new Vector2(9.9f, 2.2f), "", color, 2020, 2, null,
                b =>
                {
                    if (unlocked) App.StartLevel(i);
                    else
                    {
                        App.Audio.Error();
                        ShowToast("CLEAR THE LOTS BEFORE IT");
                    }
                });
            card.PlayClickSound = unlocked;
            Buttons.Add(card);

            Color accent = Accent(i);
            var badge = UiKit.Panel(card.T, new Vector2(-4.1f, 0f), new Vector2(1.25f, 1.25f), accent, 2024);
            UiKit.Text(card.T, (i + 1).ToString(), Vector2.zero, 2, Pal.Ink, 0.5f, 0.5f, 2030, false)
                .transform.localPosition = new Vector3(-4.1f, 0.02f, 0f);

            Color nameCol = unlocked ? (Color)Pal.White : new Color(0.55f, 0.58f, 0.64f);
            UiKit.Text(card.T, def.Short, new Vector2(-3.2f, 0.48f), 2, nameCol, 0f, 0.5f, 2030, unlocked);
            UiKit.Text(card.T, def.Tag, new Vector2(-3.2f, -0.5f), 1,
                unlocked ? new Color(0.66f, 0.7f, 0.78f) : new Color(0.42f, 0.45f, 0.5f), 0f, 0.5f, 2030, false);

            MenuArt.Stars(card.T, stars, new Vector2(3.85f, -0.5f), 0.42f, 0.52f, 2030);

            if (!unlocked)
                UiKit.Image(card.T, new Vector2(4.2f, 0.45f), new Vector2(0.5f, 0.55f), Art.Lock,
                    new Color(0.7f, 0.73f, 0.8f), 2032);
        }

        static Color Accent(int i)
        {
            switch (i)
            {
                case 0: return Pal.Green;
                case 1: return Pal.Blue;
                case 2: return Pal.Yellow;
                case 3: return Pal.Orange;
                case 4: return Pal.Purple;
                default: return Pal.Red;
            }
        }

        void ShowToast(string msg)
        {
            _toast.SetText(msg);
            _toastT = 2.0f;
            _toast.SetVisible(true);
            _toast.SetAlpha(1f);
        }

        public override void OnTick(float dt)
        {
            if (_toastT > 0f)
            {
                _toastT -= dt;
                _toast.SetAlpha(Mathf.Clamp01(_toastT * 1.5f));
                if (_toastT <= 0f) _toast.SetVisible(false);
            }
            if (App.Inputs.Cancel) App.ShowTitle();
        }
    }

    // --------------------------------------------------------------------------- HUD
    public class HudScreen : UiScreen
    {
        public LevelRuntime Level;
        PixelText _timer, _bumps, _intro, _go;
        UiSteerWheel _wheel;
        UiButton _gas, _brake;
        SpriteRenderer _introPanel;
        float _t;
        bool _goShown;

        public override void Build()
        {
            Level = App.Level;

            UiKit.Image(Root, new Vector2(0f, 9.05f), new Vector2(11.25f, 2.35f), Art.White,
                new Color(0.03f, 0.04f, 0.07f, 0.42f), 1985);

            Buttons.Add(UiKit.Button(Root, new Vector2(-5.1f, 9.4f), new Vector2(0.85f, 0.85f), "",
                new Color(0.22f, 0.25f, 0.33f, 0.95f), 2020, 2, Art.Pause, b => App.Pause()));

            UiKit.Text(Root, Level.Def.Short, new Vector2(-4.45f, 9.4f), 2, Pal.White, 0f, 0.5f, 2030, true);

            _timer = UiKit.Text(Root, "0:00", new Vector2(5.2f, 9.42f), 2, Pal.Cream, 1f, 0.5f, 2030, true);
            _bumps = UiKit.Text(Root, "BUMPS 0", new Vector2(5.2f, 8.68f), 1, new Color(0.72f, 0.76f, 0.83f), 1f, 0.5f, 2030, false);

            // intro instruction banner
            _introPanel = UiKit.Image(Root, new Vector2(0f, 6.95f), new Vector2(11.25f, 3.6f), Art.White,
                new Color(0.03f, 0.04f, 0.07f, 0.62f), 1988);
            _intro = UiKit.Text(Root, Wrap(Level.Def.Instruction, 14), new Vector2(0f, 7.95f), 2, Pal.Cream, 0.5f, 1f, 2040, true);
            _go = UiKit.Text(Root, "GET READY", new Vector2(0f, 3.3f), 2, Pal.Yellow, 0.5f, 0.5f, 2060, true);

            // steering wheel at the bottom centre: grab and spin
            _wheel = new UiSteerWheel();
            _wheel.Init(Root, new Vector2(0f, -8.94f), 1.0f, 2040);

            // gas and brake pedals on the right thumb
            _gas = Pedal(new Vector2(4.45f, -7.45f), "GAS", Art.ArrowUp, Pal.C("#3f7f57"), 2040);
            _brake = Pedal(new Vector2(4.45f, -9.0f), "REV", Art.ArrowDown, Pal.C("#a34a42"), 2046);
            Buttons.Add(_gas);
            Buttons.Add(_brake);
        }

        UiButton Pedal(Vector2 center, string label, Sprite icon, Color color, int order)
        {
            var b = UiKit.Button(Root, center, new Vector2(1.7f, 1.45f), "", color, 2020, 2, null, null);
            b.PlayClickSound = false;
            UiKit.Image(b.T, new Vector2(0f, 0.24f), new Vector2(0.58f, 0.58f), icon, Color.white, order);
            UiKit.Text(b.T, label, new Vector2(0f, -0.42f), 1, new Color(1f, 1f, 1f, 0.95f), 0.5f, 0.5f, order + 4, false);
            return b;
        }

        static string Wrap(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var words = text.Split(' ');
            var lines = new List<string>();
            string cur = "";
            foreach (var w in words)
            {
                if (cur.Length == 0) cur = w;
                else if (cur.Length + 1 + w.Length <= width) cur += " " + w;
                else { lines.Add(cur); cur = w; }
            }
            if (cur.Length > 0) lines.Add(cur);
            return string.Join("\n", lines);
        }

        public override void OnTick(float dt)
        {
            _t += dt;
            if (Level == null) return;

            _wheel.Tick(App.Inputs);

            float throttle = 0f;
            if (_gas.Down || App.Inputs.Gas) throttle += 1f;
            if (_brake.Down || App.Inputs.Brake) throttle -= 1f;

            float steer = 0f;
            if (_wheel.Held)
            {
                float x = Mathf.Abs(_wheel.Value) < 0.05f ? 0f : _wheel.Value;
                steer = x * 1.35f;
            }
            if (App.Inputs.Left) steer = -1f;
            else if (App.Inputs.Right) steer = 1f;

            Level.SetInput(Mathf.Clamp(throttle, -1f, 1f), Mathf.Clamp(steer, -1f, 1f));

            _timer.SetText(Fmt.Clock(Level.Elapsed));
            _bumps.SetText("BUMPS " + Level.Bumps);

            if (!Level.Started)
            {
                float p = 1f - Mathf.Clamp01(Level.StartDelay / 1f);
                _go.SetText("GET READY");
                _go.SetColor(Color.Lerp(Pal.Yellow, Pal.White, p));
                float s = Mathf.Lerp(1.2f, 0.9f, p);
                _go.transform.localScale = new Vector3(s, s, 1f);
            }
            else if (!_goShown)
            {
                _goShown = true;
                _go.SetText("GO!");
                _go.SetColor(Pal.Mint);
            }
            else if (_t > 1.6f && _go != null)
            {
                _go.SetVisible(false);
            }

            if (_t > 3.2f)
            {
                float a = Mathf.Clamp01(1f - (_t - 3.2f) / 0.7f);
                _intro.SetAlpha(a);
                _introPanel.color = new Color(0.03f, 0.04f, 0.07f, 0.62f * a);
                if (a <= 0f && _introPanel.gameObject.activeSelf)
                {
                    _intro.SetVisible(false);
                    _introPanel.gameObject.SetActive(false);
                }
            }


            if (App.Inputs.PausePressed) App.Pause();
        }
    }

    // --------------------------------------------------------------------------- win
    public class WinScreen : UiScreen
    {
        public int LevelIndex;
        public float Time;
        public int Bumps;
        public int Stars;
        public bool NewBest;

        float _t;
        readonly List<Transform> _starTransforms = new List<Transform>();

        public override void Build()
        {
            bool last = LevelIndex >= LevelCatalog.Count - 1;
            var def = LevelCatalog.Get(LevelIndex);

            UiKit.Dim(Root, new Color(0.02f, 0.03f, 0.05f, 0.72f), 3000);
            UiKit.Panel(Root, new Vector2(0f, 0.4f), new Vector2(9.9f, 15.0f), new Color(0.13f, 0.16f, 0.22f, 1f), 3010);

            UiKit.Text(Root, "PARKED!", new Vector2(0f, 6.1f), 3, Pal.Mint, 0.5f, 0.5f, 3040, true);
            UiKit.Text(Root, def.Short, new Vector2(0f, 4.85f), 2, new Color(0.72f, 0.76f, 0.83f), 0.5f, 0.5f, 3030, false);

            for (int i = 0; i < 3; i++)
            {
                var sr = UiKit.Image(Root, new Vector2((i - 1) * 1.7f, 3.5f), new Vector2(1.3f, 1.3f), Art.Star,
                    i < Stars ? (Color)Pal.Yellow : new Color(0.24f, 0.27f, 0.34f), 3040);
                sr.transform.localScale = Vector3.zero;
                _starTransforms.Add(sr.transform);
            }

            UiKit.Text(Root, "TIME  " + Fmt.Time(Time), new Vector2(0f, 2.1f), 2, Pal.White, 0.5f, 0.5f, 3030, false);
            UiKit.Text(Root, "BUMPS  " + Bumps, new Vector2(0f, 0.95f), 2,
                Bumps == 0 ? Pal.Green : new Color(0.8f, 0.65f, 0.5f), 0.5f, 0.5f, 3030, false);

            string best = Save.Best[LevelIndex] >= 0f ? "BEST  " + Fmt.Time(Save.Best[LevelIndex]) : "";
            UiKit.Text(Root, best, new Vector2(0f, 0.15f), 1, new Color(0.66f, 0.7f, 0.78f), 0.5f, 0.5f, 3030, false);

            if (NewBest)
                UiKit.Text(Root, "NEW BEST!", new Vector2(0f, -0.75f), 2, Pal.Yellow, 0.5f, 0.5f, 3035, true);

            Buttons.Add(UiKit.Button(Root, new Vector2(0f, -2.6f), new Vector2(6.9f, 1.9f), last ? "LOTS" : "NEXT",
                Pal.C("#4f9e63"), 3020, 3, last ? null : Art.ArrowRight, b =>
                {
                    if (last) App.ShowSelect();
                    else App.NextLevel();
                }));

            Buttons.Add(UiKit.Button(Root, new Vector2(0f, -4.4f), new Vector2(6.9f, 1.5f), "RETRY",
                new Color(0.26f, 0.3f, 0.39f), 3020, 2, null, b => App.RetryLevel()));

            Buttons.Add(UiKit.Button(Root, new Vector2(0f, -5.95f), new Vector2(6.9f, 1.25f), "ALL LOTS",
                new Color(0.19f, 0.22f, 0.29f), 3020, 2, null, b => App.ShowSelect()));
        }

        public override void OnTick(float dt)
        {
            _t += dt;
            for (int i = 0; i < _starTransforms.Count; i++)
            {
                float t = Mathf.Clamp01((_t - 0.2f - i * 0.16f) / 0.28f);
                float s = t <= 0f ? 0f : EaseOutBack(t);
                bool earned = i < Stars;
                _starTransforms[i].localScale = Vector3.one * (earned ? s : Mathf.Max(0.001f, s) * 1f);
                if (t > 0f && earned && !_starSfx[i])
                {
                    _starSfx[i] = true;
                    App.Audio.Star();
                }
            }
            if (App.Inputs.Confirm)
            {
                if (LevelIndex >= LevelCatalog.Count - 1) App.ShowSelect();
                else App.NextLevel();
            }
        }

        readonly bool[] _starSfx = new bool[3];

        static float EaseOutBack(float t)
        {
            float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }

    // --------------------------------------------------------------------------- pause
    public class PauseScreen : UiScreen
    {
        public override void Build()
        {
            UiKit.Dim(Root, new Color(0.02f, 0.03f, 0.05f, 0.66f), 3000);
            UiKit.Panel(Root, new Vector2(0f, 0.3f), new Vector2(8.9f, 9.6f), new Color(0.13f, 0.16f, 0.22f, 1f), 3010);

            UiKit.Text(Root, "PAUSED", new Vector2(0f, 3.5f), 3, Pal.White, 0.5f, 0.5f, 3040, true);
            UiKit.Text(Root, App.Level != null ? App.Level.Def.Short : "", new Vector2(0f, 2.35f), 2,
                new Color(0.7f, 0.74f, 0.82f), 0.5f, 0.5f, 3030, false);

            Buttons.Add(UiKit.Button(Root, new Vector2(0f, 0.3f), new Vector2(7.6f, 1.8f), "RESUME",
                Pal.C("#4f9e63"), 3020, 3, Art.Play, b => App.Resume()));
            Buttons.Add(UiKit.Button(Root, new Vector2(0f, -1.6f), new Vector2(7.6f, 1.55f), "RETRY",
                new Color(0.26f, 0.3f, 0.39f), 3020, 2, null, b => App.RetryLevel()));
            Buttons.Add(UiKit.Button(Root, new Vector2(0f, -3.35f), new Vector2(7.6f, 1.4f), "ALL LOTS",
                new Color(0.19f, 0.22f, 0.29f), 3020, 2, null, b => App.ShowSelect()));
        }
    }
}
