using System;
using System.Collections.Generic;

namespace Beecrowd
{
    class Beecrowd
    {
        public static void Main()
        {
            // Entrada da solução é lida de stdin e transformada numa lista de listas de string.
            var listsOfWords = ReadInput();

            // Para cada uma da lista de strings, é executado o algoritmo.
            foreach (var listOfWords in listsOfWords)
            {
                var result = GetAverageNumberOfKeysToFormWords(listOfWords);
                Console.WriteLine(result);
            }
        }

        // Método que realiza a inversão de uma string.
        public static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        // Implementação do algoritmo que calcula a média de teclas a serem pressionadas para o autocomplete de todas as palavras.
        private static float GetAverageNumberOfKeysToFormWords(IReadOnlyList<string> words)
        {
            // Instanciação de uma lista para armazenar os autômatos de sufixo.
            var automatons = new List<SuffixAutomaton>();
            // Instanciação de uma lista para armazenar as palavras invertidas.
            var reversedWords = new List<string>();

            foreach (var word in words)
            {
                // Requisição de um novo autômato de sufixo com base na palavra de input invertida.
                var automaton = SuffixAutomatonFactory.GetInstance(Reverse(word));
                // Adiciona o autômato na lista.
                automatons.Add(automaton);
                // Adiciona a palavra invertida na lista.
                reversedWords.Add(Reverse(word));
            }

            // Instanciação de uma variável para armazenar a soma de todas as teclas para todas as palavras.
            float sum = 0;

            foreach (var reversedWord in reversedWords)
            {
                // É feita uma cópia da lista de autômatos a cada palavra a ser analisada,
                // com o objetivo de remover os autômatos que já não reconhecem a palavra sem que a lista de autômatos original seja alterada.
                var copyAutomatons = new List<SuffixAutomaton>(automatons);

                for (int letterIndex = 1; letterIndex <= reversedWord.Length; letterIndex++)
                {
                    // Variável de controle que identifica se algum dos autômatos não reconhece a substring.
                    var isAnyFalse = false;
                    // Lista com os autômatos que não reconheceram as substrings.
                    var falseAutomatons = new List<SuffixAutomaton>();

                    foreach (var automaton in copyAutomatons)
                    {
                        // Condição que verifica se a substring do final da palavra invertida é um sufixo do autômato,
                        // ou seja, verifica se o prefixo da palavra até o índice letterIndex pertence a palvavra reversedWord.
                        if (!automaton.IsSuffix(reversedWord.Substring(reversedWord.Length - letterIndex)))
                        {
                            // Caso a substring não seja um prefixo válido, o autômato é adicionado na lista e a variável de controle é setada como verdadeira.
                            falseAutomatons.Add(automaton);
                            isAnyFalse = true;
                        }
                    }

                    // Ao fim da verificação de prefixos de todas as substrings até o índice letterIndex,
                    // os autômatos que não reconheceram pelo menos uma delas são removidos para a próxima iteração.
                    foreach (var falseAutomaton in falseAutomatons)
                    {
                        copyAutomatons.Remove(falseAutomaton);
                    }

                    // Se alguma das substrings não foi reconhecida por todos os autômatos,
                    // ou se for o primeiro laço da iteração (a primeira tecla sempre deve ser pressionada independente se todas as primeiras letras de todas as palavras forem iguais),
                    // a soma dos dígitos é incrementada em um.
                    if (isAnyFalse || letterIndex == 1)
                    {
                        sum++;
                    }
                }
            }

            // Ao final, a média é calculada como a soma dividida pela quantidade de palavras.
            return sum / words.Count;
        }

        // Método que realiza a transformação da entrada de stdin para uma lista de listas de string.
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

    // Classe que realiza a instanciação de novas classes de autômatos de sufixo.
    // Foi criada como o objetivo de diminuir a complexidade temporal da solução usando um mecanismo de memoization.
    // Se o autômato da palavra word não existe, ele é criado e armazenado em um dicionário;
    // caso ele já exista, então não é necessária uma nova instanciação, ele é simplesmente recuperado do dicionário.
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

    // Classe que representa os estados de um autômato de sufixo.
    class State
    {
        // Tamanho da maior string das classes de equivalência do estado.
        public int Length { get; set; }

        // Referência para o suffix link anterior (estado sem a primeira letra do estado atual, ex.: hi -> i).
        public int Link { get; set; }

        // Define se o estado é um estado final, ou seja, se uma entrada tiver este estada como seu último, então é um sufixo.
        public bool IsEndState { get; set; }

        // Dicionário com todas as transições que saem do estado atual.
        public Dictionary<char, int> Next = new Dictionary<char, int>();
    }


    // Classe que define o sufixo de autômato.
    class SuffixAutomaton
    {
        // Índice da classe de equivalência da palavra completa.
        private int last = 0;

        // Lista de todos os estados do autômato, com o estado inicial já inicializado (Link = -1 indica que não existe estado anterior, ou seja, é o estado inicial).
        private readonly List<State> states = new List<State> { new State() { Length = 0, Link = -1 } };

        // Construtor do sufixo
        public SuffixAutomaton(string word)
        {
            // Itera pera palavra adicionando cada letra à lógica que gera ou atualizada os estados.
            for (var i = 0; i < word.Length; i++)
            {
                Add(word[i], i);
            }

            // Ao final da geração do estados, é feita a identificação dos estados finais.
            SetEndStates();
        }

        // Método que identifica se uma substring é um sufixo reconhecido pelo autômato.
        public bool IsSuffix(string substring)
        {
            // Inicía-se na posição inicial do autômato
            var position = 0;

            foreach (var letter in substring)
            {
                // Para cada letra da substring, verifica se no estado atual existe uma transição para um próximo estado.
                if (!states[position].Next.ContainsKey(letter))
                {
                    // Caso não exista uma transição, a palavra não é reconhecida.
                    return false;
                }

                // Caso sim, move-se para a posição encontrada.
                position = states[position].Next[letter];
            }

            // Ao final das iterações por todas as letras da substring,
            // se o estrado em que foi finalizado é um estado inicial, a substring é sufixo, caso não seja, não é sufixo.
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

        // Método que percorre o autômato construído e identifica os estados finais
        private void SetEndStates()
        {
            // Iniciando a partir do estado final
            var p = last;

            // Enquanto o estado atual pertence aos estados válidos do autômato
            while (p > 0)
            {
                // Marca como estado atual
                states[p].IsEndState = true;
                // Move para o suffix link anterior
                p = states[p].Link;
            }
        }
    }
}