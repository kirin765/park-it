using System.Collections.Generic;
using UnityEngine;

namespace Parking
{
    /// <summary>Paints each lot's floor, bay lines, kerbs and grime into a single crisp texture.</summary>
    public static class GroundPainter
    {
        public const float PPU = 16f;
        public const float WorldW = 11.25f;
        public const float WorldH = 20f;
        public const int TW = 180;
        public const int TH = 320;

        static int PX(float x) => Mathf.RoundToInt((x + WorldW * 0.5f) * PPU);
        static int PY(float y) => Mathf.RoundToInt((y + WorldH * 0.5f) * PPU);

        public static Sprite Paint(LevelDef d, int seed = 7, bool minimap = false)
        {
            var rng = new System.Random(seed);
            Color32 baseCol = d.Ground == GroundStyle.Concrete ? Pal.Concrete : Pal.Grass;

            var p = new Px(TW, TH, baseCol);
            BaseTexture(p, d.Ground, rng);

            foreach (var r in d.Asphalt)
            {
                p.FillRect(PX(r.x - r.w / 2), PY(r.y - r.h / 2), Mathf.RoundToInt(r.w * PPU), Mathf.RoundToInt(r.h * PPU), Pal.Asphalt);
                Edge(p, r, Pal.AsphaltDark, 2);
            }

            // asphalt grain
            foreach (var r in d.Asphalt)
            {
                Speckle(p, rng, Mathf.RoundToInt(r.w * r.h * 8f), r,
                    Pal.AsphaltLight, Pal.AsphaltDark, Pal.C("#41454d"), Pal.C("#2f3339"));
                Cracks(p, rng, r, 2 + (int)(r.w * r.h / 40f));
            }

            foreach (var r in d.Buildings)
                PaintSidewalk(p, r, rng);

            foreach (var r in d.Planters)
                PaintPlanter(p, r, rng);

            foreach (var w in d.Walls)
                PaintWall(p, w);

            foreach (var b in d.Bays)
                PaintBay(p, b, rng, IsTarget(d, b), d.Ground == GroundStyle.Concrete ? Pal.Concrete : Pal.Asphalt, d.Ground);

            // The parallel-parking slot has no painted bay data; draw the stall edge.
            if (!HasTargetBay(d) && d.SlotSize.x > 0.1f)
                PaintParallelStall(p, d, rng);

            Decorations(p, d, rng);

            if (!string.IsNullOrEmpty(d.GroundWord))
                PaintWord(p, d);

            // Soft dark border so the lot meets the background cleanly.
            var tex = p.ToTexture(minimap ? "minimap" : "ground_" + d.Name);
            return Sprite.Create(tex, new Rect(0, 0, TW, TH), new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect);
        }

        static bool HasTargetBay(LevelDef d)
        {
            foreach (var b in d.Bays)
                if (Vector2.Distance(new Vector2(b.x, b.y), d.SlotCenter) < 0.35f) return true;
            return false;
        }

        static bool IsTarget(LevelDef d, BayDef b)
        {
            return Vector2.Distance(new Vector2(b.x, b.y), d.SlotCenter) < 0.35f &&
                   LevelDef.AngleDiff(b.ang, d.SlotAng) < 4f;
        }

        static void BaseTexture(Px p, GroundStyle style, System.Random rng)
        {
            if (style == GroundStyle.Concrete)
            {
                p.Noise(rng, 5000, Pal.Light(Pal.Concrete, 0.05f), Pal.Dark(Pal.Concrete, 0.06f), Pal.Light(Pal.Concrete, 0.09f));
                // expansion joints
                for (int gx = 0; gx < TW; gx += 32)
                    for (int gy = 0; gy < TH; gy += 32)
                        p.RectOutline(gx, gy, 32, 32, 1, Pal.A(Pal.ConcreteDark, 0.55f));
                for (int i = 0; i < 120; i++)
                    p.Set(rng.Next(TW), rng.Next(TH), Pal.Dark(Pal.Concrete, 0.16f));
            }
            else
            {
                for (int i = 0; i < 9000; i++)
                {
                    int x = rng.Next(TW), y = rng.Next(TH);
                    p.Set(x, y, rng.Next(3) == 0 ? Pal.GrassLight : Pal.GrassDark);
                }
                // grass clumps
                for (int i = 0; i < 260; i++)
                {
                    int x = rng.Next(TW), y = rng.Next(TH);
                    p.Set(x, y, Pal.Dark(Pal.GrassLight, 0.1f));
                    p.Set(x + 1, y, Pal.GrassLight);
                    p.Set(x, y + 1, Pal.GrassDark);
                }
            }
        }

        static void Edge(Px p, RectDef r, Color32 c, int t)
        {
            int x = PX(r.x - r.w / 2), y = PY(r.y - r.h / 2);
            int w = Mathf.RoundToInt(r.w * PPU), h = Mathf.RoundToInt(r.h * PPU);
            p.RectOutline(x, y, w, h, t, c);
        }

        static void Speckle(Px p, System.Random rng, int count, RectDef r, params Color32[] cols)
        {
            int x0 = PX(r.x - r.w / 2), y0 = PY(r.y - r.h / 2);
            int w = Mathf.RoundToInt(r.w * PPU), h = Mathf.RoundToInt(r.h * PPU);
            for (int i = 0; i < count; i++)
                p.Set(x0 + rng.Next(w), y0 + rng.Next(h), cols[rng.Next(cols.Length)]);
        }

        static void Cracks(Px p, System.Random rng, RectDef r, int count)
        {
            int x0 = PX(r.x - r.w / 2), y0 = PY(r.y - r.h / 2);
            int w = Mathf.RoundToInt(r.w * PPU), h = Mathf.RoundToInt(r.h * PPU);
            for (int i = 0; i < count; i++)
            {
                int x = x0 + rng.Next(w), y = y0 + rng.Next(h);
                int len = 8 + rng.Next(30);
                int dir = rng.Next(4);
                for (int k = 0; k < len; k++)
                {
                    if (rng.Next(10) == 0) dir = Mathf.Clamp(dir + rng.Next(-1, 2), 0, 3);
                    int nx = dir == 0 ? 1 : dir == 1 ? -1 : 0;
                    int ny = dir == 2 ? 1 : dir == 3 ? -1 : 0;
                    x += nx; y += ny;
                    p.Set(x, y, Pal.A(Pal.Ink, 0.35f));
                    if (rng.Next(4) == 0) p.Set(x + 1, y, Pal.A(Pal.Ink, 0.18f));
                }
            }
        }

        static void PaintSidewalk(Px p, RectDef r, System.Random rng)
        {
            int x = PX(r.x - r.w / 2), y = PY(r.y - r.h / 2);
            int w = Mathf.RoundToInt(r.w * PPU), h = Mathf.RoundToInt(r.h * PPU);
            p.FillRect(x, y, w, h, Pal.Concrete);
            for (int gx = x; gx < x + w; gx += 16)
                p.VLine(gx, y, h, Pal.A(Pal.ConcreteDark, 0.5f));
            for (int gy = y; gy < y + h; gy += 16)
                p.HLine(x, gy, w, Pal.A(Pal.ConcreteDark, 0.5f));
            p.RectOutline(x, y, w, h, 2, Pal.ConcreteDark);
            for (int i = 0; i < 300; i++)
                p.Set(x + rng.Next(w), y + rng.Next(h), rng.Next(2) == 0 ? Pal.Light(Pal.Concrete, 0.07f) : Pal.Dark(Pal.Concrete, 0.07f));
        }

        static void PaintPlanter(Px p, RectDef r, System.Random rng)
        {
            int x = PX(r.x - r.w / 2), y = PY(r.y - r.h / 2);
            int w = Mathf.RoundToInt(r.w * PPU), h = Mathf.RoundToInt(r.h * PPU);
            p.FillRect(x, y, w, h, Pal.GrassDark);
            p.RectOutline(x, y, w, h, 3, Pal.CurbDark);
            p.RectOutline(x + 3, y + 3, w - 6, h - 6, 2, Pal.Curb);
            p.FillRect(x + 6, y + 6, w - 12, h - 12, Pal.Grass);
            for (int i = 0; i < w * h / 30; i++)
                p.Set(x + 6 + rng.Next(Mathf.Max(1, w - 12)), y + 6 + rng.Next(Mathf.Max(1, h - 12)),
                    rng.Next(3) == 0 ? Pal.GrassLight : Pal.GrassDark);
        }

        static void PaintWall(Px p, WallDef w)
        {
            int x = PX(w.x - w.w / 2), y = PY(w.y - w.h / 2);
            int ww = Mathf.Max(1, Mathf.RoundToInt(w.w * PPU)), hh = Mathf.Max(1, Mathf.RoundToInt(w.h * PPU));
            Color32 fill, edge, hi;
            if (w.style == WallStyle.Curb) { fill = Pal.Curb; edge = Pal.CurbDark; hi = Pal.Light(Pal.Curb, 0.18f); }
            else if (w.style == WallStyle.Metal) { fill = Pal.C("#4b515c"); edge = Pal.C("#22262d"); hi = Pal.C("#666d7a"); }
            else { fill = Pal.C("#b9b2a2"); edge = Pal.C("#5b5852"); hi = Pal.Light(Pal.Curb, 0.1f); }

            p.FillRect(x, y, ww, hh, fill);
            p.RectOutline(x, y, ww, hh, 1, edge);
            if (hh > 4) p.HLine(x + 1, y + hh - 2, Mathf.Max(1, ww - 2), hi);
            if (ww > 4) p.VLine(x + 1, y + 1, Mathf.Max(1, hh - 2), hi);
            // curb shading
            if (hh > 5) p.FillRect(x + 1, y + 1, ww - 2, 1, Pal.A(Pal.Ink, 0.25f));
            if (ww > 5) p.FillRect(x + ww - 2, y + 1, 1, hh - 2, Pal.A(Pal.Ink, 0.25f));
        }

        static void PaintBay(Px p, BayDef b, System.Random rng, bool target, Color32 floorCol, GroundStyle ground)
        {
            float rad = b.ang * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad), c = Mathf.Cos(rad);
            float hw = b.w * 0.5f, hd = b.d * 0.5f;

            Vector2 f = new Vector2(s, c), r = new Vector2(c, -s);
            Vector2 center = new Vector2(b.x, b.y);

            Vector2 backL = center + f * hd - r * hw;
            Vector2 backR = center + f * hd + r * hw;
            Vector2 frontL = center - f * hd - r * hw;
            Vector2 frontR = center - f * hd + r * hw;

            Color32 col = target ? Pal.Mint : Pal.Paint;
            float alpha = target ? 0.95f : 0.75f;

            PaintSeg(p, backL, backR, 2, Pal.A(col, alpha), rng);
            PaintSeg(p, backL, frontL, 2, Pal.A(col, alpha), rng);
            PaintSeg(p, backR, frontR, 2, Pal.A(col, alpha), rng);

            bool angled = Mathf.Abs(s) > 0.2f && Mathf.Abs(c) > 0.2f;
            if (angled)
                PaintSeg(p, frontL, frontR, 2, Pal.A(col, alpha * 0.45f), rng);

            // worn entrance ticks
            PaintSeg(p, frontL, frontL + f * 0.18f, 2, Pal.A(col, alpha * 0.6f), rng);
            PaintSeg(p, frontR, frontR + f * 0.18f, 2, Pal.A(col, alpha * 0.6f), rng);

            if (b.number > 0)
            {
                Vector2 numPos = center + f * (hd - 0.55f);
                string num = b.number.ToString();
                int scale = 2;
                int tw = num.Length * 6 * scale;
                int px = PX(numPos.x) - tw / 2;
                int py = PY(numPos.y) - 7 * scale / 2;
                foreach (char ch in num)
                {
                    p.Glyph(ch, px, py, scale, Pal.A(Pal.Paint, 0.33f));
                    px += 6 * scale;
                }
            }

            // oil stain (opaque so it reads on both asphalt and concrete)
            if (!target && rng.Next(3) == 0)
            {
                float ox = (float)(rng.NextDouble() - 0.5) * b.w * 0.35f;
                float oy = (float)(rng.NextDouble() - 0.5) * b.d * 0.3f;
                Vector2 op = center + r * ox + f * oy;
                var stain = Pal.Dark(floorCol, ground == GroundStyle.Concrete ? 0.14f : 0.18f);
                p.Ellipse(PX(op.x), PY(op.y), 10 + rng.Next(5), 7 + rng.Next(4), stain);
                p.Ellipse(PX(op.x), PY(op.y), 6 + rng.Next(4), 4 + rng.Next(3), stain);
            }
        }

        static void PaintSeg(Px p, Vector2 a, Vector2 b, int thick, Color32 col, System.Random rng)
        {
            int x0 = PX(a.x), y0 = PY(a.y), x1 = PX(b.x), y1 = PY(b.y);
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int step = 0;
            while (true)
            {
                step++;
                bool worn = (step % 23) < 1 && rng.Next(3) == 0;
                if (!worn)
                {
                    p.FillRect(x0 - thick / 2, y0 - thick / 2, thick, thick, col);
                    if (rng.Next(20) == 0) p.Set(x0, y0, Pal.A(Pal.Asphalt, 0.5f));
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        static void PaintParallelStall(Px p, LevelDef d, System.Random rng)
        {
            float rad = d.SlotAng * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad), c = Mathf.Cos(rad);
            Vector2 f = new Vector2(s, c), r = new Vector2(c, -s);
            Vector2 center = d.SlotCenter;
            float hw = d.SlotSize.x * 0.5f, hd = d.SlotSize.y * 0.5f;
            // Rear edge against the kerb and short end caps, like a painted street stall.
            PaintSeg(p, center + r * hw - f * hd, center + r * hw + f * hd, 2, Pal.A(Pal.Mint, 0.8f), rng);
            PaintSeg(p, center + f * hd - r * hw, center + f * hd + r * hw, 2, Pal.A(Pal.Mint, 0.5f), rng);
            PaintSeg(p, center - f * hd - r * hw, center - f * hd + r * hw, 2, Pal.A(Pal.Mint, 0.5f), rng);
        }

        static void Decorations(Px p, LevelDef d, System.Random rng)
        {
            // Manhole covers in the driving aisle
            if (d.Ground == GroundStyle.Asphalt && d.Asphalt.Count > 0)
            {
                var r = d.Asphalt[0];
                p.EllipseOutline(PX(r.x * 0.3f), PY(-6.9f), 8, 8, 2, Pal.A(Pal.Ink, 0.45f));
                p.Ellipse(PX(r.x * 0.3f), PY(-6.9f), 6, 6, Pal.A(Pal.Ink, 0.22f));
            }

            // Painted arrows guiding into the lot
            if (d.Name == "FIRST BAY" || d.Name == "ANGLED SQUEEZE")
            {
                PaintedArrow(p, new Vector2(0f, -6.0f), 0f);
                PaintedArrow(p, new Vector2(0f, 6.6f), 180f);
            }
            if (d.Name == "T-JUNCTION")
            {
                PaintedArrow(p, new Vector2(0f, -5.4f), 0f);
                PaintedArrow(p, new Vector2(-1.0f, 3.0f), -90f);
            }
            if (d.Ground == GroundStyle.Concrete)
            {
                // pools of ceiling light down the driving aisle
                var pool = Pal.Lerp(Pal.Concrete, Pal.Light(Pal.Concrete, 0.4f), 0.55f);
                for (float yy = -6.4f; yy <= 6.4f; yy += 3.2f)
                    p.Ellipse(PX(0f), PY(yy), 34f, 18f, pool);

                // hazard band across the roller door at the bottom
                int bandY = PY(-8.45f);
                p.FillRect(PX(-5.2f), bandY, Mathf.RoundToInt(10.4f * PPU), 6, Pal.A(Pal.Yellow, 0.55f));
                for (int i = 0; i < 40; i++)
                {
                    int x = PX(-5.2f) + i * 5;
                    p.Line(x, bandY, x + 4, bandY + 5, 2, Pal.A(Pal.Ink, 0.55f));
                }

                for (int i = 0; i < 60; i++)
                {
                    int x = PX(-1.4f) + rng.Next(Mathf.Max(1, PX(1.4f) - PX(-1.4f)));
                    int y = rng.Next(TH);
                    p.VLine(x, y, 6 + rng.Next(16), Pal.A(Pal.Ink, 0.05f));
                }
            }
        }

        static void PaintedArrow(Px p, Vector2 pos, float ang)
        {
            float rad = ang * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad), c = Mathf.Cos(rad);
            Vector2 f = new Vector2(s, c), r = new Vector2(c, -s);
            Vector2 a = pos;
            Color32 col = Pal.A(Pal.Paint, 0.3f);

            Vector2 shaftA = a - f * 0.85f, shaftB = a + f * 0.65f;
            PaintSeg(p, shaftA, shaftB, 3, col, new System.Random(1));
            Vector2 tip = a + f * 1.15f;
            PaintSeg(p, tip, a + f * 0.45f - r * 0.45f, 3, col, new System.Random(2));
            PaintSeg(p, tip, a + f * 0.45f + r * 0.45f, 3, col, new System.Random(3));
        }

        static void PaintWord(Px p, LevelDef d)
        {
            string w = d.GroundWord;
            int scale = Mathf.Max(1, Mathf.RoundToInt(d.GroundWordScale));
            int tw = w.Length * 6 * scale;
            int px = PX(d.GroundWordPos.x) - tw / 2;
            int py = PY(d.GroundWordPos.y) - 7 * scale / 2;
            foreach (char ch in w)
            {
                p.Glyph(ch, px, py, scale, Pal.A(Pal.Paint, 0.22f));
                px += 6 * scale;
            }
        }

        public static Sprite Vignette()
        {
            var p = new Px(64, 114, new Color32(0, 0, 0, 0));
            for (int y = 0; y < 114; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float nx = Mathf.Abs((x + 0.5f) / 64f * 2f - 1f);
                    float ny = Mathf.Abs((y + 0.5f) / 114f * 2f - 1f);
                    float d = Mathf.Max(nx * nx * 0.9f, ny * ny * 0.65f);
                    float a = Mathf.Clamp01((d - 0.55f) / 0.45f) * 0.55f;
                    p.Set(x, y, new Color32(4, 5, 9, (byte)(a * 255f)));
                }
            }
            return p.ToSprite("vignette");
        }
    }
}
