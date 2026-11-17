using System.Collections.Generic;
using UnityEngine;

namespace Parking
{
    /// <summary>Sprite-based bitmap text. Anchored by its lower-left corner unless anchor is set.</summary>
    public class PixelText : MonoBehaviour
    {
        public string Text = "";
        public int Scale = 3;
        public Color Color = Color.white;
        public float AnchorX = 0.5f;
        public float AnchorY = 0.5f;
        public bool Shadow = false;
        public Color ShadowColor = new Color(0f, 0f, 0f, 0.55f);
        public int SortingOrder = 2030;
        public float Alpha = 1f;

        readonly List<SpriteRenderer> _chars = new List<SpriteRenderer>();
        string _builtText;
        int _builtScale = -1;
        bool _builtShadow;
        float _width, _height;

        public float Width => _width;
        public float Height => _height;

        public static float MeasureWidth(string text, int scale)
        {
            int longest = 0;
            foreach (var line in (text ?? "").Split('\n'))
                longest = Mathf.Max(longest, line.Length);
            return longest <= 0 ? 0f : (longest * PixelFont.ADV - 1) * scale / PixelFont.PPU;
        }

        public static float MeasureHeight(string text, int scale)
        {
            int lines = Mathf.Max(1, (text ?? "").Split('\n').Length);
            return (lines * PixelFont.LINE - 1) * scale / PixelFont.PPU;
        }

        public static PixelText Create(Transform parent, string text, Vector3 localPos, int scale, Color color,
            float anchorX = 0.5f, float anchorY = 0.5f, int sortingOrder = 2030, bool shadow = false, string name = "Text")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var t = go.AddComponent<PixelText>();
            t.Scale = scale;
            t.Color = color;
            t.AnchorX = anchorX;
            t.AnchorY = anchorY;
            t.SortingOrder = sortingOrder;
            t.Shadow = shadow;
            t.Text = text ?? "";
            t.Rebuild();
            return t;
        }

        public void SetText(string text)
        {
            text = (text ?? "").ToUpperInvariant();
            if (_builtText == text) return;
            Text = text;
            Rebuild();
        }

        public void SetColor(Color c)
        {
            Color = c;
            ApplyColors();
        }

        public void SetAlpha(float a)
        {
            Alpha = Mathf.Clamp01(a);
            ApplyColors();
        }

        public void SetScale(int s)
        {
            s = Mathf.Clamp(s, 1, 24);
            if (Scale == s) return;
            Scale = s;
            Rebuild();
        }

        public void SetSorting(int order)
        {
            SortingOrder = order;
            foreach (var r in _chars) r.sortingOrder = order;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        void ApplyColors()
        {
            var c = Color;
            c.a *= Alpha;
            foreach (var r in _chars)
            {
                if (r.gameObject.name == "shadow") r.color = ShadowColor;
                else r.color = c;
            }
        }

        public void Rebuild()
        {
            foreach (var r in _chars)
                if (r != null) Destroy(r.gameObject);
            _chars.Clear();

            string text = string.IsNullOrEmpty(Text) ? "" : Text.ToUpperInvariant();
            string[] lines = text.Split('\n');
            int scale = Mathf.Clamp(Scale, 1, 24);
            float px = 1f / PixelFont.PPU;
            float stepX = PixelFont.ADV * scale * px;
            float stepY = PixelFont.LINE * scale * px;
            int longest = 0;
            for (int i = 0; i < lines.Length; i++) longest = Mathf.Max(longest, lines[i].Length);

            float width = longest <= 0 ? 0f : (longest * PixelFont.ADV - 1) * scale * px;
            float height = (lines.Length * PixelFont.LINE - 1) * scale * px;
            _width = width;
            _height = height;

            float ox = -width * AnchorX;
            float oy = -height * AnchorY;

            var col = Color;
            col.a *= Alpha;

            for (int li = 0; li < lines.Length; li++)
            {
                string line = lines[li];
                for (int ci = 0; ci < line.Length; ci++)
                {
                    char ch = line[ci];
                    if (ch == ' ') continue;
                    var sprite = PixelFont.Glyph(ch);
                    float x = ox + ci * stepX;
                    float y = oy - li * stepY;

                    if (Shadow)
                    {
                        var sh = new GameObject("shadow");
                        sh.transform.SetParent(transform, false);
                        sh.transform.localPosition = new Vector3(x + scale * px, y - scale * px, 0f);
                        sh.transform.localScale = Vector3.one * scale;
                        var sr = sh.AddComponent<SpriteRenderer>();
                        sr.sprite = sprite;
                        sr.color = ShadowColor;
                        sr.sortingOrder = SortingOrder - 1;
                        _chars.Add(sr);
                    }

                    var go = new GameObject("g");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(x, y, 0f);
                    go.transform.localScale = Vector3.one * scale;
                    var g = go.AddComponent<SpriteRenderer>();
                    g.sprite = sprite;
                    g.color = col;
                    g.sortingOrder = SortingOrder;
                    _chars.Add(g);
                }
            }

            _builtText = Text;
            _builtScale = scale;
            _builtShadow = Shadow;
        }

        void LateUpdate()
        {
            if (_builtText != Text.ToUpperInvariant() || _builtScale != Scale || _builtShadow != Shadow)
                Rebuild();
        }
    }
}
