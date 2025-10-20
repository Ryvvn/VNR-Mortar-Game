using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MortarGame.Core;
using MortarGame.Gameplay;
using MortarGame.Utility;
using MortarGame.UI;

namespace MortarGame.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public GameManager GM => GameManager.Instance;
        public string quizFileName = "quiz_mission3.csv";
        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private int _currentIndex = -1;

        private int _streak = 0;
        private int _score = 0;
        public HUDController hUDController;
        public int Streak => _streak;
        public int Score => _score;
        public QuizQuestion Current => (_currentIndex >= 0 && _currentIndex < _questions.Count) ? _questions[_currentIndex] : null;

        private void Awake()
        {
            if (_questions.Count == 0)
                LoadQuestions();
        }

        public void LoadQuestions()
        {
            _questions.Clear();
            var path = Path.Combine(Application.streamingAssetsPath, quizFileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"QuizManager: Quiz file not found at {path}");
                return;
            }
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                // Parse line and add to questions if valid
                var q = QuizCSVParser.ParseLine(line);
                if (q != null) _questions.Add(q);
            }
            _currentIndex = -1;
        }



        public QuizQuestion NextQuestion()
        {
            _currentIndex++;
            if (_currentIndex >= _questions.Count) _currentIndex = 0; // loop for now
            return Current;
        }

        public bool SubmitAnswer(char choice)
        {
            var correct = (char.ToUpperInvariant(choice) == char.ToUpperInvariant(Current.correct));
            if (correct)
            {
                // Reward ammo matching the attempted ammo type (default HE)
                var attempted = GameManager.Instance.lastAttemptedAmmoType;
                // switch (attempted)
                // {
                //     case AmmoType.Smoke:
                //         GameManager.Instance.ammoManager.AddSmoke(1);
                //         break;
                //     case AmmoType.HEPlus:
                //         GameManager.Instance.ammoManager.AddHEPlus(1);
                //         break;
                //     case AmmoType.HE:
                //     default:
                //         GameManager.Instance.ammoManager.AddHE(1);
                //         break;
                // }
                GameManager.Instance.ammoManager.AddHE(1);
                _streak++;
                _score += 10; // simple scoring: +10 per correct answer
                Debug.Log($"QuizManager: Correct answer! Streak is now: {_streak}");
                HandleStreakRewards();
            }
            else
            {
                Debug.Log($"QuizManager: Wrong answer! Streak reset from {_streak} to 0");
                _streak = 0;
                // Apply enemy speed buff
                var cfg = GameManager.Instance.Config.enemy;
                GameManager.Instance.enemyManager.ApplyGlobalSpeedBuff(cfg.speedBuffOnWrongAnswer, cfg.speedBuffDurationSec);
            }
            return correct;
        }

        private void HandleStreakRewards()
        {
            if (hUDController)
            {
                hUDController.UpdateStreak(_streak);
            }

            // Preserve any existing config-based streak rewards
            var cfg = GameManager.Instance.Config;
            if (cfg != null)
            {
                var rewards = cfg.streakRewards;
                if (rewards.smokeAt > 0 && _streak % rewards.smokeAt == 0)
                {
                    GameManager.Instance.ammoManager.AddSmoke(1);
                }
                if (rewards.hePlusAt > 0 && _streak % rewards.hePlusAt == 0)
                {
                    GameManager.Instance.ammoManager.AddHEPlus(1);
                }
            }

            // Hidden Easter Egg: after reaching a streak of 5,
            // each subsequent correct answer in streak 6-10 grants 1 smoke round.
            if (_streak >= 6 && _streak <= 10)
            {
                GameManager.Instance.ammoManager?.AddSmoke(1);
            }


        }
    }
}