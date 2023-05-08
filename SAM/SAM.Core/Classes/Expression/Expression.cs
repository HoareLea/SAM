using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class Expression : IJSAMObject
    {
        private string text;
        
        public Expression(string text)
        {
            this.text = text;
        }

        public Expression(Expression expression)
        {
            text = expression?.text;
        }

        public Expression(JObject jObject)
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

        public List<ExpressionVariable> GetExpressionVariables(char openSymbol = '[', char closeSymbol = ']')
        {
            if (text == null)
                return null;

            List<ExpressionVariable> result = new List<ExpressionVariable>();
            List<string> texts = Query.Texts(text, true, openSymbol, closeSymbol);
            if (texts != null && texts.Count > 0)
                result = texts.ConvertAll(x => new ExpressionVariable(x));

            return result;
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

        public override bool Equals(object obj)
        {
            Expression expression = obj as Expression;
            if(expression == null)
            {
                return false;
            }

            return text == expression.text;
        }

        public override int GetHashCode()
        {
            if(text == null)
            {
                return -1;
            }

            return text.GetHashCode();
        }

        public static bool operator ==(Expression expression, object @object)
        {
            if (ReferenceEquals(expression, null) || ReferenceEquals(expression.text, null))
                return ReferenceEquals(@object, null) ? true : false;

            Expression expression_Temp = @object as Expression;
            if (expression_Temp == null)
                return false;

            return expression.text.Equals(expression_Temp.text);
        }

        public static bool operator !=(Expression expression, object @object)
        {
            return !(expression == @object);
        }

        public static implicit operator string(Expression expression) => expression?.text;

        public static implicit operator Expression(string @string) => new Expression(@string);
    }
}
