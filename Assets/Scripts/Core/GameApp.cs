using UnityEngine;

namespace Parking
{
    public class GameApp : MonoBehaviour
    {
        public static GameApp I;

        public Camera Cam;
        public InputService Inputs;
        public Sfx Audio;
        public Transform WorldRoot;
        public Transform UiRoot;
        public Transform FxRoot;
        public LevelRuntime Level;
        public int CurrentLevel;

        public const int StateTitle = 0;
        public const int StateSelect = 1;
        public const int StatePlaying = 2;
        public const int StatePaused = 3;
        public const int StateWon = 4;

        public int State { get; private set; }

        UiScreen _screen;
        UiScreen _overlay;
        SpriteRenderer _fade;
        float _fadeA;

        void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;

            Art.ResetRuntimeCache();
            PixelFont.ResetCache();

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Physics2D.gravity = Vector2.zero;

            Save.Load();
            Audio = new Sfx(gameObject);
            Audio.Muted = Save.Muted;
            Inputs = new InputService();

            BuildCamera();

            WorldRoot = new GameObject("World").transform;
            WorldRoot.SetParent(transform, false);
            UiRoot = new GameObject("UI").transform;
            UiRoot.SetParent(transform, false);
            FxRoot = new GameObject("FX").transform;
            FxRoot.SetParent(UiRoot, false);

            _fade = UiKit.Dim(UiRoot, new Color(0f, 0f, 0f, 1f), 5000);
            _fadeA = 1f;

            ShowTitle();
        }

        void BuildCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(0f, 0f, -10f);

            Cam = go.AddComponent<Camera>();
            Cam.orthographic = true;
            Cam.orthographicSize = 10f;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = new Color32(7, 8, 12, 255);
            Cam.nearClipPlane = 0.1f;
            Cam.farClipPlane = 60f;
            Cam.allowHDR = false;
            Cam.allowMSAA = false;

            go.AddComponent<AudioListener>();
            var fit = go.AddComponent<ViewportFitter>();
            fit.Cam = Cam;
        }

        void Update()
        {
            Inputs.Update();

            float uiDt = Time.unscaledDeltaTime;

            if (_overlay != null) _overlay.Tick(uiDt);
            else if (_screen != null) _screen.Tick(uiDt);

            if (State == StatePlaying && Level != null)
            {
                Level.Tick(Time.deltaTime);
                if (Inputs.PausePressed) Pause();
            }
            else if (State == StatePaused && Inputs.Cancel)
            {
                Resume();
            }

            if (_fadeA > 0f)
            {
                _fadeA = Mathf.MoveTowards(_fadeA, 0f, uiDt * 2.6f);
                if (_fade != null) _fade.color = new Color(0f, 0f, 0f, _fadeA);
            }

            Audio.Muted = Save.Muted;
            bool driving = State == StatePlaying && Level != null && Level.Started && Level.Car != null;
            Audio.Tick(Time.deltaTime,
                Level != null && Level.Car != null ? Level.Car.Speed01 : 0f,
                Level != null && Level.Car != null ? Level.Car.Throttle : 0f,
                driving);
        }

        // ------------------------------------------------------------------ flow
        public void ShowTitle()
        {
            ClearOverlay();
            ClearScreens();
            ClearLevel();
            State = StateTitle;

            var s = new TitleScreen();
            s.Setup(this, "title");
            s.Build();
            _screen = s;
            FadeTransition();
        }

        public void ShowSelect()
        {
            ClearOverlay();
            ClearScreens();
            ClearLevel();
            State = StateSelect;

            var s = new LevelSelectScreen();
            s.Setup(this, "select");
            s.Build();
            _screen = s;
            FadeTransition();
        }

        public void StartLevel(int index)
        {
            ClearOverlay();
            ClearScreens();
            ClearLevel();

            CurrentLevel = Mathf.Clamp(index, 0, LevelCatalog.Count - 1);
            State = StatePlaying;
            Level = new LevelRuntime(this, LevelCatalog.Get(CurrentLevel));

            var hud = new HudScreen();
            hud.Setup(this, "hud");
            hud.Build();
            _screen = hud;

            FadeTransition();
        }

        public void RetryLevel()
        {
            StartLevel(CurrentLevel);
        }

        public void NextLevel()
        {
            if (CurrentLevel + 1 < LevelCatalog.Count) StartLevel(CurrentLevel + 1);
            else ShowSelect();
        }

        public void Pause()
        {
            if (State != StatePlaying || Level == null) return;
            State = StatePaused;
            Audio.StopEngine();
            if (Level.Car != null)
            {
                Level.Car.CanDrive = false;
                Level.Car.Throttle = 0f;
                Level.Car.Steer = 0f;
                Level.Car.Body.linearVelocity = Vector2.zero;
                Level.Car.Body.angularVelocity = 0f;
                Level.Car.Body.simulated = false;
            }

            var s = new PauseScreen();
            s.Setup(this, "pause");
            s.Build();
            _overlay = s;
        }

        public void Resume()
        {
            if (State != StatePaused) return;
            ClearOverlay();
            State = StatePlaying;
            if (Level != null && Level.Car != null)
            {
                Level.Car.Body.simulated = true;
                Level.Car.CanDrive = true;
            }
            if (Level != null && Level.Started) Audio.StartEngine();
        }

        public void LevelParked(LevelRuntime level)
        {
            if (State != StatePlaying) return;

            State = StateWon;
            Audio.Success();
            Audio.StopEngine();

            float time = level.Elapsed;
            int bumps = level.Bumps;
            bool newBest = Save.Best[CurrentLevel] < 0f || time < Save.Best[CurrentLevel];
            int stars = Save.Record(CurrentLevel, time, bumps);

            Fx.Burst(new Vector2(0f, 3.0f), 44);
            Fx.Burst(new Vector2(0f, 0.0f), 24);

            var s = new WinScreen
            {
                LevelIndex = CurrentLevel,
                Time = time,
                Bumps = bumps,
                Stars = stars,
                NewBest = newBest
            };
            s.Setup(this, "win");
            s.Build();
            _overlay = s;

            if (Level.Car != null) Level.Car.CanDrive = false;
        }

        // ------------------------------------------------------------------ utils
        void FadeTransition()
        {
            _fadeA = 0.9f;
            if (_fade != null) _fade.color = new Color(0f, 0f, 0f, _fadeA);
        }

        void ClearScreens()
        {
            if (_screen != null) { _screen.Destroy(); _screen = null; }
            ClearOverlay();
        }

        void ClearOverlay()
        {
            if (_overlay != null) { _overlay.Destroy(); _overlay = null; }
        }

        void ClearLevel()
        {
            if (Level != null)
            {
                Level.Dispose();
                Level = null;
            }
            Audio.StopEngine();
        }
    }
}
