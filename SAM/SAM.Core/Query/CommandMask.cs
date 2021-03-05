using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool[] CommandMask(this string text, bool includeComment = false)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            int count = text.Length;

            char textOperator = Operator(CommandOperator.Text)[0];
            char openingBracketOperator = Operator(CommandOperator.OpeningBracket)[0];
            char closingBracketOperator = Operator(CommandOperator.ClosingBracket)[0];

            bool[] textMask = new bool[count];
            bool[] openingBracketMask = new bool[count];
            bool[] closingBracketMask = new bool[count];

            for (int i = 0; i < count; i++)
            {
                char @char = text[i];
                textMask[i] = @char == textOperator;
                openingBracketMask[i] = @char == openingBracketOperator;
                closingBracketMask[i] = @char == closingBracketOperator;
            }

            for (int i = 1; i < count; i++)
            {
                if (textMask[i] && textMask[i - 1])
                {
                    textMask[i] = false;
                    textMask[i - 1] = false;
                }
            }

            bool[] result = new bool[count];
            bool apostrophe = false;
            int braketCount = 0;
            for (int i = 0; i < count; i++)
            {
                if (textMask[i])
                    apostrophe = !apostrophe;

                if (!apostrophe)
                {
                    if (openingBracketMask[i])
                        braketCount++;
                    else if (closingBracketMask[i])
                        braketCount--;
                }

                result[i] = !apostrophe && braketCount == 0;
            }

            if(includeComment)
            {
                string @operator = Operator(CommandOperator.Comment);
                List<int> indexes = text.IndexesOf(@operator);
                indexes?.RemoveAll(x => !result[x]);
                if (indexes.Count != 0)
                {
                    int index = indexes.Max();
                    for (int i = index; i < count; i++)
                        result[i] = false;
                }
            }

            return result;
        }
    }
}