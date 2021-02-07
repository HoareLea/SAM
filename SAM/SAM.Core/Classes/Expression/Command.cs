using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class Command : IJSAMObject
    {
        public static class Operators
        {
            public static class Arithmetic
            {
                public const string Addition = "+";
                public const string Subtraction = "-";
                public const string Multiplication = "*";
                public const string Division = "/";
                public const string Modulus = "%";
            }

            public static class Relational
            {
                public const string Equal = "==";
                public const string NotEqual = "!=";
                public const string GreaterThan = ">";
                public const string LessThan = ">=";
                public const string LessThanOrEqual = "<=";
            }

            public static class Bitwise
            {
                public const string And = "&";
                public const string Or = "|";
                public const string Xor = "^";
                public const string Not = "~";
                public const string LeftShift = "<<";
                public const string RightShift = ">>";
            }

            public static class Logical
            {
                public const string And = "&&";
                public const string Or = "||";
                public const string Not = "!";
            }

            public static class Assignment
            {
                public const string Assign = "=";
                public const string AddAndAssign = "+=";
                public const string SubstractAndAssign = "-=";
                public const string MultiplyAndAssign = "*=";
                public const string DivideAndAssign = "/=";
                public const string ModulusAndAssign = "%=";
                public const string LeftShiftAndAssign = "<<=";
                public const string RightShiftAndAssign = ">>=";
                public const string BitwiseAndAndAssign = "&=";
                public const string BitwiseOrAndAssign = "|=";
                public const string BitwiseXorAndAssign = "^=";
            }

            public static class IncrementAndDecrement
            {
                public const string Increment = "++";
                public const string Decrement = "--";
            }

        }

        public static class Syntax
        {
            public const char Apostrophe = '\'';
            public const char OpeningBracket = '(';
            public const char ClosingBracket = ')';
        }
        
        
        private string text;
        
        public Command(string text)
        {
            this.text = text;
        }

        public Command(Command command)
        {
            text = command?.text;
        }

        public Command(JObject jObject)
        {
            FromJObject(jObject);
        }

        public List<Command> GetCommands()
        {
            if (string.IsNullOrEmpty(text))
                return null;

            int count = text.Length;

            bool[] apostropheMask = Enumerable.Repeat(false, count).ToArray();
            bool[] openingBracketMask = Enumerable.Repeat(false, count).ToArray();
            bool[] closingBracketMask = Enumerable.Repeat(false, count).ToArray();

            for(int i=0; i < count; i++)
            {
                switch(text[i])
                {
                    case Syntax.Apostrophe:
                        apostropheMask[i] = true;
                        break;

                    case Syntax.OpeningBracket:
                        openingBracketMask[i] = true;
                        break;

                    case Syntax.ClosingBracket:
                        closingBracketMask[i] = true;
                        break;
                }
            }

            for (int i = 1; i < count; i++)
            {
                if(apostropheMask[i] && apostropheMask[i -1])
                {
                    apostropheMask[i] = false;
                    apostropheMask[i - 1] = false;
                }
            }

            int index = 0;
            while(index < count)
            {
                switch(text[index])
                {
                    case Syntax.Apostrophe:

                        break;

                    case Syntax.OpeningBracket:

                        break;
                }

                index++;
            }

            throw new System.NotImplementedException();
        }

        public string Text
        {
            get
            {
                return text;
            }
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
