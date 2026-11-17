using System.Collections.Generic;
using UnityEngine;

namespace Parking
{
    /// <summary>Procedurally drawn sprites: cars, props, UI chrome and icons.</summary>
    public static class Art
    {
        static Material _mat;
        static Sprite _panel, _white, _confetti, _vignette;
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Runtime sprites die when play mode ends; rebuild them on the next run.</summary>
        public static void ResetRuntimeCache()
        {
            Cache.Clear();
            SlotCache.Clear();
            _panel = null;
            _white = null;
            _confetti = null;
            _vignette = null;
            _mat = null;
        }

        public static Material SpriteMaterial
        {
            get
            {
                if (_mat == null)
                {
                    var sh = Shader.Find("Sprites/Default");
                    if (sh != null) _mat = new Material(sh) { name = "PixelSprite" };
                }
                return _mat;
            }
        }

        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var p = new Px(4, 4, new Color32(255, 255, 255, 255));
                    _white = p.ToSprite("white", 16f);
                }
                return _white;
            }
        }

        public static Sprite ConfettiSprite
        {
            get
            {
                if (_confetti == null)
                {
                    var p = new Px(3, 3, new Color32(255, 255, 255, 255));
                    _confetti = p.ToSprite("confetti", 16f);
                }
                return _confetti;
            }
        }

        public static Sprite VignetteSprite
        {
            get
            {
                if (_vignette == null) _vignette = GroundPainter.Vignette();
                return _vignette;
            }
        }

        public static Sprite PanelMask
        {
            get
            {
                if (_panel == null)
                {
                    var p = new Px(24, 24, new Color32(0, 0, 0, 0));
                    p.FillRect(0, 0, 24, 24, new Color32(12, 12, 16, 255));
                    p.FillRect(1, 1, 22, 22, new Color32(216, 216, 216, 255));
                    p.FillRect(2, 2, 20, 20, new Color32(206, 206, 206, 255));
                    p.FillRect(2, 20, 20, 2, new Color32(255, 255, 255, 255));
                    p.FillRect(2, 2, 2, 18, new Color32(238, 238, 238, 255));
                    p.FillRect(2, 2, 20, 2, new Color32(150, 150, 150, 255));
                    p.FillRect(20, 2, 2, 20, new Color32(166, 166, 166, 255));
                    p.RectOutline(1, 1, 22, 22, 1, new Color32(70, 70, 70, 255));
                    _panel = p.ToSlicedSprite("panel", 7, 16f);
                }
                return _panel;
            }
        }

        // ------------------------------------------------------------------ cars
        public static Sprite Car(Color32 body, bool stripe, bool looseFrontWheels = false)
        {
            string key = "car_" + body.r + "_" + body.g + "_" + body.b + (stripe ? "_s" : "") + (looseFrontWheels ? "_f" : "");
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var p = new Px(24, 44, new Color32(0, 0, 0, 0));
            var deep = Pal.Dark(body, 0.6f);
            var edge = Pal.Dark(body, 0.4f);
            var dark = Pal.Dark(body, 0.26f);
            var light = Pal.Light(body, 0.22f);
            var trim = Pal.Dark(body, 0.72f);
            var glass = Pal.C("#273646");
            var glassLight = Pal.C("#52708c");
            var tire = Pal.C("#10121a");
            var hub = Pal.C("#5a6472");
            var cream = Pal.C("#f3ecd6");
            var head = Pal.C("#ffe9a6");
            var tail = Pal.C("#ff7464");

            // ground shadow
            p.Ellipse(12.5f, 19.5f, 10.5f, 20.5f, new Color32(0, 0, 0, 58));

            // wheels (front pair can be left out so they can steer independently)
            Wheel(p, 1, 6, tire, hub);
            Wheel(p, 19, 6, tire, hub);
            if (!looseFrontWheels)
            {
                Wheel(p, 1, 30, tire, hub);
                Wheel(p, 19, 30, tire, hub);
            }

            // shell
            p.RoundRect(3, 2, 18, 40, 6, trim);
            p.RoundRect(4, 3, 16, 38, 5, deep);
            p.RoundRect(5, 4, 14, 36, 4, body);

            // side shading
            p.FillRect(17, 7, 2, 31, Pal.A(dark, 0.55f));
            p.FillRect(5, 7, 1, 31, Pal.A(light, 0.5f));

            // bumper strips
            p.FillRect(6, 2, 12, 1, Pal.A(trim, 0.9f));
            p.FillRect(6, 41, 12, 1, Pal.A(trim, 0.9f));

            // panel seams
            p.HLine(6, 32, 12, Pal.A(edge, 0.65f));
            p.HLine(6, 11, 12, Pal.A(edge, 0.55f));
            p.HLine(6, 25, 12, Pal.A(edge, 0.4f));

            // racing stripes (only on the player's car)
            if (stripe)
            {
                p.VLine(8, 5, 32, cream);
                p.VLine(13, 5, 32, cream);
                p.VLine(9, 5, 32, Pal.A(dark, 0.18f));
                p.VLine(14, 5, 32, Pal.A(dark, 0.18f));
            }

            // windshield
            for (int y = 24; y <= 30; y++)
            {
                int inset = Mathf.RoundToInt((y - 24) / 4f);
                p.HLine(6 + inset, y, 12 - inset * 2, glass);
            }
            p.HLine(7, 30, 10, glassLight);
            p.HLine(8, 29, 3, Pal.A(glassLight, 0.85f));
            p.HLine(6, 24, 12, Pal.A(trim, 0.95f));

            // roof
            p.FillRect(6, 14, 12, 9, Pal.Light(body, 0.07f));
            p.FillRect(6, 14, 12, 1, Pal.A(edge, 0.55f));
            p.FillRect(8, 15, 8, 1, Pal.A(light, 0.2f));

            // side windows
            p.VLine(6, 15, 8, glass);
            p.VLine(17, 15, 8, glass);

            // rear window
            for (int y = 8; y <= 12; y++)
                p.HLine(7, y, 10, glass);
            p.HLine(8, 12, 8, Pal.A(glassLight, 0.7f));

            // headlights
            p.FillRect(5, 36, 4, 3, head);
            p.FillRect(15, 36, 4, 3, head);
            p.FillRect(5, 38, 4, 1, Pal.C("#fff7da"));
            p.FillRect(15, 38, 4, 1, Pal.C("#fff7da"));
            p.FillRect(5, 35, 4, 1, Pal.A(trim, 0.8f));
            p.FillRect(15, 35, 4, 1, Pal.A(trim, 0.8f));

            // tail lights
            p.FillRect(5, 3, 4, 2, tail);
            p.FillRect(15, 3, 4, 2, tail);
            p.FillRect(5, 3, 4, 1, Pal.Light(tail, 0.35f));
            p.FillRect(15, 3, 4, 1, Pal.Light(tail, 0.35f));

            // number plate
            p.FillRect(9, 4, 6, 2, cream);
            p.FillRect(10, 4, 1, 1, Pal.A(Pal.Ink, 0.75f));
            p.FillRect(12, 4, 1, 1, Pal.A(Pal.Ink, 0.75f));
            p.FillRect(14, 4, 1, 1, Pal.A(Pal.Ink, 0.75f));

            // mirrors
            p.FillRect(2, 25, 2, 2, trim);
            p.FillRect(20, 25, 2, 2, trim);
            p.Set(2, 26, light);
            p.Set(21, 26, light);

            var sprite = p.ToSprite("car", 16f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite CarWheel
        {
            get
            {
                if (Cache.TryGetValue("carwheel", out var s)) return s;
                var p = new Px(4, 8, new Color32(0, 0, 0, 0));
                var tire = Pal.C("#10121a");
                var hub = Pal.C("#5a6472");
                p.RoundRect(0, 0, 4, 8, 1, tire);
                p.FillRect(1, 2, 2, 4, hub);
                p.FillRect(1, 3, 1, 2, Pal.Light(hub, 0.3f));
                p.FillRect(1, 1, 2, 1, Pal.C("#2b3140"));
                p.FillRect(1, 6, 2, 1, Pal.C("#2b3140"));
                s = p.ToSprite("carwheel", 16f);
                Cache["carwheel"] = s;
                return s;
            }
        }

        static void Wheel(Px p, int x, int y, Color32 tire, Color32 hub)
        {
            p.FillRect(x, y, 4, 8, tire);
            p.FillRect(x + 1, y + 2, 2, 4, hub);
            p.Set(x + 1, y + 3, Pal.Light(hub, 0.35f));
        }

        // ------------------------------------------------------------------ props
        public static Sprite Cone
        {
            get
            {
                if (Cache.TryGetValue("cone", out var s)) return s;
                var p = new Px(9, 12, new Color32(0, 0, 0, 0));
                p.Ellipse(4.5f, 2f, 4.5f, 2f, new Color32(0, 0, 0, 55));
                p.FillRect(1, 1, 7, 2, Pal.C("#c9581f"));
                p.FillRect(2, 3, 5, 2, Pal.C("#ef8b3c"));
                p.FillRect(3, 5, 3, 2, Pal.C("#f7f2e2"));
                p.FillRect(2, 7, 5, 1, Pal.C("#ef8b3c"));
                p.FillRect(3, 8, 3, 2, Pal.C("#e07a2f"));
                p.FillRect(2, 9, 1, 2, Pal.C("#ef8b3c"));
                p.FillRect(6, 9, 1, 2, Pal.C("#ef8b3c"));
                p.FillRect(4, 9, 1, 2, Pal.C("#d9682a"));
                s = p.ToSprite("cone", 16f);
                Cache["cone"] = s;
                return s;
            }
        }

        public static Sprite Tree
        {
            get
            {
                if (Cache.TryGetValue("tree", out var s)) return s;
                var p = new Px(22, 28, new Color32(0, 0, 0, 0));
                var leaf = Pal.C("#3f7038");
                var leafLight = Pal.C("#568a45");
                var leafDark = Pal.C("#2f5a2c");
                p.Ellipse(11f, 6f, 9f, 5f, new Color32(0, 0, 0, 50));
                p.FillRect(10, 3, 3, 10, Pal.C("#6b4a32"));
                p.Ellipse(11f, 17f, 10f, 9f, leafDark);
                p.Ellipse(11f, 18f, 8.5f, 7.5f, leaf);
                p.Ellipse(9f, 20f, 5.5f, 4.5f, leafLight);
                p.Ellipse(14f, 15f, 4f, 3.5f, leafDark);
                for (int i = 0; i < 10; i++)
                {
                    int x = 3 + i * 2;
                    p.Set(x, 15 + (i % 3), leafLight);
                    p.Set(x + 1, 12 + (i % 4), leafDark);
                }
                s = p.ToSprite("tree", 16f);
                Cache["tree"] = s;
                return s;
            }
        }

        public static Sprite Bush
        {
            get
            {
                if (Cache.TryGetValue("bush", out var s)) return s;
                var p = new Px(14, 12, new Color32(0, 0, 0, 0));
                p.Ellipse(7f, 2f, 6f, 2f, new Color32(0, 0, 0, 45));
                p.Ellipse(7f, 6f, 6.5f, 4.5f, Pal.C("#325c2f"));
                p.Ellipse(7f, 7f, 5f, 3.5f, Pal.Grass);
                p.Ellipse(5f, 8f, 2.5f, 2f, Pal.GrassLight);
                s = p.ToSprite("bush", 16f);
                Cache["bush"] = s;
                return s;
            }
        }

        public static Sprite Pillar
        {
            get
            {
                if (Cache.TryGetValue("pillar", out var s)) return s;
                var p = new Px(14, 14, new Color32(0, 0, 0, 0));
                p.Ellipse(7f, 3f, 7f, 3f, new Color32(0, 0, 0, 55));
                p.FillRect(1, 1, 12, 12, Pal.C("#555b63"));
                p.FillRect(2, 2, 10, 10, Pal.C("#7d838c"));
                p.FillRect(2, 9, 10, 3, Pal.C("#8d939c"));
                p.FillRect(2, 2, 10, 2, Pal.C("#61676f"));
                p.FillRect(3, 3, 8, 1, Pal.C("#a3a9b2"));
                p.FillRect(1, 1, 2, 1, Pal.C("#f5c445"));
                p.FillRect(11, 1, 2, 1, Pal.C("#f5c445"));
                p.FillRect(1, 12, 12, 1, Pal.C("#3d4249"));
                s = p.ToSprite("pillar", 16f);
                Cache["pillar"] = s;
                return s;
            }
        }

        public static Sprite Hydrant
        {
            get
            {
                if (Cache.TryGetValue("hydrant", out var s)) return s;
                var p = new Px(9, 12, new Color32(0, 0, 0, 0));
                p.Ellipse(4.5f, 1.5f, 4f, 1.5f, new Color32(0, 0, 0, 50));
                p.FillRect(2, 1, 5, 8, Pal.C("#b03a34"));
                p.FillRect(1, 3, 7, 2, Pal.C("#c9483f"));
                p.FillRect(3, 9, 3, 2, Pal.C("#b03a34"));
                p.FillRect(3, 10, 3, 1, Pal.C("#e26a5f"));
                p.FillRect(2, 9, 1, 1, Pal.C("#8d2c27"));
                p.FillRect(2, 2, 1, 6, Pal.C("#d4554b"));
                s = p.ToSprite("hydrant", 16f);
                Cache["hydrant"] = s;
                return s;
            }
        }

        public static Sprite Bin
        {
            get
            {
                if (Cache.TryGetValue("bin", out var s)) return s;
                var p = new Px(10, 12, new Color32(0, 0, 0, 0));
                p.Ellipse(5f, 1.5f, 4.5f, 1.5f, new Color32(0, 0, 0, 50));
                p.FillRect(1, 1, 8, 9, Pal.C("#3f5a45"));
                p.FillRect(2, 2, 6, 7, Pal.C("#547a5c"));
                p.FillRect(0, 9, 10, 2, Pal.C("#2f4636"));
                p.FillRect(3, 8, 4, 1, Pal.C("#2f4636"));
                s = p.ToSprite("bin", 16f);
                Cache["bin"] = s;
                return s;
            }
        }

        public static Sprite Sign
        {
            get
            {
                if (Cache.TryGetValue("sign", out var s)) return s;
                var p = new Px(14, 20, new Color32(0, 0, 0, 0));
                p.Ellipse(7f, 1.5f, 3.5f, 1.3f, new Color32(0, 0, 0, 45));
                p.FillRect(6, 1, 2, 12, Pal.C("#4f5560"));
                p.FillRect(1, 11, 12, 8, Pal.C("#1f2937"));
                p.FillRect(2, 12, 10, 6, Pal.C("#2f6fb5"));
                p.Glyph('P', 5, 13, 1, Pal.White);
                s = p.ToSprite("sign", 16f);
                Cache["sign"] = s;
                return s;
            }
        }

        public static Sprite SteerWheel
        {
            get
            {
                if (Cache.TryGetValue("steerwheel", out var s)) return s;

                var p = new Px(56, 56, new Color32(0, 0, 0, 0));
                var dark = new Color32(15, 19, 27, 255);
                var rim = new Color32(65, 77, 95, 255);
                var rimHi = new Color32(126, 141, 164, 255);
                var rimLo = new Color32(38, 46, 60, 255);
                var spoke = new Color32(108, 122, 142, 255);
                var cream = new Color32(233, 226, 203, 255);
                var creamHi = new Color32(250, 245, 228, 255);
                var creamLo = new Color32(178, 170, 148, 255);

                p.Ellipse(28f, 24f, 26f, 24f, new Color32(0, 0, 0, 55));

                // tyre ring
                for (int y = 0; y < 56; y++)
                {
                    for (int x = 0; x < 56; x++)
                    {
                        float dx = x + 0.5f - 28f, dy = y + 0.5f - 28f;
                        float r = Mathf.Sqrt(dx * dx + dy * dy);
                        if (r >= 19.5f && r <= 26.5f)
                        {
                            p.Set(x, y, rim);
                            if (r >= 25.4f) p.Set(x, y, dark);
                            else if (dy > 0f && dx < 0f && r >= 23.5f) p.Set(x, y, rimHi);
                            else if (dy < 0f && r <= 21.5f) p.Set(x, y, rimLo);
                        }
                    }
                }

                // three spokes
                p.Line(28, 28, 28, 48, 4, dark);
                p.Line(28, 28, 10, 17, 4, dark);
                p.Line(28, 28, 46, 17, 4, dark);
                p.Line(28, 28, 28, 46, 2, spoke);
                p.Line(28, 28, 11, 18, 2, spoke);
                p.Line(28, 28, 45, 18, 2, spoke);

                // hub
                p.Disk(28, 28, 9, dark);
                p.Disk(28, 28, 8, spoke);
                p.Disk(28, 28, 5, creamLo);
                p.Disk(27, 29, 4, cream);
                p.Disk(28, 28, 2, creamHi);

                // spinner knob on the rim (the bit you grab)
                p.Disk(28, 46, 7, dark);
                p.Disk(28, 46, 6, creamLo);
                p.Disk(28, 46, 5, cream);
                p.Disk(27, 47, 3, creamHi);
                p.FillRect(24, 45, 8, 1, creamLo);
                p.FillRect(24, 43, 8, 1, creamLo);

                // centre marker on the rim
                p.FillRect(27, 1, 2, 3, creamLo);

                s = p.ToSprite("steerwheel", 16f);
                Cache["steerwheel"] = s;
                return s;
            }
        }

        // ------------------------------------------------------------------ icons
        static Sprite Icon(string key, System.Action<Px> draw, int w = 9, int h = 9)
        {
            if (Cache.TryGetValue(key, out var s)) return s;
            var p = new Px(w, h, new Color32(0, 0, 0, 0));
            draw(p);
            s = p.ToSprite(key, 16f);
            Cache[key] = s;
            return s;
        }

        public static Sprite ArrowLeft => Icon("arrowL", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            for (int y = 0; y < 9; y++)
            {
                int d = Mathf.Abs(y - 4);
                p.FillRect(d, y, 2, 1, c);
            }
        }, 9, 9);

        public static Sprite ArrowRight => Icon("arrowR", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            for (int y = 0; y < 9; y++)
            {
                int d = Mathf.Abs(y - 4);
                p.FillRect(4 - d, y, 2, 1, c);
            }
        }, 9, 9);

        public static Sprite ArrowUp => Icon("arrowU", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            for (int x = 0; x < 9; x++)
            {
                int d = Mathf.Abs(x - 4);
                p.FillRect(x, 4 - d, 1, 2, c);
            }
        }, 9, 9);

        public static Sprite ArrowDown => Icon("arrowD", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            for (int x = 0; x < 9; x++)
            {
                int d = Mathf.Abs(x - 4);
                p.FillRect(x, 4 + d, 1, 2, c);
            }
        }, 9, 9);

        public static Sprite Star => Icon("star", p =>
        {
            int[,] rows =
            {
                { 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
                { 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
                { 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0 },
                { 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0 },
                { 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1 },
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            };
            var c = new Color32(255, 255, 255, 255);
            for (int y = 0; y < 11; y++)
                for (int x = 0; x < 11; x++)
                    if (rows[y, x] == 1) p.Set(x, 10 - y, c);
        }, 11, 11);

        public static Sprite Lock => Icon("lock", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            p.FillRect(2, 5, 6, 5, c);
            p.FillRect(3, 6, 4, 3, new Color32(0, 0, 0, 255));
            p.FillRect(3, 7, 1, 2, c);
            p.FillRect(6, 7, 1, 2, c);
            p.FillRect(2, 2, 1, 3, c);
            p.FillRect(7, 2, 1, 3, c);
            p.FillRect(3, 1, 4, 1, c);
            p.FillRect(0, 4, 10, 6, c);
            p.FillRect(1, 5, 8, 4, new Color32(0, 0, 0, 0));
            p.FillRect(1, 5, 8, 4, c);
            p.Set(4, 6, new Color32(0, 0, 0, 255));
            p.Set(5, 6, new Color32(0, 0, 0, 255));
            p.Set(4, 7, new Color32(0, 0, 0, 255));
        }, 10, 11);

        public static Sprite Pause => Icon("pause", p =>
        {
            p.FillRect(1, 1, 3, 9, new Color32(255, 255, 255, 255));
            p.FillRect(6, 1, 3, 9, new Color32(255, 255, 255, 255));
        }, 10, 11);

        public static Sprite Play => Icon("play", p =>
        {
            for (int y = 0; y < 11; y++)
            {
                int w = 1 + Mathf.Min(y, 10 - y);
                p.HLine(1, 10 - y, w, new Color32(255, 255, 255, 255));
            }
        }, 10, 11);

        public static Sprite Sound => Icon("sound", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            p.FillRect(1, 4, 3, 4, c);
            p.FillRect(4, 2, 2, 8, c);
            p.FillRect(5, 3, 1, 6, c);
            p.FillRect(7, 4, 1, 1, c);
            p.FillRect(8, 5, 1, 2, c);
            p.FillRect(7, 7, 1, 1, c);
        }, 10, 11);

        public static Sprite SoundOff => Icon("soundoff", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            p.FillRect(1, 4, 3, 4, c);
            p.FillRect(4, 2, 2, 8, c);
            p.FillRect(5, 3, 1, 6, c);
            p.FillRect(7, 4, 1, 1, c);
            p.FillRect(8, 5, 1, 2, c);
            p.FillRect(7, 7, 1, 1, c);
            p.FillRect(1, 8, 5, 1, new Color32(0, 0, 0, 255));
            p.Line(1, 7, 6, 2, 2, new Color32(230, 80, 70, 255));
        }, 10, 11);

        public static Sprite Chevron => Icon("chev", p =>
        {
            var c = new Color32(255, 255, 255, 255);
            for (int y = 0; y < 5; y++)
            {
                p.FillRect(4 - y, 4 + y, 2, 2, c);
                p.FillRect(4 + y, 4 - y, 2, 2, c);
            }
        }, 10, 9);

        public static Sprite Pin => Icon("pin", p =>
        {
            var white = new Color32(246, 244, 232, 255);
            var blue = Pal.C("#2f6fb5");
            var darkBlue = Pal.C("#1f4a7c");
            p.Disk(10, 16, 9, white);
            p.Disk(10, 16, 8, darkBlue);
            p.Disk(10, 16, 7, blue);
            p.FillRect(6, 3, 8, 8, darkBlue);
            for (int y = 0; y < 8; y++)
            {
                int inset = 4 - Mathf.FloorToInt(y * 0.45f);
                p.FillRect(6 + inset, y + 2, 8 - inset * 2, 1, darkBlue);
            }
            p.Glyph('P', 6, 11, 2, white);
            p.Disk(8, 18, 2, Pal.A(white, 0.25f));
        }, 20, 27);

        // ------------------------------------------------------------------ slot overlay
        static readonly Dictionary<string, Sprite> SlotCache = new Dictionary<string, Sprite>();

        public static Sprite SlotOverlay(float width, float height)
        {
            string key = Mathf.RoundToInt(width * 16) + "x" + Mathf.RoundToInt(height * 16);
            if (SlotCache.TryGetValue(key, out var s)) return s;

            int w = Mathf.Max(8, Mathf.RoundToInt(width * 16));
            int h = Mathf.Max(8, Mathf.RoundToInt(height * 16));
            var p = new Px(w, h, new Color32(0, 0, 0, 0));

            var fill = new Color32(143, 240, 180, 26);
            p.FillRect(2, 2, w - 4, h - 4, fill);

            var bright = new Color32(143, 240, 180, 235);
            var dim = new Color32(143, 240, 180, 90);

            int dash = 5, gap = 4;
            for (int x = 0; x < w; x += dash + gap)
            {
                p.FillRect(x, 0, Mathf.Min(dash, w - x), 2, bright);
                p.FillRect(x, h - 2, Mathf.Min(dash, w - x), 2, bright);
            }
            for (int y = 0; y < h; y += dash + gap)
            {
                p.FillRect(0, y, 2, Mathf.Min(dash, h - y), bright);
                p.FillRect(w - 2, y, 2, Mathf.Min(dash, h - y), bright);
            }

            // corner brackets
            int b = Mathf.Min(8, Mathf.Min(w, h) / 3);
            p.FillRect(2, 2, b, 3, bright);
            p.FillRect(2, 2, 3, b, bright);
            p.FillRect(w - b - 2, 2, b, 3, bright);
            p.FillRect(w - 5, 2, 3, b, bright);
            p.FillRect(2, h - 5, b, 3, bright);
            p.FillRect(2, h - b - 2, 3, b, bright);
            p.FillRect(w - b - 2, h - 5, b, 3, bright);
            p.FillRect(w - 5, h - b - 2, 3, b, bright);

            // entry chevrons pointing into the stall (up)
            int cx = w / 2;
            int cy = Mathf.Max(10, h / 6);
            for (int i = 0; i < 2; i++)
            {
                int yy = cy + i * 6;
                p.Line(cx - 5, yy, cx, yy + 5, 2, dim);
                p.Line(cx + 5, yy, cx, yy + 5, 2, dim);
            }

            s = p.ToSprite("slot", 16f);
            SlotCache[key] = s;
            return s;
        }
    }
}
