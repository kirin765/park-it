using System.Collections.Generic;
using UnityEngine;

namespace Parking
{
    public class UiButton
    {
        public Rect R;
        public Transform T;
        public SpriteRenderer Bg;
        public PixelText Label;
        public SpriteRenderer Icon;
        public Color Base;
        public bool Enabled = true;
        public bool Down;
        public bool Toggled;
        public bool PlayClickSound = true;
        public System.Action<UiButton> OnClick;

        int _pid = int.MinValue;
        Vector3 _home;

        public void Init(Transform parent, Vector2 center, Vector2 size, string text, Color color,
            int order, int textScale = 3, Sprite icon = null, System.Action<UiButton> onClick = null)
        {
            var root = new GameObject("btn_" + (string.IsNullOrEmpty(text) ? "icon" : text));
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(center.x, center.y, 0f);
            T = root.transform;
            _home = T.localPosition;
            R = new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
            Base = color;
            OnClick = onClick;

            Bg = UiKit.Panel(T, Vector2.zero, size, color, order);

            float labelShift = 0f;
            if (icon != null)
            {
                float iconSize = Mathf.Min(0.8f, size.y * 0.46f);
                float textW = string.IsNullOrEmpty(text) ? 0f : PixelText.MeasureWidth(text, textScale);
                float total = textW + iconSize + (textW > 0f ? 0.3f : 0f);
                if (textW <= 0f || total <= size.x * 0.84f)
                {
                    Icon = UiKit.Image(T, Vector2.zero, new Vector2(iconSize, iconSize), icon, Color.white, order + 12);
                    if (textW > 0f)
                    {
                        Icon.transform.localPosition = new Vector3(-total * 0.5f + iconSize * 0.5f, 0f, 0f);
                        labelShift = total * 0.5f - textW * 0.5f;
                    }
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                Label = PixelText.Create(T, text, new Vector3(labelShift, 0f, 0f), textScale, Color.white, 0.5f, 0.5f, order + 14, true);
            }
        }

        public void SetText(string s)
        {
            if (Label != null) Label.SetText(s);
        }

        public void SetIcon(Sprite s)
        {
            if (Icon != null) Icon.sprite = s;
        }

        public void Tick(InputService input, HashSet<int> used)
        {
            if (T == null) return;
            ClickedThisFrame = false;

            if (_pid != int.MinValue)
            {
                var p = input.Find(_pid);
                if (p == null)
                {
                    _pid = int.MinValue;
                    Down = false;
                }
                else
                {
                    used.Add(_pid);
                    Down = p.Down;
                    if (p.JustUp)
                    {
                        if (Contains(p.Screen) && Enabled) Fire();
                        _pid = int.MinValue;
                        Down = false;
                    }
                    else if (!ContainsExpanded(p.Screen))
                    {
                        _pid = int.MinValue;
                        Down = false;
                    }
                }
            }
            else if (Enabled)
            {
                for (int i = 0; i < input.Pointers.Count; i++)
                {
                    var p = input.Pointers[i];
                    if (!p.JustDown || used.Contains(p.Id)) continue;
                    if (Contains(p.Screen))
                    {
                        _pid = p.Id;
                        Down = true;
                        used.Add(p.Id);
                        break;
                    }
                }
            }

            // visuals
            if (Bg != null)
            {
                Color c = Enabled ? Base : new Color(0.36f, 0.38f, 0.42f, Base.a);
                if (Down) c = Pal.Dark(c, 0.18f);
                if (Toggled) c = Pal.Light(c, 0.18f);
                Bg.color = c;
            }
            T.localPosition = _home + (Down ? new Vector3(0f, -0.055f, 0f) : Vector3.zero);
        }

        public bool ClickedThisFrame { get; private set; }

        void Fire()
        {
            ClickedThisFrame = true;
            if (PlayClickSound) GameApp.I.Audio.Click();
            OnClick?.Invoke(this);
        }

        bool Contains(Vector2 screen)
        {
            return R.Contains(UiKit.ToWorld(screen));
        }

        bool ContainsExpanded(Vector2 screen)
        {
            var w = UiKit.ToWorld(screen);
            var e = R;
            e.xMin -= 0.35f; e.xMax += 0.35f; e.yMin -= 0.35f; e.yMax += 0.35f;
            return e.Contains(w);
        }
    }

    /// <summary>A spinnable steering wheel with a 900° lock-to-lock range that holds its angle.</summary>
    public class UiSteerWheel
    {
        public Vector2 Center;
        public float Radius = 1f;
        public float MaxAngle = 450f;        // 900° lock-to-lock
        public float FullSteerAngle = 180f;  // wheel degrees that equal full steering lock
        public float Angle;
        public float Value;
        public bool Held;

        Transform _wheel;
        int _pid = int.MinValue;
        float _lastAng;
        bool _hasLast;
        bool _centerGrab;
        float _dragX;
        float _dragAngle;
        const float DragDegreesPerUnit = 150f;

        public void Init(Transform parent, Vector2 center, float radius, int order)
        {
            Center = center;
            Radius = radius;

            var root = new GameObject("steer_wheel");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(center.x, center.y, 0f);
            _wheel = root.transform;

            UiKit.Image(root.transform, Vector2.zero, new Vector2(radius * 2.05f, radius * 2.05f),
                Art.SteerWheel, Color.white, order);
        }

        public void Tick(InputService input, float speed01)
        {
            Held = false;

            if (_pid != int.MinValue)
            {
                var p = input.Find(_pid);
                if (p == null || p.JustUp)
                {
                    _pid = int.MinValue;
                    _centerGrab = false;
                }
                else
                {
                    Held = true;
                    Vector2 w = UiKit.ToWorld(p.Screen);

                    if (_centerGrab)
                    {
                        // Grabbed the hub: map a horizontal drag to wheel rotation.
                        Angle = Mathf.Clamp(_dragAngle - (w.x - _dragX) * DragDegreesPerUnit, -MaxAngle, MaxAngle);
                    }
                    else
                    {
                        Vector2 d = w - Center;
                        if (d.magnitude > Radius * 0.18f)
                        {
                            float a = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                            if (_hasLast)
                            {
                                float delta = Mathf.Clamp(Mathf.DeltaAngle(_lastAng, a), -45f, 45f);
                                Angle = Mathf.Clamp(Angle + delta, -MaxAngle, MaxAngle);
                            }
                            _lastAng = a;
                            _hasLast = true;
                        }
                    }
                }
            }
            else
            {
                _hasLast = false;
                _centerGrab = false;
                for (int i = 0; i < input.Pointers.Count; i++)
                {
                    var p = input.Pointers[i];
                    if (!p.JustDown) continue;
                    Vector2 w = UiKit.ToWorld(p.Screen);
                    if ((w - Center).sqrMagnitude <= Radius * Radius * 1.55f)
                    {
                        _pid = p.Id;
                        Held = true;
                        Vector2 d = w - Center;
                        if (d.magnitude <= Radius * 0.35f)
                        {
                            _centerGrab = true;
                            _dragX = w.x;
                            _dragAngle = Angle;
                        }
                        else
                        {
                            _lastAng = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                            _hasLast = true;
                        }
                        break;
                    }
                }
            }

            // Self-centre slowly only while the car is rolling; at a standstill the wheel stays put.
            if (!Held && speed01 > 0.08f)
            {
                float rate = Mathf.Lerp(12f, 70f, Mathf.Clamp01(speed01));
                Angle = Mathf.MoveTowards(Angle, 0f, rate * Time.unscaledDeltaTime);
            }

            Value = Mathf.Clamp(-Angle / FullSteerAngle, -1f, 1f);
            if (_wheel != null)
                _wheel.localRotation = Quaternion.Euler(0f, 0f, Angle);
        }
    }

    public static class UiKit
    {
        public static Vector2 ToWorld(Vector2 screen)
        {
            if (GameApp.I == null || GameApp.I.Cam == null) return screen;
            var cam = GameApp.I.Cam;
            Vector3 p = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            return new Vector2(p.x, p.y);
        }

        public static SpriteRenderer Panel(Transform parent, Vector2 center, Vector2 size, Color color, int order)
        {
            var go = new GameObject("panel");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(center.x, center.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Art.PanelMask;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public static SpriteRenderer Image(Transform parent, Vector2 center, Vector2 size, Sprite sprite, Color color, int order)
        {
            var go = new GameObject("img");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(center.x, center.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            if (sprite != null)
            {
                var b = sprite.bounds.size;
                float sx = b.x > 0.0001f ? size.x / b.x : 1f;
                float sy = b.y > 0.0001f ? size.y / b.y : 1f;
                go.transform.localScale = new Vector3(sx, sy, 1f);
            }
            return sr;
        }

        public static SpriteRenderer RawImage(Transform parent, Sprite sprite, Color color, int order, string name = "img")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        public static SpriteRenderer Dim(Transform parent, Color color, int order)
        {
            var sr = RawImage(parent, Art.White, color, order, "dim");
            sr.transform.localScale = new Vector3(45f, 80f, 1f);
            return sr;
        }

        public static PixelText Text(Transform parent, string text, Vector2 pos, int scale, Color color,
            float ax = 0.5f, float ay = 0.5f, int order = 2030, bool shadow = true)
        {
            return PixelText.Create(parent, text, new Vector3(pos.x, pos.y, 0f), scale, color, ax, ay, order, shadow);
        }

        public static UiButton Button(Transform parent, Vector2 center, Vector2 size, string text, Color color,
            int order = 2020, int textScale = 3, Sprite icon = null, System.Action<UiButton> onClick = null)
        {
            var b = new UiButton();
            b.Init(parent, center, size, text, color, order, textScale, icon, onClick);
            return b;
        }
    }

    public abstract class UiScreen
    {
        public GameApp App;
        public Transform Root;
        public readonly List<UiButton> Buttons = new List<UiButton>();
        static readonly HashSet<int> Used = new HashSet<int>();

        public void Setup(GameApp app, string name)
        {
            App = app;
            Root = new GameObject("Screen_" + name).transform;
            Root.SetParent(app.UiRoot, false);
        }

        public virtual void Build() { }

        public void Tick(float dt)
        {
            Used.Clear();
            for (int i = 0; i < Buttons.Count; i++)
                Buttons[i].Tick(App.Inputs, Used);
            OnTick(dt);
        }

        public virtual void OnTick(float dt) { }

        public virtual void Show(bool visible)
        {
            if (Root != null) Root.gameObject.SetActive(visible);
        }

        public void Destroy()
        {
            if (Root != null) Object.Destroy(Root.gameObject);
            Root = null;
            Buttons.Clear();
        }
    }
}
