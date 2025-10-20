using UnityEngine;
using MortarGame.Gameplay;
using MortarGame.Combat;
using System;

namespace MortarGame.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        public AmmoType ammoType = AmmoType.HE;
        public float flightTime = 1.5f; // used in kinematic arc mode
        public Vector3 startPos;
        public Vector3 targetPos;

        [Header("Physics Flight")]
        public bool usePhysicsFlight = true;
        [Tooltip("If true, launch toward targetPos; if false, launch along the projectile's forward (muzzle) direction")] public bool useTargetDirection = false;
        [Tooltip("Initial launch speed in m/s")] public float launchSpeed = 45f;
        [Tooltip("Maximum lifetime before auto-destroy if no collision occurs")] public float maxLifetime = 10f;
        [Tooltip("Adds a small SphereCollider if none exists to ensure collisions are detected")] public bool addDefaultColliderIfMissing = true;

        [Header("Tracer (Path Tail)")]
        [Tooltip("Enable a visible path tail behind the projectile (like a tracer)")] public bool enableTracer = true;
        [Tooltip("Lifetime of the trail in seconds")] public float tracerLifetime = 1.2f;
        [Tooltip("Trail width at the start")] public float tracerStartWidth = 0.03f;
        [Tooltip("Trail width at the end")] public float tracerEndWidth = 0.005f;
        [Tooltip("Trail start color (alpha controls opacity)")] public Color tracerStartColor = new Color(1f, 1f, 0.9f, 0.85f);
        [Tooltip("Trail end color (alpha should be near 0 for fade out)")] public Color tracerEndColor = new Color(1f, 0.8f, 0.1f, 0f);
        [Tooltip("If true, add/configure a TrailRenderer automatically if missing")] public bool addDefaultTracerIfMissing = true;
        [Tooltip("Minimum distance between trail points (smaller = smoother, more vertices)")] public float tracerMinVertexDistance = 0.05f;
        [Tooltip("Primary shader name to use for the trail (URP particles)")] public string tracerShaderNamePrimary = "Universal Render Pipeline/Particles/Unlit";
        [Tooltip("Fallback shader name if primary isn't found")] public string tracerShaderNameFallback = "Particles/Standard Unlit";
        [Tooltip("Secondary fallback shader if neither primary nor fallback found")] public string tracerShaderNameSecondaryFallback = "Legacy Shaders/Particles/Additive";

        [Header("Tracer Persistence on Impact")]
        [Tooltip("If true, the trail remains visible for a short time after impact")]
        public bool keepTracerAfterImpact = true;
        [Tooltip("How long the trail should remain after impact")]
        public float tracerPersistSeconds = 1.5f;

        [Header("Impact VFX/SFX")]
        [Tooltip("Explosion VFX used for HE shells")] public ParticleSystem explosionVFX_HE;
        [Tooltip("Bigger explosion VFX used for HE+ shells")] public ParticleSystem explosionVFX_HEPlus;
        [Tooltip("Smoke VFX used for Smoke shells")] public ParticleSystem smokeVFX;
        [Tooltip("Explosion sound for HE shells")] public AudioClip explosionSFX_HE;
        [Tooltip("Explosion sound for HE+ shells")] public AudioClip explosionSFX_HEPlus;
        [Tooltip("Pop/woosh sound for smoke shells")] public AudioClip smokeSFX;
        [Tooltip("Auto destroy spawned VFX after this many seconds")] public float vfxAutoDestroySeconds = 3f;

        private Rigidbody _rb;
        private TrailRenderer _trail;
        private float _spawnTime;
        private float _elapsed; // used for kinematic arc mode

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            if (addDefaultColliderIfMissing && GetComponent<Collider>() == null)
            {
                var sc = gameObject.AddComponent<SphereCollider>();
                sc.radius = 0.1f;
            }

            // Ensure a trail renderer exists/configured if tracer enabled
            if (enableTracer && addDefaultTracerIfMissing)
            {
                _trail = GetComponent<TrailRenderer>();
                if (_trail == null) _trail = gameObject.AddComponent<TrailRenderer>();

                _trail.time = tracerLifetime;
                _trail.minVertexDistance = tracerMinVertexDistance;
                _trail.emitting = true;
                _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _trail.receiveShadows = false;

                // Width over lifetime
                var widthCurve = new AnimationCurve();
                widthCurve.AddKey(0f, tracerStartWidth);
                widthCurve.AddKey(1f, tracerEndWidth);
                _trail.widthCurve = widthCurve;

                // Color gradient
                var gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(tracerStartColor, 0f),
                        new GradientColorKey(tracerEndColor, 1f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(tracerStartColor.a, 0f),
                        new GradientAlphaKey(tracerEndColor.a, 1f)
                    }
                );
                _trail.colorGradient = gradient;

                // Material
                if (_trail.material == null)
                {
                    var shader = Shader.Find(tracerShaderNamePrimary);
                    if (shader == null) shader = Shader.Find(tracerShaderNameFallback);
                    if (shader == null) shader = Shader.Find(tracerShaderNameSecondaryFallback);
                    if (shader == null) shader = Shader.Find("Sprites/Default");
                    if (shader == null) shader = Shader.Find("Unlit/Color");
                    if (shader == null) shader = Shader.Find("Standard");
                    if (shader != null)
                    {
                        _trail.material = new Material(shader);
                        if (_trail.material.HasProperty("_BaseColor"))
                            _trail.material.SetColor("_BaseColor", tracerStartColor);
                        else
                            _trail.material.color = tracerStartColor;
                    }
                }
            }
        }

        // Called by MortarController after instantiation
        public void Setup(AmmoType type, Vector3 origin, Vector3 impact, float flightTime)
        {
            ammoType = type;
            startPos = origin;
            targetPos = impact;
            this.flightTime = flightTime;
            transform.position = origin;
            _spawnTime = Time.time;

            // Reset tracer when re-used/spawned
            if (enableTracer && _trail != null)
            {
                _trail.Clear();
                _trail.emitting = true;
            }

            if (usePhysicsFlight)
            {
                LaunchPhysics();
            }
            else
            {
                // Reset kinematic arc timer
                _elapsed = 0f;
            }
        }

        // Expose current velocity for camera follow
        public Vector3 GetVelocity()
        {
            return _rb != null ? _rb.velocity : Vector3.zero;
        }

        private void LaunchPhysics()
        {
            if (_rb == null) return;
            Vector3 dir;
            if (useTargetDirection && (targetPos - startPos).sqrMagnitude > 0.0001f)
            {
                // Aim towards computed impact point; gravity will arc the shell
                dir = (targetPos - startPos).normalized;
            }
            else
            {
                // Launch along the muzzle/projectile's forward direction
                dir = transform.forward;
            }
            _rb.velocity = dir * launchSpeed;
        }

        private void Update()
        {
            if (usePhysicsFlight)
            {
                // Auto-destroy if it never collides
                if (Time.time - _spawnTime >= maxLifetime)
                {
                    Explode(transform.position);
                }
                return;
            }

            // Kinematic visual arc (legacy mode)
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, flightTime));

            var pos = Vector3.Lerp(startPos, targetPos, t);
            float arcHeight = Mathf.Sin(t * Mathf.PI) * 3f; // small arc
            pos.y += arcHeight;
            transform.position = pos;

            if (_elapsed >= flightTime)
            {
                Explode(targetPos);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            var point = (collision != null && collision.contacts != null && collision.contacts.Length > 0)
                ? collision.contacts[0].point
                : transform.position;
            Explode(point);
        }

        private void OnTriggerEnter(Collider other)
        {
            // If using trigger colliders, explode at current position
            Explode(transform.position);
        }



        public event Action<Vector3> OnExploded; // fired when the shell explodes; provides impact position

        private void Explode(Vector3 center)
        {
            // Notify listeners before destroying
            OnExploded?.Invoke(center);
            float radius;
            float damage;
            switch (ammoType)
            {
                case AmmoType.HEPlus:
                    radius = 5.5f; damage = 120f; break;
                case AmmoType.Smoke:
                    radius = 4f; damage = 0f; break;
                case AmmoType.HE:
                default:
                    radius = 4f; damage = 100f; break;
            }

            // Apply area effects
            var hits = Physics.OverlapSphere(center, radius);
            foreach (var h in hits)
            {
                var dmg = h.GetComponent<IDamageable>();
                if (dmg != null && damage > 0f)
                {
                    dmg.ApplyDamage(damage);
                }

                if (ammoType == AmmoType.Smoke)
                {
                    var enemy = h.GetComponent<MortarGame.Enemies.EnemyController>();
                    if (enemy != null)
                    {
                        enemy.ApplySlow(0.3f, 7f);
                    }
                }
            }

            // Detach and persist the trail briefly if requested
            if (enableTracer && keepTracerAfterImpact && _trail != null)
            {
                var trailGO = _trail.gameObject;
                trailGO.transform.SetParent(null, true);
                _trail.emitting = false;
                _trail.time = Mathf.Max(_trail.time, tracerPersistSeconds);
                Destroy(trailGO, Mathf.Max(0.05f, tracerPersistSeconds + 0.05f));
            }

            // Spawn impact VFX/SFX
            if (ammoType == AmmoType.HE)
            {
                if (explosionVFX_HE != null)
                {
                    var vfx = Instantiate(explosionVFX_HE, center, Quaternion.identity);
                    vfx.Play();
                    Destroy(vfx.gameObject, vfxAutoDestroySeconds);
                }
                if (explosionSFX_HE != null)
                {
                    AudioSource.PlayClipAtPoint(explosionSFX_HE, center, 1f);
                }
            }
            else if (ammoType == AmmoType.HEPlus)
            {
                if (explosionVFX_HEPlus != null)
                {
                    var vfx = Instantiate(explosionVFX_HEPlus, center, Quaternion.identity);
                    vfx.Play();
                    Destroy(vfx.gameObject, vfxAutoDestroySeconds);
                }
                if (explosionSFX_HEPlus != null)
                {
                    AudioSource.PlayClipAtPoint(explosionSFX_HEPlus, center, 1f);
                }
            }
            else if (ammoType == AmmoType.Smoke)
            {
                if (smokeVFX != null)
                {
                    var vfx = Instantiate(smokeVFX, center, Quaternion.identity);
                    vfx.Play();
                    Destroy(vfx.gameObject, vfxAutoDestroySeconds);
                }
                if (smokeSFX != null)
                {
                    AudioSource.PlayClipAtPoint(smokeSFX, center, 0.9f);
                }
            }

            Destroy(gameObject);
        }
    }
}