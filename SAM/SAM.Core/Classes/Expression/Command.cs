using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

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

            List<Enum> enums = Query.Enums(typeof(ArithmeticOperator), typeof(RelationalOperator), typeof(LogicalOperator), typeof(IncrementAndDecrementOperator), typeof(AssignmentOperator), typeof(BitwiseOperator));
            enums.Add(CommandOperator.Comment);

            return Create.Commands(text, enums);
        }

        public bool IsOperator(out Enum @enum)
        {
            @enum = CommandOperator.Undefined;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            List<Enum> enums = Query.Enums(
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
                string @operator = Query.Operator(enum_Temp);
                if (text_Trim == @operator)
                {
                    @enum = enum_Temp;
                    return true;
                }
            }

            return false;
        }

        public bool IsOperator()
        {
            return IsOperator(out Enum @enum);
        }

        public bool IsObject(out string name, out List<Command> members)
        {
            members = null;
            name = null;

            if (!IsCommandOperator(CommandOperator.Object, out string value))
                return false;

            List<Command> commands = Create.Commands(value, new Enum[] { CommandOperator.MemberSeparator });
            if (commands == null || commands.Count == 0)
                return false;

            name = commands[0].Text;
            commands.RemoveAt(0);

            if (commands.Count != 0)
                members = commands.FindAll(x => !x.IsOperator(out Enum @enum));

            return true;
        }

        public bool IsObject()
        {
            return IsObject(out string name, out List<Command> members);
        }

        public bool IsValue(out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string text_Trim = text.Trim();

            if (text_Trim == "null")
            {
                value = null;
                return true;
            }

            string @operator = Query.Operator(CommandOperator.Text);

            if(text_Trim.Length >= 2 && text_Trim[0] == @operator[0] && text_Trim[text_Trim.Length - 1] == @operator[0])
            {
                value = text_Trim.Substring(1, text_Trim.Length - 2);
                return true;
            }

            decimal @decimal;

            if (decimal.TryParse(text_Trim, NumberStyles.Integer, CultureInfo.InvariantCulture, out @decimal))
            {
                value = System.Convert.ToInt32(@decimal);
                return true;
            }

            if (decimal.TryParse(text_Trim, NumberStyles.Float, CultureInfo.InvariantCulture, out @decimal))
            {
                value = System.Convert.ToDouble(@decimal);
                return true;
            }

            return false;
        }

        public bool IsValue()
        {
            return IsValue(out object value);
        }

        public bool IsDirective(out string directive)
        {
            return IsCommandOperator(CommandOperator.Directive, out directive);
        }

        public bool IsDirective()
        {
            return IsDirective(out string directive);
        }

        public bool IsCommand(out string name, out Command command, out List<Command> members)
        {
            name = null;
            command = null;
            members = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string text_Trim = text.Trim();

            if (string.IsNullOrWhiteSpace(text_Trim))
                return false;

            if (IsOperator() || IsObject() || IsComment() || IsValue() || IsDirective())
                return false;

            bool[] mask = Query.CommandMask(text_Trim);
            if (mask == null || mask.Length == 0)
                return false;

            int index_Start = -1;
            for (int i = 0; i < mask.Length; i++)
                if (!mask[i])
                {
                    name = text_Trim.Substring(0, i);
                    index_Start = i;
                    break;
                }

            if (index_Start == -1)
                return false;

            int index_End = -1;
            for (int i = index_Start; i < mask.Length; i++)
                if (mask[i])
                {
                    index_End = i - 1;
                    break;
                }

            if (index_End == -1)
                return false;

            command = new Command(text_Trim.Substring(index_Start + 1, index_End - index_Start));

            text_Trim = text_Trim.Substring(index_Start);
            List<Command> commands = Create.Commands(text_Trim, new Enum[] { CommandOperator.MemberSeparator });
            if(commands != null && commands.Count != 0)
            {
                commands.RemoveAt(0);
                if (commands.Count != 0)
                    members = commands.FindAll(x => !x.IsOperator(out Enum @enum_Temp));
            }
            
            return true;
        }

        public bool IsCommand()
        {
            return IsCommand(out string name, out Command command, out List<Command> members);
        }

        public bool IsComment(out string comment)
        {
            return IsCommandOperator(CommandOperator.Comment, out comment);
        }

        public bool IsComment()
        {
            return IsComment(out string comment);
        }
        
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(text);
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

        private bool IsCommandOperator(CommandOperator commandOperator, out string value)
        {
            value = null;

            if (string.IsNullOrEmpty(text))
                return false;

            string text_Trim = text.Trim();

            if (string.IsNullOrEmpty(text_Trim))
                return false;

            string objectOperator = Query.Operator(commandOperator);

            if (!text_Trim.StartsWith(objectOperator))
                return false;

            value = text_Trim.Substring(objectOperator.Length);
            return true;
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
