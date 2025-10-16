using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MortarGame.Utility;
using MortarGame.Core;
using MortarGame.Gameplay;

namespace MortarGame.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public string quizFileName = "quiz_mission3.csv";
        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private int _currentIndex = -1;
        private int _streak = 0;

        public int Streak => _streak;
        public QuizQuestion Current => (_currentIndex >= 0 && _currentIndex < _questions.Count) ? _questions[_currentIndex] : null;

        private void Awake()
        {
            LoadQuiz();
        }

        private void LoadQuiz()
        {
            var path = Path.Combine(Application.streamingAssetsPath, quizFileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"Quiz file not found: {path}");
                return;
            }
            var rows = SimpleCSV.Parse(path);
            if (rows.Count <= 1) return; // header + data
            for (int i = 1; i < rows.Count; i++)
            {
                var r = rows[i];
                if (r.Length < 7) continue;
                _questions.Add(new QuizQuestion
                {
                    question = r[0], A = r[1], B = r[2], C = r[3], D = r[4], correct = string.IsNullOrEmpty(r[5]) ? 'A' : r[5][0], tag = r[6]
                });
            }
            Debug.Log($"Loaded {_questions.Count} quiz questions.");
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
                GameManager.Instance.ammoManager.AddHE(1);
                _streak++;
                HandleStreakRewards();
            }
            else
            {
                _streak = 0;
                // Apply enemy speed buff
                var cfg = GameManager.Instance.Config.enemy;
                GameManager.Instance.enemyManager.ApplyGlobalSpeedBuff(cfg.speedBuffOnWrongAnswer, cfg.speedBuffDurationSec);
            }
            return correct;
        }

        private void HandleStreakRewards()
        {
            var rewards = GameManager.Instance.Config.streakRewards;
            if (_streak == rewards.smokeAt)
            {
                GameManager.Instance.ammoManager.AddSmoke(1);
            }
            if (_streak == rewards.hePlusAt)
            {
                GameManager.Instance.ammoManager.AddHEPlus(1);
            }
        }
    }
}