using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Common.Model
{
    public class WordProcessorResult
    {

        public WordProcessorResult(params string[] words) => Words = words.ToList();
        public WordProcessorResult(IEnumerable<string> words) => Words = words.ToList();
        public WordProcessorResult(MatchCollection matches)
        {
            List<string> words = new List<string>();
            foreach (Match m in matches)
            {
                words.Add(m.Value);
            }
            Words = words;
        }


        public List<string> Words { get; }

        public int Count => Words.Count;


        public StringTriple GetAsTriple()
        {
            if (Count != 3)
            {
                throw new InvalidOperationException("Cant generate Triple if there are not exactly three words in the WordProcessorResult");
            }
            return new StringTriple(Words.ToArray());

        }

        public List<WordProcessorResult> GetSubcollectionOfLengthThreeInRow()
        {
            if (Count <= 3)
            {
                return new List<WordProcessorResult>() { new WordProcessorResult(Words) };
            }


            List<WordProcessorResult> result = new List<WordProcessorResult>();
            for (int i = 0; i <= Count - 3; i++)
            {
                var subcollection = Words.Skip(i).Take(3);
                result.Add(new WordProcessorResult(subcollection));
            }
            return result;
        }


        public List<StringTriple> GetSubcollectionOfLengthThreeAnyOrderedCombination()
        {
            if (Count <= 3)
            {
                return new List<StringTriple>() { new StringTriple(Words) };
            }


            List<StringTriple> result = new List<StringTriple>();
            for (int i = 0; i <= Count - 3; i++)
            {
                for (int j = i+1; j <= Count -2; j++)
                {
                    for (int k = j + 1; k <= Count -1; k++)
                    {
                        result.Add(new StringTriple(Words[i], Words[j], Words[k]));

                    }
                }
            }
            return result;
        }

        public override string ToString()
        {
            return string.Join(" ", Words);
        }
    }
}
