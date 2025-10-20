using UnityEngine;
using TMPro;

namespace MortarGame.UI
{
    public class WaveHUD : MonoBehaviour
    {
        [Header("Text elements")]
        public TMP_Text waveText;
        public TMP_Text enemiesRemainingText;
        public TMP_Text nextWaveText;

        [Header("Behavior")]
        [Tooltip("Hide this entire HUD when waves are disabled or not present (Static scenario).")]
        public bool hideWhenNoWaves = true;

        private MortarGame.Waves.WaveManager _waveManager;
        private MortarGame.Enemies.EnemyManager _enemyManager;
        private bool _hidden;

        private void Awake()
        {
            // Best-effort auto-wiring by scene object names if not assigned
            if (!waveText) waveText = GameObject.Find("WaveText")?.GetComponent<TMP_Text>();
            if (!enemiesRemainingText) enemiesRemainingText = GameObject.Find("EnemiesRemainingText")?.GetComponent<TMP_Text>();
            if (!nextWaveText) nextWaveText = GameObject.Find("NextWaveText")?.GetComponent<TMP_Text>();
        }

        private void Start()
        {
            _waveManager = FindObjectOfType<MortarGame.Waves.WaveManager>();
            _enemyManager = MortarGame.Core.GameManager.Instance ? MortarGame.Core.GameManager.Instance.enemyManager : null;

            // If waves are not used, hide this HUD entirely
            var gm = MortarGame.Core.GameManager.Instance;
            bool wavesEnabledByScenario = gm && gm.Config != null && !string.IsNullOrEmpty(gm.Config.scenarioMode) && gm.Config.scenarioMode.Equals("Waves", System.StringComparison.OrdinalIgnoreCase);
            if (hideWhenNoWaves && (!wavesEnabledByScenario || _waveManager == null))
            {
                HideAll();
                _hidden = true;
                // Disable further updates
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (_hidden) return;
            // Wave number
            if (waveText)
            {
                int waveNum = _waveManager ? Mathf.Max(0, _waveManager.currentWave) : 0;
                waveText.text = $"Wave {waveNum}";
            }

            // Enemies remaining
            if (enemiesRemainingText)
            {
                int alive = _enemyManager != null ? _enemyManager.GetAliveCount() : 0;
                enemiesRemainingText.text = $"Enemies Remaining: {alive}";
            }

            // Next wave countdown
            if (nextWaveText)
            {
                float t = _waveManager ? _waveManager.nextWaveCountdown : 0f;
                nextWaveText.text = t > 0f ? $"Next Wave: {Mathf.CeilToInt(t)}s" : "Next Wave: —";
            }
        }

        private void HideAll()
        {
            if (waveText) waveText.gameObject.SetActive(false);
            if (enemiesRemainingText) enemiesRemainingText.gameObject.SetActive(false);
            if (nextWaveText) nextWaveText.gameObject.SetActive(false);
        }
    }
}