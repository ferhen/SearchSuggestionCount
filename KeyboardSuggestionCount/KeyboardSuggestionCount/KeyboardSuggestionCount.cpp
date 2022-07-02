#include <iostream>
#include <string>
#include <map>
#include <vector>
#include <algorithm>

class State
{
public:
    int length = 0;
    int link = -1;
    bool isEndState = false;
    std::map<char, int> next = std::map<char, int>();

    State() = default;

    State(int _length, int _link, std::map<char, int> _next = std::map<char, int>()) {
        length = _length;
        link = _link;
        next = _next;
    }
};

class SuffixAutomaton {
private:
    int last = 0;
    std::vector<State> states = { State() };

    void add(char letter, int index) {
        states.push_back(State(index + 1, 0));
        int r = states.size() - 1;
        int p = last;

        while (p >= 0 && states[p].next.count(letter) == 0)
        {
            states[p].next[letter] = r;
            p = states[p].link;
        }
        if (p != -1)
        {
            int q = states[p].next[letter];

            if (states[p].length + 1 == states[q].length)
            {
                states[r].link = q;
            }
            else
            {
                State currentState = states[q];

                states.push_back(State(currentState.length + 1, currentState.link, currentState.next));

                int qq = states.size() - 1;

                states[q].link = qq;
                states[r].link = qq;

                while (p >= 0 && states[p].next[letter] == q)
                {
                    states[p].next[letter] = qq;
                    p = states[p].link;
                }
            }
        }
        last = r;
    }

    void setEndStates() {
        int p = last;

        while (p > 0)
        {
            states[p].isEndState = true;
            p = states[p].link;
        }
    }

public:
    SuffixAutomaton(std::string word) {
        for (int i = 0; i < word.length(); i++)
        {
            add(word[i], i);
        }

        setEndStates();
    }

    bool isSuffix(std::string substring) {
        int position = 0;

        for (const char& letter : substring) {
            if (states[position].next.count(letter) == 0) {
                return false;
            }

            position = states[position].next[letter];
        }

        return states[position].isEndState;
    }
};

std::string reverse(std::string s)
{
    std::string copy(s);
    std::reverse(copy.begin(), copy.end());
    return copy;
}

float getAverageNumberOfKeysToFormWords(std::vector<std::string> words)
{
    std::vector<SuffixAutomaton> automatons;
    std::vector<std::string> reversedWords;

    for (const std::string& word : words)
    {
        SuffixAutomaton automaton(reverse(word));
        automatons.push_back(automaton);
        reversedWords.push_back(reverse(word));
    }

    float sum = 0;

    for (const std::string& reversedWord : reversedWords)
    {
        std::vector<int> automatonsToIgnore;

        for (int letterIndex = 1; letterIndex <= reversedWord.size(); letterIndex++)
        {
            bool isAnyFalse = false;

            for (int i = 0; i < automatons.size(); i++)
            {
                if (std::find(automatonsToIgnore.begin(), automatonsToIgnore.end(), i) != automatonsToIgnore.end())
                {
                    continue;
                }

                if (!automatons[i].isSuffix(reversedWord.substr(reversedWord.size() - letterIndex)))
                {
                    automatonsToIgnore.push_back(i);
                    isAnyFalse = true;
                }
            }

            if (isAnyFalse || letterIndex == 1)
            {
                sum++;
            }
        }
    }

    return sum / words.size();
}

int main()
{
    std::vector<std::string> currentListOfWords;
    int currentWordCount = 0;

    while (true)
    {
        std::string line;
        std::getline(std::cin, line);

        if (line.empty()) {
            break;
        }

        if (currentWordCount == 0)
        {
            currentWordCount = std::stoi(line);
            std::vector<std::string> currentListOfWords;
        }
        else
        {
            currentListOfWords.push_back(line);
            currentWordCount--;
        }

        if (currentWordCount == 0)
        {
            std::cout << getAverageNumberOfKeysToFormWords(currentListOfWords);
        }
    }
}