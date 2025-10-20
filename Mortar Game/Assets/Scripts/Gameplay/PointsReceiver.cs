using UnityEngine;
using MortarGame.Combat;
using System;

namespace MortarGame.Gameplay
{
    // Generic destructible for random spawned meshes/props
    // Implements IDamageable so projectiles can damage it
    public class PointsReceiver : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        public float maxHP = 30f;
        public float currentHP = 30f;

        [Header("Scoring")]
        public int pointsOnDestroyed = 10;
        public event Action OnDestroyed;

        [Header("Optional VFX/SFX")]
        public ParticleSystem destroyedVFX;
        public AudioClip destroyedSFX;
        public float vfxAutoDestroySeconds = 2.0f;

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
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}