# MarkovTextGenerator
Builds a configurable n-gram model of input text, then generates randomized output text using probabilistic Markov chains. Essentially, given a text file, this program analyzes which letters or words often come after each letter or word. Then it generates new text using those probabilities as weights, resulting in generated text more or less similar to the input text. 

## Usage

`mtg.exe -i *input_path* -o *num_words_to_generate* [-g *n-gram_size*] [-w]`

Input path: Path to a plain text file with at least one space.

Num words to generate: Number of words to generate from the input text.

N-gram size (formerly "group size"): Number of characters or words to group together and consider as one unit when building the Markov chain. Defaults to 1. The larger the n-gram size, the closer the generated text will be to the input text. (This used to be called "group size" before I learned the term "n-gram".)

-w: Calculate probabilities based on words (delimited by spaces) instead of by chunks of characters.

## Examples

`mtg.exe -i NalhallanText.txt -o 16`

`mtg.exe -i NalhallanText.txt -o 16 -g 2`

`mtg.exe -i NalhallanText.txt -o 16 -w`

`mtg.exe -i NalhallanText.txt -o 16 -g 2 -w`
