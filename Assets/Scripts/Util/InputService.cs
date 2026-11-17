using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Parking
{
    public class PointerState
    {
        public int Id;
        public Vector2 Screen;
        public bool Down;
        public bool JustDown;
        public bool JustUp;
    }

    /// <summary>Multi-touch (steer + pedal at once), mouse and keyboard through the Input System.</summary>
    public class InputService
    {
        public readonly List<PointerState> Pointers = new List<PointerState>();

        public bool Left, Right, Gas, Brake;
        public bool Confirm, Cancel, PausePressed, AnyTap;
        public Vector2 MouseScreen;

        readonly HashSet<int> _prevDown = new HashSet<int>();
        readonly HashSet<int> _seen = new HashSet<int>();
        readonly Dictionary<int, Vector2> _lastPos = new Dictionary<int, Vector2>();
        readonly List<PointerState> _fresh = new List<PointerState>();

        public bool AnyDown => Pointers.Count > 0;

        public PointerState Find(int id)
        {
            for (int i = 0; i < Pointers.Count; i++)
                if (Pointers[i].Id == id) return Pointers[i];
            return null;
        }

        public void Update()
        {
            _fresh.Clear();
            bool anyTap = false;

            Left = Right = Gas = Brake = false;
            Confirm = Cancel = PausePressed = false;

#if ENABLE_INPUT_SYSTEM
            var ts = Touchscreen.current;
            if (ts != null)
            {
                var touches = ts.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    var t = touches[i];
                    bool pressed = t.press.isPressed;
                    bool released = t.press.wasReleasedThisFrame;
                    if (!pressed && !released) continue;
                    int id = t.touchId.ReadValue();
                    Vector2 pos = t.position.ReadValue();
                    AddPointer(id, pos, pressed, ref anyTap);
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 mp = mouse.position.ReadValue();
                MouseScreen = mp;
                bool pressed = mouse.leftButton.isPressed;
                bool released = mouse.leftButton.wasReleasedThisFrame;
                if (pressed || released) AddPointer(-7777, mp, pressed, ref anyTap);
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                Left = kb.aKey.isPressed || kb.leftArrowKey.isPressed;
                Right = kb.dKey.isPressed || kb.rightArrowKey.isPressed;
                Gas = kb.wKey.isPressed || kb.upArrowKey.isPressed;
                Brake = kb.sKey.isPressed || kb.downArrowKey.isPressed;
                Confirm = kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame ||
                          kb.numpadEnterKey.wasPressedThisFrame;
                Cancel = kb.escapeKey.wasPressedThisFrame;
                PausePressed = kb.escapeKey.wasPressedThisFrame;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    // still reported, handled as up
                }
                AddPointer(t.fingerId, t.position, t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled, ref anyTap);
            }
            if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                AddPointer(-7777, Input.mousePosition, Input.GetMouseButton(0), ref anyTap);
            MouseScreen = Input.mousePosition;

            Left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            Right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            Gas = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            Brake = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            Confirm = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
            Cancel = Input.GetKeyDown(KeyCode.Escape);
            PausePressed = Input.GetKeyDown(KeyCode.Escape);
#else
            MouseScreen = Vector2.zero;
#endif

            Pointers.Clear();
            _seen.Clear();
            for (int i = 0; i < _fresh.Count; i++)
            {
                var p = _fresh[i];
                bool was = _prevDown.Contains(p.Id);
                p.JustDown = p.Down && !was;
                p.JustUp = !p.Down && was;
                if (p.JustDown) anyTap = true;
                Pointers.Add(p);
                _seen.Add(p.Id);
                _lastPos[p.Id] = p.Screen;
            }

            // If the platform's release frame was missed, synthesise it so taps still fire.
            foreach (int id in _prevDown)
            {
                if (_seen.Contains(id)) continue;
                Vector2 pos;
                if (!_lastPos.TryGetValue(id, out pos)) pos = Vector2.zero;
                Pointers.Add(new PointerState { Id = id, Screen = pos, Down = false, JustUp = true });
            }

            _prevDown.Clear();
            for (int i = 0; i < Pointers.Count; i++)
                if (Pointers[i].Down) _prevDown.Add(Pointers[i].Id);

            AnyTap = anyTap;
        }

        void AddPointer(int id, Vector2 screen, bool down, ref bool anyTap)
        {
            for (int i = 0; i < _fresh.Count; i++)
            {
                if (_fresh[i].Id != id) continue;
                _fresh[i].Down = down;
                _fresh[i].Screen = screen;
                return;
            }
            _fresh.Add(new PointerState { Id = id, Screen = screen, Down = down });
        }
    }
}
