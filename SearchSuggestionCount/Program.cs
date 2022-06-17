using System;

class SearchSuggestionCount
{
    public static void Main()
    {
        // trocar por readline antes de submeter resposta
        // string input = Console.ReadLine();
        //var input = File.ReadLines("input.txt");

        //var listsOfWords = ParseInput(input);

        //var output = File.ReadLines("output.txt").ToArray();

        //for (int i = 0; i < output.Length; i++)
        //{
        //    var result = GetAverageNumberOfKeysToFormWords(listsOfWords[i]);
        //    Console.WriteLine($"Result: {result} | Expected: {output[i]}");
        //}

        var suffixAutomaton = new SuffixAutomaton("abcbc");

        Console.WriteLine(suffixAutomaton.IsSuffix("cbcb"));
        Console.WriteLine(suffixAutomaton.IsSuffix("ab"));
        Console.WriteLine(suffixAutomaton.IsSuffix("a"));
        // TODO: entender porque está dando true quando era para dar false
        Console.WriteLine(suffixAutomaton.IsSuffix("bbc"));

        //var suffixAutomaton2 = new SuffixAutomaton("heaven");
    }

    private static float GetAverageNumberOfKeysToFormWords(IEnumerable<string> words)
    {
        return 1;
    }

    private static IReadOnlyList<IReadOnlyList<string>> ParseInput(IEnumerable<string> input)
    {
        var listsOfWords = new List<IReadOnlyList<string>>();
        var currentListOfWords = new List<string>();
        var currentWordCount = 0;

        foreach (var line in input)
        {
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

        return listsOfWords;
    }
}

class State
{
    public int Lenght { get; set; }
    public int Link { get; set; }
    public Dictionary<char, int> Next = new();
}

class SuffixAutomaton
{
    private int last = 0;

    // TODO: trocar por uma lista
    private readonly List<State> states = new() { new State() { Lenght = 0, Link = -1 } };

    public SuffixAutomaton(string word)
    {
        for (var i = 0; i < word.Length; i++)
        {
            Add(word[i], i);
        }
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

        return true;
    }

    private void Add(char letter, int index)
    {
        states.Add(new State() { Lenght = index + 1, Link = 0 });
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

            if (states[p].Lenght + 1 == states[q].Lenght)
            {
                states[r].Link = q;
            }
            else
            {
                var currentState = states[q];

                states.Add(new State()
                {
                    Next = currentState.Next,
                    Lenght = currentState.Lenght + 1,
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
}
