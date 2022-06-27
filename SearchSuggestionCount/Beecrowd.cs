using System;
using System.Collections.Generic;

namespace Beecrowd
{
    class Beecrowd
    {
        public static void Main2()
        {
            var listsOfWords = ReadInput();

            foreach (var listOfWords in listsOfWords)
            {
                var result = GetAverageNumberOfKeysToFormWords(listOfWords);
                Console.WriteLine(result);
            }
        }

        public static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static float GetAverageNumberOfKeysToFormWords(IReadOnlyList<string> words)
        {
            var automatons = new List<SuffixAutomaton>();
            var reversedWords = new List<string>();

            foreach (var word in words)
            {
                var automaton = SuffixAutomatonFactory.GetInstance(Reverse(word));
                automatons.Add(automaton);
                reversedWords.Add(Reverse(word));
            }

            float sum = 0;

            foreach (var reversedWord in reversedWords)
            {
                var copyAutomatons = new List<SuffixAutomaton>(automatons);

                for (int letterIndex = 1; letterIndex <= reversedWord.Length; letterIndex++)
                {
                    var isAnyFalse = false;
                    var falseAutomatons = new List<SuffixAutomaton>();

                    foreach (var automaton in copyAutomatons)
                    {
                        if (!automaton.IsSuffix(reversedWord.Substring(reversedWord.Length - letterIndex)))
                        {
                            falseAutomatons.Add(automaton);
                            isAnyFalse = true;
                        }
                    }

                    foreach (var falseAutomaton in falseAutomatons)
                    {
                        copyAutomatons.Remove(falseAutomaton);
                    }

                    if (isAnyFalse || letterIndex == 1)
                    {
                        sum++;
                    }
                }
            }

            return sum / words.Count;
        }

        private static List<List<string>> ReadInput()
        {
            var listsOfWords = new List<List<string>>();
            var currentListOfWords = new List<string>();
            var currentWordCount = 0;

            while (true)
            {
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    return listsOfWords;
                }

                if (currentWordCount == 0)
                {
                    currentWordCount = int.Parse(line);
                    currentListOfWords = new List<string>();
                }
                else
                {
                    currentListOfWords.Add(line);
                    currentWordCount--;
                }

                if (currentWordCount == 0)
                {
                    listsOfWords.Add(currentListOfWords);
                }
            }
        }
    }

    static class SuffixAutomatonFactory
    {
        private static readonly Dictionary<string, SuffixAutomaton> automatons = new Dictionary<string, SuffixAutomaton>();

        public static SuffixAutomaton GetInstance(string word)
        {
            if (!automatons.ContainsKey(word))
            {
                automatons[word] = new SuffixAutomaton(word);
            }

            return automatons[word];
        }
    }

    class State
    {
        public int Length { get; set; }
        public int Link { get; set; }
        public bool IsEndState { get; set; }
        public Dictionary<char, int> Next = new Dictionary<char, int>();
    }

    class SuffixAutomaton
    {
        private int last = 0;

        private readonly List<State> states = new List<State> { new State() { Length = 0, Link = -1 } };

        public SuffixAutomaton(string word)
        {
            for (var i = 0; i < word.Length; i++)
            {
                Add(word[i], i);
            }

            SetEndStates();
        }

        public bool IsSuffix(string substring)
        {
            var position = 0;

            foreach (var letter in substring)
            {
                if (!states[position].Next.ContainsKey(letter))
                {
                    return false;
                }

                position = states[position].Next[letter];
            }

            return states[position].IsEndState;
        }

        private void Add(char letter, int index)
        {
            states.Add(new State() { Length = index + 1, Link = 0 });
            var r = states.Count - 1;
            var p = last;

            while (p >= 0 && !states[p].Next.ContainsKey(letter))
            {
                states[p].Next[letter] = r;
                p = states[p].Link;
            }
            if (p != -1)
            {
                var q = states[p].Next[letter];

                if (states[p].Length + 1 == states[q].Length)
                {
                    states[r].Link = q;
                }
                else
                {
                    var currentState = states[q];

                    states.Add(new State()
                    {
                        Next = new Dictionary<char, int>(currentState.Next),
                        Length = currentState.Length + 1,
                        Link = currentState.Link
                    });

                    int qq = states.Count - 1;

                    states[q].Link = qq;
                    states[r].Link = qq;

                    while (p >= 0 && states[p].Next[letter] == q)
                    {
                        states[p].Next[letter] = qq;
                        p = states[p].Link;
                    }
                }
            }
            last = r;
        }

        private void SetEndStates()
        {
            var p = last;

            while (p > 0)
            {
                states[p].IsEndState = true;
                p = states[p].Link;
            }
        }
    }
}