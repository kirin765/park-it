using UnityEngine;

namespace Parking
{
    /// <summary>Builds one lot at runtime and runs its parking logic.</summary>
    public class LevelRuntime
    {
        public LevelDef Def;
        public Transform Root;
        public CarController Car;
        public Rigidbody2D Body;

        public float Elapsed;
        public int Bumps;
        public bool Complete;
        public bool Started;
        public float Hold;
        public float StartDelay = 1.0f;

        public const float HoldNeeded = 0.6f;

        GameApp _app;
        SpriteRenderer _carSr;
        SpriteRenderer _slotGlow;
        SpriteRenderer _pin;
        Vector3 _pinBase;
        PixelText _status;
        float _shake;
        float _time;
        bool _inside, _aligned;
        float _speed;

        static readonly Vector2[] Corners =
        {
            new Vector2(-0.60f, -1.20f), new Vector2(0.60f, -1.20f),
            new Vector2(-0.60f, 1.20f), new Vector2(0.60f, 1.20f)
        };

        public float Speed => _speed;
        public bool IsInside => _inside;
        public float Progress => Mathf.Clamp01(Hold / HoldNeeded);
        public Vector2 SlotForward => LevelDef.Heading(Def.SlotAng);
        public Vector2 SlotRight { get { var f = SlotForward; return new Vector2(f.y, -f.x); } }

        public LevelRuntime(GameApp app, LevelDef def)
        {
            _app = app;
            Def = def;
            Root = new GameObject("Lot_" + def.Name).transform;
            Root.SetParent(app.WorldRoot, false);
            Build();
        }

        public void Dispose()
        {
            if (Root != null) Object.Destroy(Root.gameObject);
        }

        // ------------------------------------------------------------------ build
        void Build()
        {
            var ground = Node("ground", Root, Vector3.zero, -100);
            ground.sprite = GroundPainter.Paint(Def, Def.Name.GetHashCode());
            ground.color = Color.white;

            var vig = Node("vignette", Root, new Vector3(0, 0, 0), 1800);
            vig.sprite = Art.VignetteSprite;
            vig.transform.localScale = new Vector3(GroundPainter.WorldW / 4f, GroundPainter.WorldH / 7.125f, 1f);
            vig.color = new Color(1f, 1f, 1f, 0.9f);

            foreach (var w in Def.Walls)
                Collider("wall", new Vector2(w.x, w.y), new Vector2(w.w, w.h));

            foreach (var r in Def.Planters)
                Collider("planter", new Vector2(r.x, r.y), new Vector2(r.w, r.h));

            foreach (var r in Def.Buildings)
                Collider("building", new Vector2(r.x, r.y), new Vector2(r.w, r.h));

            // target markings
            var slot = Node("slot", Root, new Vector3(Def.SlotCenter.x, Def.SlotCenter.y, 0f), 40);
            slot.sprite = Art.SlotOverlay(Def.SlotSize.x, Def.SlotSize.y);
            slot.transform.rotation = Quaternion.Euler(0f, 0f, -Def.SlotAng);
            slot.color = new Color(1f, 1f, 1f, 0.9f);
            _slotGlow = slot;

            // floating pin just outside the stall entrance
            Vector3 pinPos;
            if (Mathf.Abs(Mathf.Cos(Def.SlotAng * Mathf.Deg2Rad)) > 0.85f)
            {
                // parallel stall: marker stands in the road beside it
                Vector2 rt = new Vector2(Mathf.Cos(Def.SlotAng * Mathf.Deg2Rad), -Mathf.Sin(Def.SlotAng * Mathf.Deg2Rad));
                float side = Def.SlotCenter.x > 0f ? -1f : 1f;
                pinPos = Def.SlotCenter + rt * side * (Def.SlotSize.x * 0.5f + 1.0f) + new Vector2(0f, 0.6f);
            }
            else
            {
                Vector2 entry = -LevelDef.Heading(Def.SlotAng);
                pinPos = Def.SlotCenter + entry * (Def.SlotSize.y * 0.5f + 0.8f) + new Vector2(0f, 0.5f);
            }
            pinPos.x = Mathf.Clamp(pinPos.x, -4.9f, 4.9f);
            pinPos.y = Mathf.Clamp(pinPos.y, -8.4f, 7.9f);
            var pin = Node("pin", Root, new Vector3(pinPos.x, pinPos.y, 0f), 600);
            pin.sprite = Art.Pin;
            _pin = pin;
            _pinBase = pin.transform.localPosition;

            foreach (var c in Def.Cars)
            {
                var sr = Node("parked_car", Root, new Vector3(c.x, c.y, 0f), 0);
                sr.sprite = Art.Car(c.body, false);
                sr.transform.rotation = Quaternion.Euler(0f, 0f, -c.ang);
                sr.sortingOrder = 400 - Mathf.RoundToInt(c.y * 8f);
                var b = sr.gameObject.AddComponent<BoxCollider2D>();
                b.size = new Vector2(1.26f, 2.46f);
                b.offset = new Vector2(0f, -0.02f);
                b.sharedMaterial = CarPhysics;
            }

            foreach (var p in Def.Props)
                BuildProp(p);

            BuildPlayer();

            _status = PixelText.Create(Root, "", new Vector3(0, 0, 0), 2, Pal.White, 0.5f, 0f, 700, true, "status");
            _status.SetVisible(false);
        }

        static PhysicsMaterial2D _carMat;
        static PhysicsMaterial2D CarPhysics
        {
            get
            {
                if (_carMat == null) _carMat = new PhysicsMaterial2D("carstatic") { friction = 0.3f, bounciness = 0.02f };
                return _carMat;
            }
        }

        void BuildProp(PropDef p)
        {
            Sprite s;
            Vector2 size = Vector2.zero;
            switch (p.kind)
            {
                case PropKind.Cone: s = Art.Cone; size = new Vector2(0.3f, 0.3f); break;
                case PropKind.Tree: s = Art.Tree; break;
                case PropKind.Bush: s = Art.Bush; break;
                case PropKind.Pillar: s = Art.Pillar; size = new Vector2(0.72f, 0.72f); break;
                case PropKind.Hydrant: s = Art.Hydrant; size = new Vector2(0.26f, 0.26f); break;
                case PropKind.Bin: s = Art.Bin; size = new Vector2(0.34f, 0.34f); break;
                case PropKind.Sign: s = Art.Sign; size = new Vector2(0.18f, 0.18f); break;
                default: s = Art.Cone; size = new Vector2(0.3f, 0.3f); break;
            }

            var sr = Node(p.kind.ToString(), Root, new Vector3(p.x, p.y, 0f), 0);
            sr.sprite = s;
            sr.transform.localRotation = Quaternion.Euler(0f, 0f, -p.ang);
            sr.transform.localScale = Vector3.one * Mathf.Max(0.2f, p.scale);
            sr.sortingOrder = 380 - Mathf.RoundToInt(p.y * 8f);

            if (size != Vector2.zero)
            {
                var b = sr.gameObject.AddComponent<BoxCollider2D>();
                b.size = size;
            }
        }

        void BuildPlayer()
        {
            var go = new GameObject("PlayerCar");
            go.transform.SetParent(Root, false);
            go.transform.localPosition = new Vector3(Def.Start.x, Def.Start.y, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, -Def.StartAng);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Art.Car(LevelCatalog.PlayerBody, true);
            sr.sortingOrder = 420;
            _carSr = sr;

            Car = go.AddComponent<CarController>();
            Car.Init();
            Car.OnBump = OnBump;
            Body = Car.Body;
        }

        SpriteRenderer Node(string name, Transform parent, Vector3 pos, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        void Collider(string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root, false);
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            var b = go.AddComponent<BoxCollider2D>();
            b.size = size;
        }

        // ------------------------------------------------------------------ tick
        public void Tick(float dt)
        {
            _time += dt;

            if (!Started)
            {
                StartDelay -= dt;
                if (StartDelay <= 0f)
                {
                    Started = true;
                    _app.Audio.StartEngine();
                }
            }

            if (!Complete)
            {
                if (Started) Elapsed += dt;
                UpdateParking(dt);
            }

            // shake
            if (_shake > 0.001f)
            {
                _shake = Mathf.Lerp(_shake, 0f, 1f - Mathf.Exp(-dt * 10f));
                Root.localPosition = new Vector3(
                    Random.Range(-_shake, _shake) * 0.35f,
                    Random.Range(-_shake, _shake) * 0.35f, 0f);
            }
            else Root.localPosition = Vector3.zero;

            // pin bob
            if (_pin != null)
            {
                float bob = Mathf.Sin(_time * 3.2f) * 0.11f;
                _pin.transform.localPosition = _pinBase + new Vector3(0f, bob, 0f);
                float scale = 0.85f + (Complete ? 0.25f : 0f) + Progress * 0.12f;
                _pin.transform.localScale = Vector3.one * scale;
                _pin.color = Color.Lerp(new Color(1f, 1f, 1f, 0.92f), new Color(1f, 1f, 0.85f, 1f), Progress);
            }

            // slot pulse
            if (_slotGlow != null)
            {
                float pulse = 0.55f + Mathf.Sin(_time * (2.2f + Progress * 6f)) * 0.18f;
                float a = Mathf.Lerp(pulse, 1f, Progress);
                Color tint = Color.Lerp(new Color(0.85f, 1f, 0.9f, 1f), new Color(1f, 1f, 0.8f, 1f), Progress);
                if (Complete) tint = new Color(1f, 1f, 0.75f, 1f);
                _slotGlow.color = new Color(tint.r, tint.g, tint.b, a);
            }

            // car depth sorting
            if (_carSr != null)
                _carSr.sortingOrder = 402 - Mathf.RoundToInt(Car.transform.position.y * 8f);

            UpdateStatus();
        }

        void UpdateStatus()
        {
            if (_status == null) return;
            if (Complete)
            {
                _status.SetVisible(false);
                return;
            }

            float dist = Vector2.Distance(Car.transform.position, Def.SlotCenter);
            string msg = null;
            Color col = Pal.White;

            if (Hold > 0.05f)
            {
                msg = "HOLD IT";
                col = Pal.Mint;
            }
            else if (_inside && !_aligned)
            {
                msg = "STRAIGHTEN UP";
                col = Pal.Yellow;
            }
            else if (_inside && _aligned && _speed > 0.5f)
            {
                msg = "SLOW DOWN";
                col = Pal.Yellow;
            }
            else if (!_inside && dist < 3.4f)
            {
                msg = "LINE IT UP";
                col = new Color(1f, 1f, 1f, 0.8f);
            }

            if (msg == null || !Started)
            {
                _status.SetVisible(false);
                return;
            }

            _status.SetVisible(true);
            _status.SetText(msg);
            _status.SetColor(col);
            _status.transform.position = Car.transform.position + new Vector3(0f, 2.15f, 0f);
        }

        void UpdateParking(float dt)
        {
            _speed = Body.linearVelocity.magnitude;
            Vector2 slotF = SlotForward;
            Vector2 slotR = SlotRight;
            Vector2 carPos = Body.position;

            bool inside = true;
            for (int i = 0; i < 4; i++)
            {
                Vector2 local = Corners[i];
                Vector2 world = carPos + (Vector2)Car.transform.right * local.x + (Vector2)Car.transform.up * local.y;
                Vector2 rel = world - Def.SlotCenter;
                float u = Vector2.Dot(rel, slotR);
                float v = Vector2.Dot(rel, slotF);
                if (Mathf.Abs(u) > Def.SlotSize.x * 0.5f - 0.02f ||
                    Mathf.Abs(v) > Def.SlotSize.y * 0.5f - 0.02f)
                {
                    inside = false;
                    break;
                }
            }

            float angDiff = Mathf.Abs(Mathf.DeltaAngle(Car.HeadingDeg, Def.SlotAng));
            if (angDiff > 90f) angDiff = 180f - angDiff;
            bool aligned = angDiff < 10f;

            _inside = inside;
            _aligned = aligned;

            bool ok = inside && aligned && _speed < 0.5f && Started;
            if (ok)
            {
                Hold += dt;
                if (Hold >= HoldNeeded)
                {
                    Complete = true;
                    Hold = HoldNeeded;
                    Car.CanDrive = false;
                    Car.Throttle = 0f;
                    Car.Steer = 0f;
                    Car.Body.linearVelocity = Vector2.zero;
                    Car.Body.angularVelocity = 0f;
                    _app.LevelParked(this);
                }
            }
            else Hold = Mathf.Max(0f, Hold - dt * 3f);
        }

        void OnBump(float impact)
        {
            if (Complete) return;
            Bumps++;
            _shake = Mathf.Max(_shake, Mathf.Clamp01(impact / 6f) * 0.8f);
            _app.Audio.Bump(Mathf.Clamp01(impact / 7f));
        }

        public void SetInput(float throttle, float steer)
        {
            if (Car == null) return;
            Car.Throttle = Complete ? 0f : throttle;
            Car.Steer = Complete ? 0f : steer;
        }
    }
}
