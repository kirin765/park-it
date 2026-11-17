using UnityEngine;

namespace Parking
{
    /// <summary>Keeps a 9:16 portrait viewport on any display, letterboxed on wide screens.</summary>
    public class ViewportFitter : MonoBehaviour
    {
        public Camera Cam;
        public const float DesignAspect = 9f / 16f;
        public const float HalfHeight = 10f;
        public const float HalfWidth = 5.625f;

        float _lastW, _lastH;

        void Awake()
        {
            if (Cam == null) Cam = GetComponent<Camera>();
            Apply();
        }

        void LateUpdate()
        {
            if (_lastW != Screen.width || _lastH != Screen.height) Apply();
        }

        void Apply()
        {
            _lastW = Screen.width;
            _lastH = Screen.height;
            if (Cam == null) return;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            float w = 1f, h = 1f;
            if (aspect > DesignAspect) w = DesignAspect / aspect;
            else h = aspect / DesignAspect;

            Cam.rect = new Rect((1f - w) * 0.5f, (1f - h) * 0.5f, w, h);
            Cam.orthographicSize = HalfHeight / Mathf.Max(0.0001f, h);
        }
    }
}
