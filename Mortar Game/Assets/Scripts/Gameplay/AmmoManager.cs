using UnityEngine;

namespace MortarGame.Gameplay
{
    public enum AmmoType { HE, Smoke, HEPlus }

    public class AmmoManager : MonoBehaviour
    {
        [Header("Ammo Counts")]
        public int heRounds;
        public int smokeRounds;
        public int hePlusRounds;

        public void AddHE(int amount = 1) { heRounds += amount; }
        public void AddSmoke(int amount = 1) { smokeRounds += amount; }
        public void AddHEPlus(int amount = 1) { hePlusRounds += amount; }

        public bool TryConsume(AmmoType type)
        {
            switch (type)
            {
                case AmmoType.HE:
                    if (heRounds > 0) { heRounds--; return true; } return false;
                case AmmoType.Smoke:
                    if (smokeRounds > 0) { smokeRounds--; return true; } return false;
                case AmmoType.HEPlus:
                    if (hePlusRounds > 0) { hePlusRounds--; return true; } return false;
                default: return false;
            }
        }
    }
}