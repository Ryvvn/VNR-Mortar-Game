using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MortarGame.Core;
using MortarGame.Gameplay;
using System.Collections;

namespace MortarGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Top-left")] public TMP_Text compassHeadingText; public TMP_Text rangeText; public TMP_Text targetText; public TMP_Text elevationText; public TMP_Text deltaText; public TMP_Text solutionText; public TMP_Text leadText;
        [Header("Top-right")] public TMP_Text ammoText; public TMP_Text spotterText; public TMP_Text streakText; public TMP_Text scoreText;
        [Header("Center")] public TMP_Text impactText; public TMP_Text suggestionText;

        [Header("Visibility")]
        [Tooltip("Show the Spotter cooldown text (top-right)")]
        public bool showSpotterText = false;
        [Tooltip("Show the Lead text (top-left)")]
        public bool showLeadText = false;

        private Coroutine _clearScoreDeltaCo;
        private Coroutine _clearAchievementCo;

        private void Awake()
        {
            // Apply initial visibility
            if (spotterText) spotterText.gameObject.SetActive(showSpotterText);
            if (leadText) leadText.gameObject.SetActive(showLeadText);
        }

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged += UpdateScore;
                gm.OnScoreDelta += ShowScoreDelta;
                UpdateScore(gm.score);
            }
        }

        private void OnDisable()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged -= UpdateScore;
                gm.OnScoreDelta -= ShowScoreDelta;
            }
        }

        public void UpdateCompass(float bearingDegrees)
        {
            if (compassHeadingText) compassHeadingText.text = $"{bearingDegrees:000}°";
        }

        public void UpdateRange(float rangeM, float ballisticMaxM)
        {
            if (rangeText) rangeText.text = $"Range: {rangeM:0} m (max {ballisticMaxM:0} m)";
        }

        public void UpdateRangeWithPredicted(float dialM, float predictedM, float ballisticMaxM)
        {
            if (rangeText) rangeText.text = $"Range: {dialM:0} m (pred {predictedM:0} m; max {ballisticMaxM:0} m)";
        }

        public void UpdateAmmo()
        {
            var a = GameManager.Instance.ammoManager;
            if (ammoText) ammoText.text = $"HE:{a.heRounds} · Smoke:{a.smokeRounds} · HE+:{a.hePlusRounds}";
        }

        public void UpdateAmmoWithSelection(AmmoType selected)
        {
            var a = GameManager.Instance.ammoManager;
            if (ammoText)
            {
                string he = selected == AmmoType.HE ? $"[HE:{a.heRounds}]" : $"HE:{a.heRounds}";
                string smoke = selected == AmmoType.Smoke ? $"[Smoke:{a.smokeRounds}]" : $"Smoke:{a.smokeRounds}";
                string hep = selected == AmmoType.HEPlus ? $"[HE+:{a.hePlusRounds}]" : $"HE+:{a.hePlusRounds}";
                ammoText.text = $"{he} · {smoke} · {hep}";
            }
        }

        public void UpdateSpotterCooldown(float secondsRemaining)
        {
            if (!showSpotterText || !spotterText) return;
            if (secondsRemaining <= 0f) spotterText.text = "Spotter: READY";
            else spotterText.text = $"Spotter: {secondsRemaining:0}s";
        }

        public void UpdateStreak(int streak)
        {
            Debug.Log($"HUDController.UpdateStreak called with streak: {streak}");
            if (streakText) 
            {
                streakText.text = $"Streak: {streak}";
                Debug.Log($"HUDController: Set streakText to: '{streakText.text}'");
            }
            else
            {
                Debug.LogWarning("HUDController: streakText is null!");
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText) scoreText.text = $"Score: {score}";
        }

        public void ShowScoreDelta(int delta)
        {
            if (!scoreText) return;
            scoreText.text = $"Score: {GameManager.Instance.score} (+{delta})";
            if (_clearScoreDeltaCo != null) StopCoroutine(_clearScoreDeltaCo);
            _clearScoreDeltaCo = StartCoroutine(ClearScoreDeltaAfter(1.5f));
        }

        private IEnumerator ClearScoreDeltaAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (scoreText) scoreText.text = $"Score: {GameManager.Instance.score}";
        }

        public void ShowImpactFeedback(float missMeters)
        {
            if (!impactText) return;
            impactText.color = Color.white;
            if (Mathf.Abs(missMeters) < 0.5f) impactText.text = "HIT";
            else if (missMeters > 0f) impactText.text = $"OFF by +{missMeters:0.0} m";
            else impactText.text = $"OFF by {missMeters:0.0} m";
        }

        public void ShowHitTarget()
        {
            if (!impactText) return;
            impactText.color = Color.white; // WT-style: white for HIT
            impactText.text = "HIT TARGET";
        }

        public void ShowTargetDestroyed()
        {
            if (!impactText) return;
            impactText.color = new Color(1f, 0.2f, 0.2f); // WT-style: red for DESTROYED
            impactText.text = "TARGET DESTROYED";
        }

        public void ClearSuggestion()
        {
            if (suggestionText) suggestionText.text = "";
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

        public void ShowAchievement(string text, float seconds = 3f)
        {
            if (!suggestionText) return;
            suggestionText.text = text;
            if (_clearAchievementCo != null) StopCoroutine(_clearAchievementCo);
            _clearAchievementCo = StartCoroutine(ClearAchievementAfter(seconds));
        }

        private IEnumerator ClearAchievementAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (suggestionText) suggestionText.text = "";
        }

        public void UpdateTargetDistance(float distanceM, bool hasTarget, bool isSelected)
        {
            if (!targetText) return;
            if (hasTarget)
            {
                string sel = isSelected ? " · Selected" : "";
                targetText.text = $"Target: {distanceM:0} m{sel}";
            }
            else
            {
                targetText.text = "Target: --";
            }
        }

        public void UpdateElevation(float elevationDeg)
        {
            if (elevationText) elevationText.text = $"Elev: {elevationDeg:0}°";
        }

        public void UpdateRangeDelta(float deltaM)
        {
            if (deltaText) deltaText.text = $"Δ to target: {deltaM:+0;-0;0} m";
        }

        public void UpdateSolution(bool high)
        {
            if (solutionText) solutionText.text = high ? "Solution: HIGH" : "Solution: LOW";
        }

        public void UpdateLead(float leadM, bool active)
        {
            if (!showLeadText || !leadText) return;
            leadText.text = active ? $"Lead: {leadM:+0.0;-0.0;0.0} m" : "Lead: --";
        }
    }
}