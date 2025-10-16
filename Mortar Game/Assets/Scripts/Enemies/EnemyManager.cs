using System.Collections.Generic;
using UnityEngine;

namespace MortarGame.Enemies
{
    public class EnemyManager : MonoBehaviour
    {
        private readonly List<EnemyController> _enemies = new List<EnemyController>();

        public void Register(EnemyController enemy)
        {
            if (!_enemies.Contains(enemy)) _enemies.Add(enemy);
        }

        public void Unregister(EnemyController enemy)
        {
            _enemies.Remove(enemy);
        }

        public void ApplyGlobalSpeedBuff(float percent, float duration)
        {
            foreach (var e in _enemies)
            {
                if (e != null)
                    e.ApplyGlobalSpeedBuff(percent, duration);
            }
        }
    }
}