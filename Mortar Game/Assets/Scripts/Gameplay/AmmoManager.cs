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

        [Header("Hidden Easter Egg (Smoke)")]
        [Tooltip("Counts the number of smoke shots the player has fired for the hidden easter egg.")]
        public int smokeShotsFiredEasterEggCount = 0;
        [Tooltip("Whether the 5-smoke bonus has already been awarded.")]
        public bool easterEggSmokeBonusAwarded = false;

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
                    if (smokeRounds > 0) { smokeRounds--; 
                        // Track smoke shots fired for the easter egg
                        smokeShotsFiredEasterEggCount++;
                        CheckEasterEggSmokeBonus();
                        return true; } 
                    return false;
                case AmmoType.HEPlus:
                    if (hePlusRounds > 0) { hePlusRounds--; return true; } return false;
                default: return false;
            }
        }

        private void CheckEasterEggSmokeBonus()
        {
            if (!easterEggSmokeBonusAwarded && smokeShotsFiredEasterEggCount >= 5)
            {
                easterEggSmokeBonusAwarded = true;
                // Award bonus points and show message
                var gm = MortarGame.Core.GameManager.Instance;
                if (gm)
                {
                    gm.AddScore(2000);
                    var hud = UnityEngine.Object.FindObjectOfType<MortarGame.UI.HUDController>();
                    if (hud != null)
                    {
                        hud.ShowAchievement("Good player always know to use smoke", 4f);
                    }
                }
            }
        }
    }
}