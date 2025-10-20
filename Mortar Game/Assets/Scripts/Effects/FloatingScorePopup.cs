using UnityEngine;
using TMPro;

namespace MortarGame.Effects
{
    // World-space floating "+X" popup that rises, fades, and faces the camera.
    public class FloatingScorePopup : MonoBehaviour
    {
        [Header("Style")] public Color color = new Color(1f, 0.95f, 0.15f); // warm yellow
        public float lifetime = 1.3f;
        public float riseDistance = 1.5f;
        public float initialUpOffset = 0.6f;
        public float fontSize = 24.0f;
        [Tooltip("Use TMP overlay shader so the popup renders above geometry (useful if small or far).")]
        public bool useOverlayShader = true;

        private float _age;
        private float _riseSpeed;
        private TextMeshPro _tmp;
        private Color _baseColor;
        private float _baseScale = 0.35f;

        // Spawn helper
        public static FloatingScorePopup Spawn(Vector3 worldPos, int points, Color? overrideColor = null, float lifetime = 1.3f, float riseDistance = 1.5f)
        {
            var go = new GameObject("ScorePopup");
            var comp = go.AddComponent<FloatingScorePopup>();
            comp.color = overrideColor.HasValue ? overrideColor.Value : comp.color;
            comp.lifetime = lifetime;
            comp.riseDistance = riseDistance;
            comp.Setup(worldPos + Vector3.up * comp.initialUpOffset, $"+{points}");
            return comp;
        }

        public void Setup(Vector3 worldPos, string text)
        {
            transform.position = worldPos;
            _riseSpeed = riseDistance / Mathf.Max(0.01f, lifetime);

            _tmp = gameObject.AddComponent<TextMeshPro>();
            _tmp.text = text;
            _tmp.fontSize = fontSize;
            _tmp.alignment = TextAlignmentOptions.Center;
            _tmp.enableWordWrapping = false;
            _tmp.raycastTarget = false;
            _tmp.color = color;
            _baseColor = color;

            // Ensure a valid font asset
            if (TMP_Settings.defaultFontAsset != null)
            {
                _tmp.font = TMP_Settings.defaultFontAsset;
            }
            else
            {
                // Try a common built-in TMP font if default is not set
                var fallback = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (fallback) _tmp.font = fallback;
            }

            // Render above typical geometry if configured
            if (useOverlayShader && _tmp.fontMaterial != null)
            {
                var overlay = Shader.Find("TextMeshPro/Distance Field (Surface Overlay)");
                if (overlay) _tmp.fontMaterial.shader = overlay;
            }

            // Slight scale so it's readable in typical scene scales
            transform.localScale = Vector3.one * _baseScale;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            // Rise
            transform.position += Vector3.up * _riseSpeed * Time.deltaTime;

            // Face camera (billboard) and scale by distance for readability
            var cam = Camera.main;
            if (cam)
            {
                var dir = cam.transform.position - transform.position; // direction from text to camera
                if (dir.sqrMagnitude > 0.0001f)
                {
                    // Flip facing so the front of the text meshes faces the player/camera
                    transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
                }
                // Dynamic scale: roughly constant screen size across distances
                float d = dir.magnitude;
                float scale = Mathf.Clamp(_baseScale * (d * 0.03f), _baseScale * 0.8f, _baseScale * 3.0f);
                transform.localScale = Vector3.one * scale;
            }

            // Fade out over lifetime
            float t = Mathf.Clamp01(_age / Mathf.Max(0.01f, lifetime));
            var c = _baseColor;
            c.a = 1.0f - t;
            if (_tmp) _tmp.color = c;

            if (_age >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}