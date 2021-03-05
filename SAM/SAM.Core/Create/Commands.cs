
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<Command> Commands(this string text, IEnumerable<Enum> enums)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            int count = text.Length;

            bool[] mask = Query.CommandMask(text);
            if (mask == null)
                return null;

            List<int> indexes = new List<int>();
            List<string> operators = new List<string>();

            if(enums != null)
            {
                foreach (Enum @enum in enums)
                {
                    string @operator = Query.Operator(@enum);
                    if (string.IsNullOrWhiteSpace(@operator))
                        continue;

                    List<int> indexes_Temp = text.IndexesOf(@operator);
                    indexes_Temp?.RemoveAll(x => !mask[x]);
                    if (indexes_Temp != null && indexes_Temp.Count > 0)
                    {
                        indexes.AddRange(indexes_Temp);
                        operators.Add(@operator);
                    }
                }
            }

            indexes.Add(0);

            indexes = indexes.Distinct().ToList();

            indexes.Sort();

            List<Command> result = new List<Command>();
            while (indexes.Count > 0)
            {
                int startIndex = indexes[0];
                indexes.RemoveAt(0);

                int endIndex = count;
                if (indexes.Count > 0)
                    endIndex = indexes[0];

                int length = endIndex - startIndex;

                string text_Temp = text.Substring(startIndex);
                List<string> operators_Temp = operators.FindAll(x => text_Temp.StartsWith(x));
                if (operators_Temp == null || operators_Temp.Count == 0)
                {
                    text_Temp = text_Temp.Substring(0, length).Trim();
                    if (!string.IsNullOrWhiteSpace(text_Temp))
                        result.Add(new Command(text_Temp));
                    continue;
                }

                string @operator = null;
                if (operators_Temp.Count == 1)
                {
                    @operator = operators_Temp[0];
                }
                else
                {
                    operators_Temp.Sort((x, y) => y.Length.CompareTo(x.Length));
                    @operator = operators_Temp[0];

                    int index = startIndex;
                    while (indexes.Count > 0 && indexes[0] < index + @operator.Length)
                        indexes.RemoveAt(0);

                    endIndex = count;
                    if (indexes.Count > 0)
                        endIndex = indexes[0];

                    length = endIndex - startIndex;
                }

                if (string.IsNullOrWhiteSpace(@operator))
                    continue;

                if (@operator == CommandOperator.Comment.Operator())
                {
                    text_Temp = text_Temp.Substring(0, length).Trim();
                    if (!string.IsNullOrWhiteSpace(text_Temp))
                        result.Add(new Command(text_Temp));
                }
                else
                {
                    result.Add(new Command(@operator));
                    text_Temp = text_Temp.Substring(@operator.Length, length - @operator.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(text_Temp))
                        result.Add(new Command(text_Temp));
                }
            }

            return result;
        }
    }
}