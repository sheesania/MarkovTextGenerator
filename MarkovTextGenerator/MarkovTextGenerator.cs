/**
 * Given a text file, this program analyzes the pattern of text in the
 * file, creating Markov chains of letters/words that are statistically
 * likely to appear after each letter/word. Then it can generate text
 * similar to the input using the Markov chains.
 * 
 * Copyright March 2017 (c) Alison Bell
 *
 * This work ‘as-is’ I provide.
 * No warranty express or implied.
 *  For no purpose fit,
 *  not even a wee bit.
 * Liability for damages denied.
 *
 * Permission is granted hereby,
 * to copy, share, and modify.
 *  Use it with glee,
 *  for profit or free.
 * On this notice, these rights rely.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// command line parsing
using CommandLine;

namespace MarkovTextGenerator
{
    class MarkovTextGenerator
    {
        // command line option system
        class Options
        {
            [Option('i', "input", Required = true, HelpText = "Path to file with input text")]
            public string InputFile { get; set; }

            [Option('o', "word-count", Required = true, HelpText = "Number of words to generate")]
            public int TargetWordCount { get; set; }

            [Option('g', "group-size", Required = false, DefaultValue = 1, HelpText = "Number of characters/words to group together in analysis")]
            public int GroupSize { get; set; }

            [Option('w', "use-words", Required = false, HelpText = "Calculate probabilities based on words instead of groups of characters")]
            public bool UseWords { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                var usage = new StringBuilder();
                usage.AppendLine("Markov Text Generator 1.2");
                usage.AppendLine("Description: Generates text similar to input text using Markov probability chains.");
                usage.AppendLine("Usage: mtg.exe -i input_path -o num_words_to_generate [-g group_size] [-w]");
                usage.AppendLine("Examples:");
                usage.AppendLine("\tmtg.exe -i NalhallanText.txt -o 16");
                usage.AppendLine("\tmtg.exe -i NalhallanText.txt -o 16 -g 2");
                usage.AppendLine("\tmtg.exe -i NalhallanText.txt -o 16 -w");
                usage.AppendLine("\tmtg.exe -i NalhallanText.txt -o 16 -g 2 -w");
                usage.AppendLine();
                usage.AppendLine("OPTIONS");
                usage.AppendLine("Input path\n\tPath to a plain text file with at least one space.");
                usage.AppendLine();
                usage.AppendLine("Num words to generate\n\tNumber of words to generate from the input text.");
                usage.AppendLine();
                usage.AppendLine("Group size\n\tNumber of characters or words to group together in Markov chains. Defaults to 1. " +
                    "The larger the group size, the closer the generated text will be to the input text. " +
                    "In general, the larger your input text, the larger of a group size MTG can handle. " +
                    "2 or 3 is " +
                    "usually a good size in my experience. If your chosen group size is too large for your input size, you may cause an " +
                    "endless loop, making " +
                    "MTG hang. If MTG consistently hangs with a certain letter group size and input, then either make the input longer " +
                    "or decrease the letter group size.");
                usage.AppendLine();
                usage.AppendLine("-w\n\tCalculate probabilities based on words (delimited by spaces) instead of by chunks of characters.");
                usage.AppendLine();
                usage.AppendLine("If you encounter the 'Key not found' error, either make the input longer or increase the group size " +
                    "if using that option.");
                return usage.ToString();
            }
        }

        private string inputPath; //input text file
        private bool useWords; // will we analyze words or groups of chars?
        private int targetWordCount; //number of words to generate after analysis
        private int groupSize; //how many letters/words should be grouped together in the Markov chains?

        static void Main(string[] args)
        {
            // get command line options and start the program
            var options = new Options();
            var parser = new Parser();
            if (parser.ParseArguments(args, options))
            {
                new MarkovTextGenerator(options.InputFile, options.GroupSize, options.TargetWordCount, options.UseWords).Run();
            }
            else
            {
                Console.WriteLine(options.GetUsage());
            }
        }

        public MarkovTextGenerator(String inputPath, int groupSize, int targetWordCount, bool useWords)
        {
            this.inputPath = inputPath;
            this.groupSize = groupSize;
            this.useWords = useWords;
            this.targetWordCount = targetWordCount;
        }

        public void Run()
        {
            string inputText = ReadInput();
            Dictionary<string, List<string>> frequencies = GetFrequencies(inputText);
            string generatedWords = GenerateWords(frequencies);
            generatedWords = FormatOutput(generatedWords);
            WriteOutput(generatedWords);
        }

        // Reads all the text in the input file. Exits if there are any errors.
        private string ReadInput()
        {
            string input = "";
            try
            {
                input = System.IO.File.ReadAllText(inputPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading input file: " + e.Message);
                Console.WriteLine("Exiting");
                Environment.Exit(1);
            }
            return input;
        }

        // For each letter/word group in the input string, calculates what letters/words come 
        // after it and how often.
        private Dictionary<string, List<string>> GetFrequencies(string inputText)
        {
            // the key is the letter/word group we're interested in; the value is a List of letters/words
            // that come after it. Letters that come after it more often will appear more often in the 
            // List.
            var frequencies = new Dictionary<string, List<string>>();

            // use groups of words as keys
            if (useWords)
            {
                var words = inputText.Split(new Char[] {' '});
                for (int counter = 0; counter < words.Length - groupSize; counter++)
                {
                    string currentWordGroup = string.Join(" ", words.Skip(counter).Take(groupSize));
                    if (!(frequencies.ContainsKey(currentWordGroup)))
                    {
                        frequencies.Add(currentWordGroup, new List<string>());
                    }
                    frequencies[currentWordGroup].Add(words.Skip(counter + groupSize).Take(1).First());
                }
            }

            // use groups of characters as keys
            else
            {
                // Explanation of the weird middle term of the for loop: We will be looking
                // at chunks of letters, each with [groupSize] characters. inputText[counter]
                // will always be the *first* character of a group. So we need to make sure
                // to stop iterating before the group would go over the length of the string.
                // Also, we need to stop iterating before the very last character, as that
                // character doesn't have any letters following it to collect frequency
                // information about.
                for (int counter = 0; counter + groupSize < inputText.Length - 1; counter++)
                {
                    string currentGroup = inputText.Substring(counter, groupSize);
                    if (!(frequencies.ContainsKey(currentGroup)))
                    {
                        frequencies.Add(currentGroup, new List<string>());
                    }
                    frequencies[currentGroup].Add(Char.ToString(inputText[counter + groupSize]));
                }

                // Check that there's a space among the input we've just analyzed. If there isn't,
                // things will break, so warn the user and get out.
                if (!(frequencies.Any(x => x.Value.Contains(" "))))
                {
                    Console.WriteLine("Your input does not contain a space. Please add one and try again.");
                    Environment.Exit(1);
                }
            }

            return frequencies;
        }

        // Generates random words based on the given Markov chain frequency information.
        private string GenerateWords(Dictionary<string, List<string>> frequencies)
        {
            var words = new StringBuilder();
            var random = new Random();

            // get a random starting letter/word group
            string starter = "";
            if (useWords)
            {
                int startWordIndex = random.Next(0, frequencies.Keys.Count);
                starter = frequencies.Keys.ElementAt(startWordIndex);
                words.Append(starter);
            }

            else
            {
                // make sure starting letters are all alphanumeric
                bool isAlpha = false;
                while (!isAlpha)
                {
                    int startCharIndex = random.Next(0, frequencies.Keys.Count);
                    starter = frequencies.Keys.ElementAt(startCharIndex);
                    if (starter.All(x => Char.IsLetter(x)))
                    {
                        isAlpha = true;
                        words.Append(starter);
                    }
                }
            }

            string currentKey = starter;
            int wordCount = 0;
            while (wordCount < targetWordCount)
            {
                // Get the list of letters/words that tend to come after this letter/word group
                List<string> followingCharsWords = new List<string>();
                try
                {
                    followingCharsWords = frequencies[currentKey];
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Key '" + currentKey + "' not found. Try again, add more input, or decrease your group size.");
                    Environment.Exit(1);
                }

                // Get a random value from it. The more often a certain letter/word appears
                // in the list, the more often it'll be chosen!
                int randomCharWord = random.Next(0, followingCharsWords.Count);
                string followingCharWord = followingCharsWords[randomCharWord];

                // Add it to our word
                words.Append(followingCharWord);

                if (useWords)
                {
                    words.Append(" ");
                    wordCount++;
                }
                else
                {
                    // If it was a space, a word was finished, so increment the word count
                    if (followingCharWord.Equals(" "))
                        wordCount++;
                }

                // Re-set the key to use for finding following words
                if (useWords)
                {
                    if (groupSize > 1)
                    {
                        // need to include some of the previous words in the new key
                        currentKey = currentKey.Substring(currentKey.IndexOf(" ") + 1) + " " + followingCharWord;
                    }
                    else
                    {
                        currentKey = followingCharWord;
                    }
                }
                else
                {
                    currentKey = words.ToString().Substring(words.Length - groupSize, groupSize);
                }
            }
            
            return words.ToString();
        }

        private void WriteOutput(string text)
        {
            Console.WriteLine(text);
        }

        // Do miscellaneous formatting tasks to make the outputted words look better
        private string FormatOutput(string output)
        {
            var formattedOutput = new StringBuilder(output);

            // Capitalize the first letter
            formattedOutput[0] = Char.ToUpper(formattedOutput[0]);

            // Capitalize letters at the beginning of sentences
            List<int> sentenceEndings = AllIndexesOf(formattedOutput.ToString(), ". ");
            sentenceEndings.AddRange(AllIndexesOf(formattedOutput.ToString(), "? "));
            sentenceEndings.AddRange(AllIndexesOf(formattedOutput.ToString(), "! "));
            foreach (int index in sentenceEndings)
            {
                int sentenceStartIndex = index + 2;
                if (sentenceStartIndex < output.Length)
                {
                    formattedOutput[sentenceStartIndex] = Char.ToUpper(formattedOutput[sentenceStartIndex]);
                }
            }

            return formattedOutput.ToString();
        }

        // utility method to help with formatting the output
        private List<int> AllIndexesOf(string str, string value)
        {
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
    }
}
