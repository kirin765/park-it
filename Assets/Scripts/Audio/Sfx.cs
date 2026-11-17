using UnityEngine;

namespace Parking
{
    /// <summary>All audio is synthesised at runtime, so the game ships with no audio assets.</summary>
    public class Sfx
    {
        const int Rate = 44100;

        readonly AudioSource _engine;
        readonly AudioSource _sfx;
        readonly AudioClip _click, _bump, _success, _star, _error, _beep;
        readonly AudioClip _engineClip;

        float _targetVol, _vol, _targetPitch = 1f, _pitch = 1f;
        public bool Muted;

        public Sfx(GameObject host)
        {
            _engine = host.AddComponent<AudioSource>();
            _engine.loop = true;
            _engine.playOnAwake = false;
            _engine.spatialBlend = 0f;
            _engine.volume = 0f;

            _sfx = host.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
            _sfx.volume = 1f;

            _click = Clip("click", 0.06f, t =>
            {
                float env = Mathf.Exp(-t * 55f);
                return Square(t, 1250f) * env * 0.18f;
            });
            _bump = Clip("bump", 0.28f, t =>
            {
                float env = Mathf.Exp(-t * 16f);
                float low = Mathf.Sin(2f * Mathf.PI * 72f * t) * Mathf.Exp(-t * 10f);
                float noise = (Random01(t * 44100f) * 2f - 1f) * Mathf.Exp(-t * 26f);
                return (low * 0.55f + noise * 0.45f) * env * 0.55f;
            });
            _success = Clip("success", 1.1f, t =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                int idx = Mathf.Min(3, Mathf.FloorToInt(t / 0.16f));
                float local = t - idx * 0.16f;
                float env = Mathf.Exp(-local * 7f) * Mathf.Exp(-t * 0.6f);
                float tone = (Square(t, notes[idx]) * 0.32f + Mathf.Sin(2f * Mathf.PI * notes[idx] * 2f * t) * 0.1f);
                if (t > 0.64f)
                {
                    env = Mathf.Exp(-(t - 0.64f) * 4.5f);
                    tone = 0f;
                    for (int i = 0; i < notes.Length; i++)
                        tone += Mathf.Sin(2f * Mathf.PI * notes[i] * t) * 0.10f;
                }
                return tone * env;
            });
            _star = Clip("star", 0.18f, t =>
            {
                float env = Mathf.Exp(-t * 22f);
                return (Mathf.Sin(2f * Mathf.PI * 1568f * t) * 0.22f + Mathf.Sin(2f * Mathf.PI * 2093f * t) * 0.1f) * env;
            });
            _error = Clip("error", 0.34f, t =>
            {
                float f = Mathf.Lerp(340f, 150f, Mathf.Clamp01(t / 0.3f));
                return Square(t, f) * Mathf.Exp(-t * 8f) * 0.2f;
            });
            _beep = Clip("beep", 0.14f, t => Square(t, 880f) * Mathf.Exp(-t * 12f) * 0.18f);

            _engineClip = EngineLoop();
            _engine.clip = _engineClip;
        }

        public void StartEngine()
        {
            if (_engine.clip == null) return;
            if (!_engine.isPlaying) _engine.Play();
        }

        public void StopEngine()
        {
            _targetVol = 0f;
            if (_engine.isPlaying) _engine.Stop();
        }

        public void Tick(float dt, float speed01, float throttle, bool driving)
        {
            _targetVol = Muted ? 0f : (driving ? 0.10f + Mathf.Abs(speed01) * 0.16f + Mathf.Abs(throttle) * 0.03f : 0f);
            _targetPitch = 0.8f + Mathf.Abs(speed01) * 0.85f + Mathf.Abs(throttle) * 0.05f;
            float k = 1f - Mathf.Exp(-dt * 8f);
            _vol = Mathf.Lerp(_vol, _targetVol, k);
            _pitch = Mathf.Lerp(_pitch, _targetPitch, k);
            if (_engine == null) return;
            _engine.volume = _vol;
            _engine.pitch = _pitch;
            if (driving && !_engine.isPlaying && !Muted) _engine.Play();
            if (!driving && _engine.isPlaying && _vol < 0.005f) _engine.Stop();
        }

        void One(AudioClip c, float vol = 1f, float pitch = 1f)
        {
            if (Muted || c == null || _sfx == null) return;
            _sfx.pitch = pitch;
            _sfx.PlayOneShot(c, vol);
        }

        public void Click() => One(_click, 0.8f);
        public void Bump(float strength) => One(_bump, Mathf.Clamp01(0.35f + strength * 0.5f), Random.Range(0.9f, 1.1f));
        public void Success() => One(_success, 0.9f);
        public void Star() => One(_star, 0.8f, Random.Range(0.98f, 1.04f));
        public void Error() => One(_error, 0.8f);
        public void Beep() => One(_beep, 0.7f);

        // ------------------------------------------------------------------ synthesis
        static AudioClip Clip(string name, float dur, System.Func<float, float> gen)
        {
            int n = Mathf.CeilToInt(dur * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
                data[i] = Mathf.Clamp(gen(i / (float)Rate), -1f, 1f);
            // tiny fade-out to avoid clicks
            int fade = Mathf.Min(n, 220);
            for (int i = 0; i < fade; i++) data[n - 1 - i] *= i / (float)fade;
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Square(float t, float f)
        {
            float p = t * f;
            p -= Mathf.Floor(p);
            return p < 0.5f ? 1f : -1f;
        }

        static float Random01(float x)
        {
            float v = Mathf.Sin(x * 12.9898f) * 43758.5453f;
            return v - Mathf.Floor(v);
        }

        static AudioClip EngineLoop()
        {
            int n = Rate;
            var data = new float[n];
            float phase = 0f;
            float lp = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                phase += 55f / Rate;
                if (phase > 1f) phase -= 1f;
                float saw = phase * 2f - 1f;
                float rumble = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.6f +
                               Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.25f;
                float noise = (Random01(i * 1.7f) * 2f - 1f) * 0.12f;
                float s = saw * 0.18f + rumble * 0.3f + noise;
                lp += (s - lp) * 0.08f;
                data[i] = Mathf.Clamp(lp * 0.9f, -1f, 1f);
            }
            var clip = AudioClip.Create("engine", n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
