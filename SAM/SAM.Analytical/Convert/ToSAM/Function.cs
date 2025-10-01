using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static Function ToSAM_Function(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] texts = text.Split(',');
            if(texts is null || texts.Length == 0)
            {
                return null;
            }

            string name = texts[0];

            List<double> values = [];
            for (int i = 1; i < texts.Length; i++)
            {
                if (Core.Query.TryConvert(texts[i], out double value))
                {
                    values.Add(value);
                }
            }

            return new Function(name, values);
        }
    }
}