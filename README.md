# MarkovTextGenerator
Generates text similar to input text using Markov probability chains.

Given a text file, this program analyzes the pattern of text in the file, creating Markov chains of letters/words that are statistically likely to appear after each letter/word. Then it can generate text similar to the input using the Markov chains.

## Usage

`mtg.exe -i *input_path* -o *num_words_to_generate* [-g *group_size*] [-w]`

Input path: Path to a plain text file with at least one space.

Num words to generate: Number of words to generate from the input text.

Group size: Number of characters or words to group together in Markov chains. Defaults to 1. The larger the group size, the closer the generated text will be to the input text.

-w: Calculate probabilities based on words (delimited by spaces) instead of by chunks of characters.

## Examples

`mtg.exe -i NalhallanText.txt -o 16`

`mtg.exe -i NalhallanText.txt -o 16 -g 2`

`mtg.exe -i NalhallanText.txt -o 16 -w`

`mtg.exe -i NalhallanText.txt -o 16 -g 2 -w`
