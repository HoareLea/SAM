using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class ExpressionVariable : IJSAMObject
    {
        private string text;
        
        public ExpressionVariable(string text)
        {
            this.text = text;
        }

        public ExpressionVariable(ExpressionVariable expressionVariable)
        {
            text = expressionVariable?.text;
        }

        public ExpressionVariable(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public bool TryGetProperties(out ExpressionVariable expressionVariable, out string name, char openSymbol = '[', char closeSymbol = ']')
        {
            expressionVariable = null;
            name = null;

            if (text == null)
                return false;

            if (text.IndexOf(openSymbol) == -1)
            {
                name = text;
                return true;
            }

            List<string> texts = Query.Texts(Text, openSymbol, closeSymbol);
            if(texts == null || texts.Count == 0)
            {
                name = text;
                return true;
            }

            expressionVariable = new ExpressionVariable(texts[0]);

            int length = text.IndexOf(texts[0]) - 1;
            if (length < 1)
                name = string.Empty;
            else
                name = text.Substring(0, length);

            return true;
        }

        public string GetName()
        {
            if (!TryGetProperties(out ExpressionVariable expressionVariable, out string name))
                return null;

            return name;
        }

        public ExpressionVariable GetExpressionVariable()
        {
            if (!TryGetProperties(out ExpressionVariable expressionVariable, out string name))
                return null;

            return expressionVariable;
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

        public static bool operator ==(ExpressionVariable expressionVariable, object @object)
        {
            if (ReferenceEquals(expressionVariable, null) || ReferenceEquals(expressionVariable.text, null))
                return ReferenceEquals(@object, null) ? true : false;

            ExpressionVariable expression_Temp = @object as ExpressionVariable;
            if (expression_Temp == null)
                return false;

            return expressionVariable.text.Equals(expression_Temp.text);
        }

        public static bool operator !=(ExpressionVariable expressionVariable, object @object)
        {
            return !(expressionVariable == @object);
        }

        public static implicit operator string(ExpressionVariable expressionVariable) => expressionVariable?.text;

        public static implicit operator ExpressionVariable(string @string) => new ExpressionVariable(@string);
    }
}
