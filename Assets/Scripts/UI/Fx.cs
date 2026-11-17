using UnityEngine;

namespace Parking
{
    public class Confetti : MonoBehaviour
    {
        Vector2 _vel;
        float _life, _maxLife, _spin;
        SpriteRenderer _sr;
        Color _color;
        float _scale;

        public void Init(Vector2 vel, Color color, float scale, float life)
        {
            _vel = vel;
            _color = color;
            _scale = scale;
            _life = _maxLife = life;
            _spin = Random.Range(-360f, 360f);
            _sr = GetComponent<SpriteRenderer>();
            _sr.color = color;
            transform.localScale = Vector3.one * scale;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _life -= dt;
            _vel.y -= 7.5f * dt;
            _vel *= 1f - dt * 1.4f;
            transform.localPosition += (Vector3)(_vel * dt);
            transform.localRotation = Quaternion.Euler(0f, 0f, transform.localEulerAngles.z + _spin * dt);
            if (_life < 0.55f && _sr != null)
            {
                var c = _color;
                c.a = Mathf.Clamp01(_life / 0.55f);
                _sr.color = c;
            }
            if (_life <= 0f) Destroy(gameObject);
        }
    }

    public static class Fx
    {
        static readonly Color[] Colors =
        {
            Pal.Yellow, Pal.Mint, Pal.Blue, Pal.Red, Pal.White, Pal.Orange, Pal.Purple
        };

        public static void Burst(Vector2 center, int count, float spread = 6f)
        {
            if (GameApp.I == null) return;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("confetti");
                go.transform.SetParent(GameApp.I.FxRoot, false);
                go.transform.localPosition = new Vector3(center.x + Random.Range(-1.2f, 1.2f), center.y + Random.Range(-0.6f, 0.6f), 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Art.ConfettiSprite;
                sr.sortingOrder = 3500;
                var c = Colors[Random.Range(0, Colors.Length)];
                var comp = go.AddComponent<Confetti>();
                Vector2 vel = new Vector2(Random.Range(-spread, spread), Random.Range(3.5f, 9f));
                comp.Init(vel, c, Random.Range(1.6f, 2.8f), Random.Range(1.3f, 2.1f));
            }
        }
    }
}
