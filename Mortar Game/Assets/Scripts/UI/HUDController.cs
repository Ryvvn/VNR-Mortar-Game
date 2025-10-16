using UnityEngine;
using UnityEngine.UI;
using MortarGame.Core;
using MortarGame.Gameplay;

namespace MortarGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Top-left")] public Text compassHeadingText; public Text rangeText;
        [Header("Top-right")] public Text ammoText; public Text spotterText; public Text streakText;
        [Header("Center")] public Text impactText; public Text suggestionText;

        public void UpdateCompass(float bearingDegrees)
        {
            if (compassHeadingText) compassHeadingText.text = $"{bearingDegrees:000}°";
        }

        public void UpdateRange(float rangeM)
        {
            if (rangeText) rangeText.text = $"Range: {rangeM:0} m";
        }

        public void UpdateAmmo()
        {
            var a = GameManager.Instance.ammoManager;
            if (ammoText) ammoText.text = $"HE:{a.heRounds} · Smoke:{a.smokeRounds} · HE+:{a.hePlusRounds}";
        }

        public void UpdateSpotterCooldown(float secondsRemaining)
        {
            if (spotterText)
            {
                if (secondsRemaining <= 0f) spotterText.text = "Spotter: READY";
                else spotterText.text = $"Spotter: {secondsRemaining:0.0}s";
            }
        }

        public void UpdateStreak(int streak)
        {
            if (streakText) streakText.text = $"Streak: {streak}";
        }

        public void ShowImpactFeedback(float missMeters)
        {
            if (!impactText) return;
            if (Mathf.Abs(missMeters) < 0.5f) impactText.text = "HIT";
            else if (missMeters > 0f) impactText.text = $"OFF by +{missMeters:0.0} m";
            else impactText.text = $"OFF by {missMeters:0.0} m";
        }

        public void ShowQOLSuggestion(float missMeters)
        {
            if (!suggestionText) return;
            if (Mathf.Abs(missMeters) < 3f)
            {
                var suggestion = missMeters > 0 ? "+10 m" : "-10 m";
                suggestionText.text = $"Try {suggestion} next shot";
            }
            else
            {
                suggestionText.text = "";
            }
        }
    }
}