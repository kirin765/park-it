using UnityEngine;

namespace Parking
{
    public class CarController : MonoBehaviour
    {
        public Rigidbody2D Body;
        public bool CanDrive = true;
        public float Throttle;
        public float Steer;
        public System.Action<float> OnBump;

        public const float MaxForward = 6.4f;
        public const float MaxReverse = 3.6f;
        const float Accel = 9f;
        const float BrakeRate = 17f;
        const float Coast = 4.6f;
        const float MaxTurn = 138f;

        float _bumpCooldown;

        public float ForwardSpeed => Body != null ? Vector2.Dot(Body.linearVelocity, transform.up) : 0f;
        public float Speed => Body != null ? Body.linearVelocity.magnitude : 0f;
        public float Speed01 => Mathf.Clamp01(Speed / MaxForward);
        public float HeadingDeg
        {
            get
            {
                Vector2 up = transform.up;
                return Mathf.Atan2(up.x, up.y) * Mathf.Rad2Deg;
            }
        }

        public void Init()
        {
            Body = gameObject.AddComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.mass = 1.2f;
            Body.linearDamping = 0f;
            Body.angularDamping = 2.5f;
            Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Body.constraints = RigidbodyConstraints2D.None;

            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.26f, 2.46f);
            box.offset = new Vector2(0f, -0.02f);
            box.sharedMaterial = PhysicsMat;
        }

        static PhysicsMaterial2D _mat;
        static PhysicsMaterial2D PhysicsMat
        {
            get
            {
                if (_mat == null)
                {
                    _mat = new PhysicsMaterial2D("car") { friction = 0.35f, bounciness = 0.02f };
                }
                return _mat;
            }
        }

        void FixedUpdate()
        {
            if (Body == null) return;
            float dt = Time.fixedDeltaTime;
            Vector2 fwd = transform.up;
            float v = Vector2.Dot(Body.linearVelocity, fwd);

            float throttle = CanDrive ? Throttle : 0f;
            float steer = CanDrive ? Steer : 0f;

            float desired;
            float rate;
            if (throttle > 0.05f)
            {
                if (v < -0.05f) { desired = 0f; rate = BrakeRate; }
                else { desired = MaxForward * throttle; rate = Accel; }
            }
            else if (throttle < -0.05f)
            {
                if (v > 0.05f) { desired = 0f; rate = BrakeRate; }
                else { desired = MaxReverse * throttle; rate = Accel * 0.9f; }
            }
            else
            {
                desired = 0f;
                rate = Coast;
            }

            float newV = Mathf.MoveTowards(v, desired, rate * dt);

            float speedAbs = Mathf.Abs(newV);
            float grip = Mathf.Clamp01(speedAbs / 1.0f);
            float taper = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(speedAbs / MaxForward));
            float turn = steer * MaxTurn * grip * taper;
            if (speedAbs < 0.02f) turn = 0f;

            Vector2 lateral = Body.linearVelocity - fwd * v;
            Body.linearVelocity = fwd * newV + lateral * 0.8f;

            Body.angularVelocity = Mathf.Lerp(Body.angularVelocity, -turn, 1f - Mathf.Exp(-dt * 22f));
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            if (Time.time < _bumpCooldown) return;
            float impact = c.relativeVelocity.magnitude;
            if (impact < 0.75f) return;
            _bumpCooldown = Time.time + 0.25f;
            OnBump?.Invoke(impact);
        }
    }
}
