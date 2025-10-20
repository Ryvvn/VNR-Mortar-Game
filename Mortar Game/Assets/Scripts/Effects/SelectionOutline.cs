using System.Collections.Generic;
using UnityEngine;

namespace MortarGame.Effects
{
    // Simple selection outline effect that appends an outline material to MeshRenderers under the target.
    // Note: Current shader supports non-skinned MeshRenderers. Skinned meshes will be ignored to avoid visual glitches.
    // This is intended primarily for vehicles/props (e.g., tanks). Infantry may fall back to other indicators if needed.
    [DisallowMultipleComponent]
    public class SelectionOutline : MonoBehaviour
    {
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineThickness = 0.035f; // meters of normal extrusion
        [SerializeField] private Shader outlineShader; // Assign OutlineURP.shader in inspector to ensure availability

        private bool _active;
        private Material _outlineMat;
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private bool _initialized;

        private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");

        private void Awake()
        {
            CacheRenderers();
            EnsureMaterial();
        }

        private void OnEnable()
        {
            if (_active) Apply();
        }

        private void OnDisable()
        {
            Revert();
        }

        private void OnDestroy()
        {
            Revert();
        }

        public void SetColor(Color c)
        {
            outlineColor = c;
            if (_outlineMat != null)
            {
                _outlineMat.SetColor(OutlineColorID, outlineColor);
            }
        }

        public void SetThickness(float t)
        {
            outlineThickness = Mathf.Max(0f, t);
            if (_outlineMat != null)
            {
                _outlineMat.SetFloat(OutlineThicknessID, outlineThickness);
            }
        }

        public void SetShader(Shader s)
        {
            outlineShader = s;
            // Recreate material with the new shader on next apply
            if (_outlineMat != null)
            {
                // Destroy previous material instance to prevent leaks
                Destroy(_outlineMat);
                _outlineMat = null;
            }
        }

        public void Activate(bool on)
        {
            _active = on;
            EnsureMaterial();
            if (on) Apply(); else Revert();
        }

        private void CacheRenderers()
        {
            if (_initialized) return;
            _initialized = true;
            _renderers.Clear();
            // Only include standard MeshRenderers. Skip SkinnedMeshRenderer for now (requires skinning support in shader).
            var all = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
            foreach (var r in all)
            {
                // Skip if renderer is part of UI or decals
                _renderers.Add(r);
            }
        }

        private Shader GetOutlineShader()
        {
            if (outlineShader != null) return outlineShader;
            return Shader.Find("MortarGame/Outline");
        }

        private void EnsureMaterial()
        {
            if (_outlineMat != null) return;
            // Prefer explicitly assigned shader to guarantee availability in builds
            Shader shader = GetOutlineShader();
            if (shader == null)
            {
                Debug.LogWarning("SelectionOutline: Outline shader 'MortarGame/Outline' not found. Skipping outline to avoid whitening overlay.");
                return; // do not create a fallback material that overlays the whole mesh
            }
            _outlineMat = new Material(shader);
            _outlineMat.SetColor(OutlineColorID, outlineColor);
            _outlineMat.SetFloat(OutlineThicknessID, outlineThickness);
        }

        private void Apply()
        {
            EnsureMaterial();
            if (_outlineMat == null) return;
            CacheRenderers();
            Shader s = _outlineMat.shader;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                // Work with sharedMaterials to avoid Unity auto-instancing duplications
                var mats = new List<Material>(r.sharedMaterials);
                // Remove any previous outline materials (by shader match) to avoid duplicates
                mats.RemoveAll(m => m != null && m.shader == s);
                // Append our single outline material
                mats.Add(_outlineMat);
                r.sharedMaterials = mats.ToArray();
            }
        }

        private void Revert()
        {
            CacheRenderers();
            Shader s = _outlineMat != null ? _outlineMat.shader : GetOutlineShader();
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                var mats = new List<Material>(r.sharedMaterials);
                int before = mats.Count;
                mats.RemoveAll(m => m != null && m.shader == s);
                if (mats.Count != before)
                {
                    r.sharedMaterials = mats.ToArray();
                }
            }
        }
    }
}