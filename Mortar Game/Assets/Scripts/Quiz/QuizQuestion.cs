using System;

namespace MortarGame.Quiz
{
    [Serializable]
    public class QuizQuestion
    {
        public string question;
        public string A;
        public string B;
        public string C;
        public string D;
        public char correct;
        public string tag;
    }
}