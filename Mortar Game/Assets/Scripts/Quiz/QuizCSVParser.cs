using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MortarGame.Quiz;

namespace MortarGame.Utility
{
    /// <summary>
    /// CSV parser for quiz questions.
    /// Expected columns per line:
    /// question, A, B, C, D, correctLetter[, tag]
    /// - Fields may be quoted with double quotes
    /// - Double quotes inside quoted fields are escaped as "" (RFC4180 style)
    /// - Lines starting with # or // are treated as comments
    /// - A header line with first field 'question' is ignored
    /// </summary>
    public static class QuizCSVParser
    {
        public static QuizQuestion ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            string trimmed = line.Trim();
            if (trimmed.StartsWith("#") || trimmed.StartsWith("//")) return null;

            var fields = SplitCsv(trimmed);
            if (fields.Count == 0) return null;

            // Skip header row
            if (string.Equals(fields[0], "question", StringComparison.OrdinalIgnoreCase))
                return null;

            if (fields.Count < 6)
            {
                Debug.LogWarning($"QuizCSVParser: Skipping line with {fields.Count} fields (expected >= 6): '{line}'");
                return null;
            }

            var q = new QuizQuestion
            {
                question = fields[0],
                A = fields[1],
                B = fields[2],
                C = fields[3],
                D = fields[4]
            };

            // Correct letter (A/B/C/D). If missing/invalid, default to 'A'
            string correctField = fields[5]?.Trim();
            char correct = 'A';
            if (!string.IsNullOrEmpty(correctField))
            {
                correct = char.ToUpperInvariant(correctField[0]);
                if (correct < 'A' || correct > 'D') correct = 'A';
            }
            q.correct = correct;

            // Optional tag
            if (fields.Count >= 7)
                q.tag = fields[6];

            return q;
        }

        // RFC4180-style CSV splitting with quote handling and escaping
        private static List<string> SplitCsv(string input)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    if (inQuotes)
                    {
                        // Escaped quote inside quoted field: ""
                        if (i + 1 < input.Length && input[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++; // skip the next quote
                        }
                        else
                        {
                            inQuotes = false; // end of quoted field
                        }
                    }
                    else
                    {
                        inQuotes = true; // begin quoted field
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(TrimCsvField(sb.ToString()));
                    sb.Length = 0;
                }
                else
                {
                    sb.Append(c);
                }
            }

            // Add last field
            result.Add(TrimCsvField(sb.ToString()));
            return result;
        }

        private static string TrimCsvField(string field)
        {
            if (field == null) return string.Empty;
            string f = field.Trim();
            if (f.Length >= 2 && f[0] == '"' && f[f.Length - 1] == '"')
            {
                f = f.Substring(1, f.Length - 2);
            }
            return f;
        }
    }
}