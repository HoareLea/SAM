using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static CountryCode CountryCode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Core.CountryCode.Undefined;

            CountryCode result = Core.CountryCode.Undefined;
            if (Enum.TryParse(text, out result))
                return result;

            string value_1 = text.Trim().ToUpper();
            foreach (CountryCode countryCode in Enum.GetValues(typeof(CountryCode)))
            {
                string value_2 = countryCode.ToString().ToUpper();
                if (value_1.Equals(value_2))
                    return countryCode;

                value_2 = Description(countryCode).Trim().ToUpper();
                if (value_1.Equals(value_2))
                    return countryCode;
            }

            return Core.CountryCode.Undefined;
        }
    }
}