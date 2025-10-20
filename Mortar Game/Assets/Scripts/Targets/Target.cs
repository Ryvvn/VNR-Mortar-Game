using UnityEngine;
using MortarGame.Combat;
using System;
using MortarGame.Gameplay;
using MortarGame.Core;

namespace MortarGame.Targets
{
    public class Target : MonoBehaviour, IDamageable
    {
        [Header("Target HP")]
        public float maxHP = 100f;
        public float currentHP = 100f;
        public bool isPriorityTarget = true;

        public event Action OnDestroyed; // Event fired when this target is destroyed

        [Header("VFX/SFX")]
        [Tooltip("VFX to spawn when the target is destroyed")] public ParticleSystem destroyedVFX;
        [Tooltip("Sound to play when the target is destroyed")] public AudioClip destroyedSFX;
        [Tooltip("Auto destroy spawned VFX after this many seconds")] public float vfxAutoDestroySeconds = 3f;

        private void Awake()
        {
            currentHP = maxHP;
        }

        public void ApplyDamage(float damage)
        {
            currentHP -= damage;
            if (currentHP <= 0f)
            {
                currentHP = 0f;
                Destroyed();
            }
        }

        private void Destroyed()
        {
            // Award points for destroying this target
            GameManager.Instance.AddScoreAtWorldPosition(50, transform.position);

            // Spawn destroyed VFX/SFX
            if (destroyedVFX != null)
            {
                var vfx = Instantiate(destroyedVFX, transform.position, Quaternion.identity);
                vfx.Play();
                Destroy(vfx.gameObject, vfxAutoDestroySeconds);
            }
            if (destroyedSFX != null)
            {
                AudioSource.PlayClipAtPoint(destroyedSFX, transform.position, 1f);
            }

            Debug.Log($"Target destroyed: {name}");
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}