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

        public bool TryGetProperties(out string typeName, out string name)
        {
            typeName = null;
            name = null;

            if (text == null)
                return false;

            if (text.IndexOf('[') == -1)
            {
                name = text;
                return true;
            }

            List<string> texts = Query.Texts(Text, '[', ']');
            if(texts == null || texts.Count == 0)
            {
                name = text;
                return true;
            }

            name = texts[0];

            int length = text.IndexOf(texts[0]) - 1;
            if (length < 1)
                typeName = string.Empty;
            else
                typeName = text.Substring(0, length);

            return true;
        }

        public string Name
        {
            get
            {
                if (!TryGetProperties(out string typeName, out string name))
                    return null;

                return name;
            }
        }

        public string TypeName
        {
            get
            {
                if (!TryGetProperties(out string typeName, out string name))
                    return null;

                return typeName;
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
