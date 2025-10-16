using UnityEngine;
using MortarGame.Combat;

namespace MortarGame.Targets
{
    public class Target : MonoBehaviour, IDamageable
    {
        [Header("Target HP")]
        public float maxHP = 100f;
        public float currentHP = 100f;
        public bool isPriorityTarget = true;

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
            // TODO: VFX/SFX here
            Debug.Log($"Target destroyed: {name}");
            Destroy(gameObject);
        }
    }
}