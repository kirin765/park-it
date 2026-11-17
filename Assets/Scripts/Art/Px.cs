using System;
using UnityEngine;

namespace Parking
{
    /// <summary>Tiny software pixel canvas used to author all art procedurally.</summary>
    public class Px
    {
        public readonly int W, H;
        public readonly Color32[] D;

        public Px(int w, int h, Color32 fill)
        {
            W = w; H = h;
            D = new Color32[w * h];
            for (int i = 0; i < D.Length; i++) D[i] = fill;
        }

        public void Set(int x, int y, Color32 c)
        {
            if ((uint)x < (uint)W && (uint)y < (uint)H) D[y * W + x] = c;
        }

        public Color32 Get(int x, int y)
        {
            if ((uint)x < (uint)W && (uint)y < (uint)H) return D[y * W + x];
            return new Color32(0, 0, 0, 0);
        }

        public void Blend(int x, int y, Color32 c)
        {
            if (c.a == 0) return;
            if ((uint)x >= (uint)W || (uint)y >= (uint)H) return;
            Color32 dst = D[y * W + x];
            float a = c.a / 255f;
            D[y * W + x] = new Color32(
                (byte)(c.r * a + dst.r * (1 - a)),
                (byte)(c.g * a + dst.g * (1 - a)),
                (byte)(c.b * a + dst.b * (1 - a)),
                (byte)Mathf.Min(255, dst.a + c.a));
        }

        public void FillRect(int x, int y, int w, int h, Color32 c)
        {
            int x0 = Mathf.Max(0, x), y0 = Mathf.Max(0, y);
            int x1 = Mathf.Min(W, x + w), y1 = Mathf.Min(H, y + h);
            for (int yy = y0; yy < y1; yy++)
                for (int xx = x0; xx < x1; xx++)
                    D[yy * W + xx] = c;
        }

        public void RectOutline(int x, int y, int w, int h, int t, Color32 c)
        {
            FillRect(x, y, w, t, c);
            FillRect(x, y + h - t, w, t, c);
            FillRect(x, y, t, h, c);
            FillRect(x + w - t, y, t, h, c);
        }

        public void RoundRect(int x, int y, int w, int h, int r, Color32 c)
        {
            if (w <= 0 || h <= 0) return;
            r = Mathf.Clamp(r, 0, Mathf.Min(w, h) / 2);
            FillRect(x + r, y, w - r * 2, h, c);
            FillRect(x, y + r, r, h - r * 2, c);
            FillRect(x + w - r, y + r, r, h - r * 2, c);
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    float dx = r - i - 0.5f, dy = r - j - 0.5f;
                    if (dx * dx + dy * dy <= r * r)
                    {
                        Set(x + i, y + j, c);
                        Set(x + w - 1 - i, y + j, c);
                        Set(x + i, y + h - 1 - j, c);
                        Set(x + w - 1 - i, y + h - 1 - j, c);
                    }
                }
            }
        }

        public void HLine(int x, int y, int len, Color32 c)
        {
            for (int i = 0; i < len; i++) Set(x + i, y, c);
        }

        public void VLine(int x, int y, int len, Color32 c)
        {
            for (int i = 0; i < len; i++) Set(x, y + i, c);
        }

        public void Line(int x0, int y0, int x1, int y1, int thick, Color32 c)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int half = Mathf.Max(0, thick / 2);
            while (true)
            {
                int r = thick - 1 - half;
                FillRect(x0 - half, y0 - half, thick, thick, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public void Disk(int cx, int cy, int r, Color32 c)
        {
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= r * r) Set(cx + x, cy + y, c);
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color32 c)
        {
            int x0 = Mathf.FloorToInt(cx - rx), x1 = Mathf.CeilToInt(cx + rx);
            int y0 = Mathf.FloorToInt(cy - ry), y1 = Mathf.CeilToInt(cy + ry);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                    if (dx * dx + dy * dy <= 1f) Set(x, y, c);
                }
            }
        }

        public void EllipseOutline(float cx, float cy, float rx, float ry, float t, Color32 c)
        {
            int x0 = Mathf.FloorToInt(cx - rx), x1 = Mathf.CeilToInt(cx + rx);
            int y0 = Mathf.FloorToInt(cy - ry), y1 = Mathf.CeilToInt(cy + ry);
            float inner = 1f - t / Mathf.Min(rx, ry);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d <= 1f && d >= inner * inner) Set(x, y, c);
                }
            }
        }

        /// <summary>Fills a rotated rectangle. Angle in degrees: 0 = up (+Y), 90 = right (+X).</summary>
        public void FillRotRect(float cx, float cy, float w, float h, float angDeg, Color32 c)
        {
            float rad = angDeg * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad), co = Mathf.Cos(rad);
            float hw = w * 0.5f, hh = h * 0.5f;
            float ext = Mathf.Abs(hw * co) + Mathf.Abs(hh * s) + 2f;
            float exty = Mathf.Abs(hw * s) + Mathf.Abs(hh * co) + 2f;
            int x0 = Mathf.FloorToInt(cx - ext), x1 = Mathf.CeilToInt(cx + ext);
            int y0 = Mathf.FloorToInt(cy - exty), y1 = Mathf.CeilToInt(cy + exty);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float px = x + 0.5f - cx, py = y + 0.5f - cy;
                    float u = px * co - py * s;   // along width (right)
                    float v = px * s + py * co;   // along depth (forward)
                    if (Mathf.Abs(u) <= hw && Mathf.Abs(v) <= hh) Set(x, y, c);
                }
            }
        }

        public void RotRectOutline(float cx, float cy, float w, float h, float angDeg, int thick, Color32 c)
        {
            float rad = angDeg * Mathf.Deg2Rad;
            float s = Mathf.Sin(rad), co = Mathf.Cos(rad);
            float hw = w * 0.5f, hh = h * 0.5f;
            Vector2 A = Corner(cx, cy, -hw, -hh, s, co);
            Vector2 B = Corner(cx, cy, hw, -hh, s, co);
            Vector2 C = Corner(cx, cy, hw, hh, s, co);
            Vector2 Dc = Corner(cx, cy, -hw, hh, s, co);
            Seg(A, B, thick, c);
            Seg(B, C, thick, c);
            Seg(C, Dc, thick, c);
            Seg(Dc, A, thick, c);
        }

        static Vector2 Corner(float cx, float cy, float u, float v, float s, float co)
        {
            return new Vector2(cx + u * co + v * s, cy - u * s + v * co);
        }

        void Seg(Vector2 a, Vector2 b, int thick, Color32 c)
        {
            Line(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y), thick, c);
        }

        public void Noise(System.Random rng, int count, params Color32[] colors)
        {
            for (int i = 0; i < count; i++)
            {
                int x = rng.Next(W), y = rng.Next(H);
                Set(x, y, colors[rng.Next(colors.Length)]);
            }
        }

        public void Glyph(char ch, int x, int y, int scale, Color32 c)
        {
            string[] rows = PixelFont.Rows(ch);
            for (int r = 0; r < rows.Length; r++)
            {
                string row = rows[r];
                for (int i = 0; i < row.Length; i++)
                {
                    if (row[i] == '1')
                        FillRect(x + i * scale, y + (PixelFont.GH - 1 - r) * scale, scale, scale, c);
                }
            }
        }

        public Texture2D ToTexture(string name)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false, false);
            tex.name = name;
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels32(D);
            tex.Apply(false, false);
            return tex;
        }

        public Sprite ToSprite(string name, float ppu = 16f, Vector2? pivot = null)
        {
            var tex = ToTexture(name);
            var sprite = Sprite.Create(tex, new Rect(0, 0, W, H), pivot ?? new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        public Sprite ToSlicedSprite(string name, int border, float ppu = 16f)
        {
            var tex = ToTexture(name);
            var sprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            sprite.name = name;
            return sprite;
        }
    }
}
