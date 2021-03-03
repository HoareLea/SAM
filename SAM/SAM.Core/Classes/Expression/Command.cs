using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class Command : IJSAMObject
    {
        private string text;

        public Command(string text = null)
        {
            this.text = text;
        }

        public List<Command> GetCommands()
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string text_Trim = text.Trim();

            if (string.IsNullOrEmpty(text_Trim))
                return null;

            int count = text_Trim.Length;

            char textOperator = SAM.Core.Query.Operator(CommandOperator.Text)[0];
            char openingBracketOperator = SAM.Core.Query.Operator(CommandOperator.OpeningBracket)[0];
            char closingBracketOperator = SAM.Core.Query.Operator(CommandOperator.ClosingBracket)[0];

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

            bool[] expressionMask = new bool[count];
            bool apostrophe = false;
            bool braket = false;
            for (int i = 0; i < count; i++)
            {
                if (!apostrophe && textMask[i])
                    apostrophe = true;
                else if (apostrophe && textMask[i])
                    apostrophe = false;

                if (!apostrophe)
                {
                    if (!braket && openingBracketMask[i])
                        braket = true;
                    else if (braket && closingBracketMask[i])
                        braket = false;
                }

                expressionMask[i] = !apostrophe && !braket;
            }

            List<int> indexes = new List<int>();
            List<string> operators = new List<string>();

            List<Enum> enums = SAM.Core.Query.Enums(typeof(ArithmeticOperator), typeof(RelationalOperator), typeof(LogicalOperator), typeof(IncrementAndDecrementOperator), typeof(AssignmentOperator), typeof(BitwiseOperator));
            foreach (Enum @enum in enums)
            {
                string @operator = SAM.Core.Query.Operator(@enum);
                List<int> indexes_Temp = text_Trim.IndexesOf(@operator);
                indexes_Temp?.RemoveAll(x => !expressionMask[x]);
                if (indexes_Temp != null && indexes_Temp.Count > 0)
                {
                    indexes.AddRange(indexes_Temp);
                    operators.Add(@operator);
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

                string text_Temp = text_Trim.Substring(startIndex);
                List<string> operators_Temp = operators.FindAll(x => text_Temp.StartsWith(x));
                if (operators_Temp == null || operators_Temp.Count == 0)
                {
                    text_Temp = text_Temp.Substring(0, length).Trim();
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

                result.Add(new Command(@operator));
                result.Add(new Command(text_Temp.Substring(@operator.Length, length - @operator.Length).Trim()));
            }

            return result;
        }

        public bool IsOperator(out Enum @enum)
        {
            @enum = CommandOperator.Undefined;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            List<Enum> enums = SAM.Core.Query.Enums(
                typeof(LogicalOperator),
                typeof(ArithmeticOperator),
                typeof(RelationalOperator),
                typeof(IncrementAndDecrementOperator),
                typeof(AssignmentOperator),
                typeof(BitwiseOperator),
                typeof(CommandOperator));

            string text_Trim = text.Trim();
            foreach (Enum enum_Temp in enums)
            {
                string @operator = SAM.Core.Query.Operator(enum_Temp);
                if (text_Trim == @operator)
                {
                    @enum = enum_Temp;
                    return true;
                }
            }

            return false;
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public override string ToString()
        {
            return text;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Text"))
                text = jObject.Value<string>("Text");

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            if (text != null)
                jObject.Add("Text", text);

            return jObject;
        }

        public static bool operator ==(Command command, object @object)
        {
            if (ReferenceEquals(command, null) || ReferenceEquals(command.text, null))
                return ReferenceEquals(@object, null) ? true : false;

            Command command_Temp = @object as Command;
            if (command_Temp == null)
                return false;

            return command_Temp.text == command.text;
        }

        public static bool operator !=(Command command, object @object)
        {
            return !(command == @object);
        }

        public static implicit operator string(Command command) => command?.text;

        public static implicit operator Command(string @string) => new Command(@string);
    }
}
