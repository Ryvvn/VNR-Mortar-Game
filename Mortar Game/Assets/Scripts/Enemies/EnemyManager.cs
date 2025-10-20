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

        public int GetAliveCount()
        {
            _enemies.RemoveAll(e => e == null);
            return _enemies.Count;
        }

        public void ApplyGlobalSpeedBuff(float percent, float duration)
        {
            // Clean up null entries before applying
            _enemies.RemoveAll(e => e == null);
            foreach (var e in _enemies)
            {
                e.ApplyGlobalSpeedBuff(percent, duration);
            }
        }
    }
}