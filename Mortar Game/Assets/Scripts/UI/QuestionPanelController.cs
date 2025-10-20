using UnityEngine;
using TMPro;
using UnityEngine.UI;

using MortarGame.Quiz; // Bind to this project's Quiz system
using MortarGame.Core;

namespace MortarGame.UI
{
    public class QuestionPanelController : MonoBehaviour
    {
        public static QuestionPanelController Instance { get; private set; }
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button answerA;
        [SerializeField] private Button answerB;
        [SerializeField] private Button answerC;
        [SerializeField] private Button answerD;
        [SerializeField] private Slider timerSlider;

        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI answerAText;
        [SerializeField] private TextMeshProUGUI answerBText;
        [SerializeField] private TextMeshProUGUI answerCText;
        [SerializeField] private TextMeshProUGUI answerDText;

        [Header("Timer Visuals")]
        [SerializeField] private Image timerFillImage; // Fill image of the slider
        [SerializeField] private Color timerNormalColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color timerWarningColor = new Color(1f, 0.65f, 0f);
        [SerializeField] private Color timerDangerColor = new Color(0.9f, 0.2f, 0.2f);
        [Range(0f, 1f)][SerializeField] private float warningThreshold = 0.3f;
        [Range(0f, 1f)][SerializeField] private float dangerThreshold = 0.15f;

        [Header("Answer Feedback")]
        [SerializeField] private Color correctFlashColor = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color incorrectFlashColor = new Color(0.9f, 0.25f, 0.25f);
        [SerializeField] private float flashDuration = 0.35f;

        private bool isActive = false;
        public bool IsOpen => gameObject.activeSelf; // expose panel open state

        // Cache base colors to restore after flashes
        private Color baseColorA = Color.white;
        private Color baseColorB = Color.white;
        private Color baseColorC = Color.white;
        private Color baseColorD = Color.white;

        // ===== Integration with MortarGame QuizManager =====
        [Header("Quiz Manager Binding")]
        [SerializeField] private QuizManager quizManager; // Auto-resolved in Awake if not set

        [Header("Question Timer (local)")]
        [Tooltip("Time limit per question (seconds). Used for local countdown UI.")]
        [SerializeField] private float questionTimeLimitSeconds = 12f;
        private float questionTimeRemaining = 0f;
        [Tooltip("Use local timer to drive the slider.")]
        [SerializeField] private bool useLocalTimer = true;
        // ===== End integration =====

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            // Prefer an explicitly named child for the question prompt
            if (questionText == null)
            {
                var qTransform = transform.Find("Question");
                if (qTransform != null)
                {
                    questionText = qTransform.GetComponent<TextMeshProUGUI>();
                    if (questionText == null)
                    {
                        // In case TMP is deeper
                        questionText = qTransform.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
                // Fallback: first TMP under panel (may pick a button label if layout differs)
                if (questionText == null)
                {
                    questionText = GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            if (answerA == null) answerA = transform.Find("Answer_A")?.GetComponent<Button>();
            if (answerB == null) answerB = transform.Find("Answer_B")?.GetComponent<Button>();
            if (answerC == null) answerC = transform.Find("Answer_C")?.GetComponent<Button>();
            if (answerD == null) answerD = transform.Find("Answer_D")?.GetComponent<Button>();

            // Get text components from buttons
            if (answerAText == null && answerA != null) answerAText = answerA.GetComponentInChildren<TextMeshProUGUI>();
            if (answerBText == null && answerB != null) answerBText = answerB.GetComponentInChildren<TextMeshProUGUI>();
            if (answerCText == null && answerC != null) answerCText = answerC.GetComponentInChildren<TextMeshProUGUI>();
            if (answerDText == null && answerD != null) answerDText = answerD.GetComponentInChildren<TextMeshProUGUI>();

            if (timerSlider == null)
                timerSlider = GetComponentInChildren<Slider>();

            // Try to resolve slider fill image
            if (timerFillImage == null && timerSlider != null && timerSlider.fillRect != null)
            {
                timerFillImage = timerSlider.fillRect.GetComponent<Image>();
            }

            // Cache base colors from target graphics
            if (answerA != null && answerA.targetGraphic != null) baseColorA = answerA.targetGraphic.color;
            if (answerB != null && answerB.targetGraphic != null) baseColorB = answerB.targetGraphic.color;
            if (answerC != null && answerC.targetGraphic != null) baseColorC = answerC.targetGraphic.color;
            if (answerD != null && answerD.targetGraphic != null) baseColorD = answerD.targetGraphic.color;

            // Resolve QuizManager if not assigned
            if (quizManager == null)
                quizManager = FindObjectOfType<QuizManager>();
        }

        void Start()
        {
            // Wire up answer button events
            if (answerA != null) answerA.onClick.AddListener(() => SubmitAnswer(0));
            if (answerB != null) answerB.onClick.AddListener(() => SubmitAnswer(1));
            if (answerC != null) answerC.onClick.AddListener(() => SubmitAnswer(2));
            if (answerD != null) answerD.onClick.AddListener(() => SubmitAnswer(3));

            // Validate bindings and warn if anything is missing
            ValidateReferences();

            // Start hidden
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (isActive && timerSlider != null)
            {
                float progress = 1f;

                if (useLocalTimer)
                {
                    questionTimeRemaining = Mathf.Max(0f, questionTimeRemaining - Time.deltaTime);
                    progress = (questionTimeLimitSeconds > 0f) ? (questionTimeRemaining / questionTimeLimitSeconds) : 0f;

                    if (questionTimeRemaining <= 0f)
                    {
                        // Time's up – show resolution as incorrect without submitting
                        int correctIdx = (quizManager != null && quizManager.Current != null)
                            ? Mathf.Clamp(char.ToUpperInvariant(quizManager.Current.correct) - 'A', 0, 3)
                            : 0;
                        ShowResolution(-1, false, correctIdx);
                    }
                }

                progress = Mathf.Clamp01(progress);
                timerSlider.value = progress;

                // Update timer color based on thresholds
                if (timerFillImage != null)
                {
                    if (progress <= dangerThreshold)
                        timerFillImage.color = timerDangerColor;
                    else if (progress <= warningThreshold)
                        timerFillImage.color = timerWarningColor;
                    else
                        timerFillImage.color = timerNormalColor;
                }
            }
        }

        public void ShowQuestion(QuizQuestion question)
        {
            // Ensure no pending hide from previous question interferes
            CancelInvoke(nameof(HidePanel));

            isActive = true;
            gameObject.SetActive(true);

            // Populate question text
            if (questionText != null)
                questionText.text = question != null ? question.question : "";

            // Populate answer options
            if (question != null)
            {
                if (answerAText != null) answerAText.text = "A) " + question.A;
                if (answerBText != null) answerBText.text = "B) " + question.B;
                if (answerCText != null) answerCText.text = "C) " + question.C;
                if (answerDText != null) answerDText.text = "D) " + question.D;
            }

            // Reset timer
            if (timerSlider != null)
            {
                timerSlider.maxValue = 1f;
                timerSlider.value = 1f;
            }
            if (timerFillImage != null)
                timerFillImage.color = timerNormalColor;

            // Initialize local timer
            questionTimeRemaining = questionTimeLimitSeconds;

            // Enable buttons and reset visuals/visibility
            SetButtonsInteractable(true);
            ResetAnswerVisibilityAndColors();
        }

        // Convenience: ask QuizManager for the next question and show it
        public void ShowNextQuestion()
        {
            if (quizManager == null)
            {
                Debug.LogWarning("QuestionPanelController: No QuizManager found.");
                return;
            }
            var q = quizManager.NextQuestion();
            ShowQuestion(q);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (answerA != null) answerA.interactable = interactable;
            if (answerB != null) answerB.interactable = interactable;
            if (answerC != null) answerC.interactable = interactable;
            if (answerD != null) answerD.interactable = interactable;
        }

        private void ResetAnswerVisibilityAndColors()
        {
            if (answerA != null)
            {
                answerA.gameObject.SetActive(true);
                if (answerA.targetGraphic != null) answerA.targetGraphic.color = baseColorA;
            }
            if (answerB != null)
            {
                answerB.gameObject.SetActive(true);
                if (answerB.targetGraphic != null) answerB.targetGraphic.color = baseColorB;
            }
            if (answerC != null)
            {
                answerC.gameObject.SetActive(true);
                if (answerC.targetGraphic != null) answerC.targetGraphic.color = baseColorC;
            }
            if (answerD != null)
            {
                answerD.gameObject.SetActive(true);
                if (answerD.targetGraphic != null) answerD.targetGraphic.color = baseColorD;
            }
        }

        private UnityEngine.UI.Button GetButtonByIndex(int idx)
        {
            switch (idx)
            {
                case 0: return answerA;
                case 1: return answerB;
                case 2: return answerC;
                case 3: return answerD;
                default: return null;
            }
        }

        private System.Collections.IEnumerator FlashButton(Button btn, Color flashColor, float duration)
        {
            if (btn == null || btn.targetGraphic == null) yield break;
            var g = btn.targetGraphic;
            Color original = g.color;
            g.color = flashColor;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime; // flash unaffected by time freeze
                yield return null;
            }
            // Smoothly revert back
            float back = 0.15f;
            t = 0f;
            while (t < back)
            {
                t += Time.unscaledDeltaTime;
                g.color = Color.Lerp(flashColor, original, t / back);
                yield return null;
            }
            g.color = original;
        }

        private void ValidateReferences()
        {
            if (questionText == null)
                Debug.LogWarning("QuestionPanelController: Missing reference for 'Question' text. Ensure a child named 'Question' with a TextMeshProUGUI is present.");
            if (answerA == null || answerAText == null)
                Debug.LogWarning("QuestionPanelController: Missing binding for Answer_A button or its TMP text child.");
            if (answerB == null || answerBText == null)
                Debug.LogWarning("QuestionPanelController: Missing binding for Answer_B button or its TMP text child.");
            if (answerC == null || answerCText == null)
                Debug.LogWarning("QuestionPanelController: Missing binding for Answer_C button or its TMP text child.");
            if (answerD == null || answerDText == null)
                Debug.LogWarning("QuestionPanelController: Missing binding for Answer_D button or its TMP text child.");
            if (timerSlider == null)
                Debug.LogWarning("QuestionPanelController: Timer Slider was not found. Timer UI will not update.");
            if (timerSlider != null && timerFillImage == null)
                Debug.LogWarning("QuestionPanelController: Timer fill Image not found. Urgency coloring will be disabled.");
        }

        // Removed legacy event bus + external overlays from previous project to avoid missing references
        // Advance/PowerPlay prompts were part of another game (BossFight2D). If needed, we can recreate them here.

        // Added methods for quiz flow and UI control
        public void ShowResolution(int selectedIndex, bool isCorrect, int correctIndex)
        {
            isActive = false;
            SetButtonsInteractable(false);

            // Flash the selected button to indicate correctness
            if (selectedIndex >= 0)
            {
                var selectedBtn = GetButtonByIndex(selectedIndex);
                if (selectedBtn != null)
                    StartCoroutine(FlashButton(selectedBtn, isCorrect ? correctFlashColor : incorrectFlashColor, flashDuration));
            }

            // Highlight the correct answer if the player was wrong
            if (!isCorrect)
            {
                var correctBtn = GetButtonByIndex(correctIndex);
                if (correctBtn != null)
                    StartCoroutine(FlashButton(correctBtn, correctFlashColor, flashDuration));
            }

            // Hide the panel after a delay
            Invoke(nameof(HidePanel), 2.5f);
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }

        private void SubmitAnswer(int choice)
        {
            if (!isActive) return;

            if (quizManager == null)
            {
                Debug.LogWarning("QuestionPanelController: SubmitAnswer called but QuizManager is not set/found.");
                return;
            }

            char letter = (char)('A' + Mathf.Clamp(choice, 0, 3));
            bool isCorrect = quizManager.SubmitAnswer(letter);
            int correctIndex = Mathf.Clamp(char.ToUpperInvariant(quizManager.Current.correct) - 'A', 0, 3);

            // Update HUD to reflect new streak, score, and ammo counts
            var hud = FindObjectOfType<HUDController>();
            if (hud != null)
            {
                Debug.Log($"QuestionPanelController: Updating HUD with streak: {quizManager.Streak}");
                hud.UpdateStreak(quizManager.Streak);
                hud.UpdateScore(quizManager.Score);
                hud.UpdateAmmoWithSelection(GameManager.Instance.lastAttemptedAmmoType);
            }

            // Stop timer and show resolution
            ShowResolution(choice, isCorrect, correctIndex);
        }
    }
}