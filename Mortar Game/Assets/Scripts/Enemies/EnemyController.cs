using UnityEngine;
using MortarGame.Combat;
using MortarGame.Core;
using MortarGame.Gameplay;

namespace MortarGame.Enemies
{
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        public float baseSpeed = 1.7f;
        private float _speedMultiplier = 1f;
        private float _slowMultiplier = 1f;
        private float _slowUntil;

        [Header("HP")]
        public float maxHP = 50f;
        public float currentHP = 50f;

        private void Awake()
        {
            currentHP = maxHP;
        }

        private void Update()
        {
            float speed = baseSpeed * _speedMultiplier * _slowMultiplier;
            // Placeholder movement forward along local Z
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            if (_slowUntil > 0f && Time.time >= _slowUntil)
            {
                _slowMultiplier = 1f;
                _slowUntil = 0f;
            }
        }

        // Expose current ground speed and velocity for lead calculations
        public float GetCurrentSpeed() => baseSpeed * _speedMultiplier * _slowMultiplier;
        public Vector3 GetGroundVelocity() => transform.forward * GetCurrentSpeed();

        public void ApplyGlobalSpeedBuff(float percent, float duration)
        {
            // percent e.g. 0.1 (10%)
            StartCoroutine(SpeedBuffRoutine(percent, duration));
        }

        private System.Collections.IEnumerator SpeedBuffRoutine(float percent, float duration)
        {
            _speedMultiplier = 1f + percent;
            yield return new WaitForSeconds(duration);
            _speedMultiplier = 1f;
        }

        public void ApplySlow(float slowPercent, float duration)
        {
            // slowPercent e.g. 0.3 => -30% speed
            _slowMultiplier = Mathf.Clamp01(1f - slowPercent);
            _slowUntil = Time.time + duration;
        }

        public void ApplyDamage(float damage)
        {
            currentHP -= damage;
            if (currentHP <= 0f)
            {
                currentHP = 0f;
                Die();
            }
        }

        private void Die()
        {
            // Unregister from EnemyManager before destroying
            if (GameManager.Instance && GameManager.Instance.enemyManager)
            {

                GameManager.Instance.enemyManager.Unregister(this);
            }
            // TODO: VFX/SFX
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // Safety: ensure unregistration in case destroyed by other means
            if (GameManager.Instance && GameManager.Instance.enemyManager)
            {
                GameManager.Instance.enemyManager.Unregister(this);
            }
        }
    }
}