using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string Text(this CountryCode countryCode)
        {
            return Description(countryCode);
        }

        public static string Text(this IComplexReference complexReference)
        {
            if(complexReference == null)
            {
                return null;
            }


            if (complexReference is PathReference)
            {
                ObjectReference objectReference_Last = ((PathReference)complexReference)?.LastOrDefault();
                if (objectReference_Last == null)
                {
                    return null;
                }

                return Text(objectReference_Last);
            }

            ObjectReference objectReference = complexReference as ObjectReference;
            if (objectReference == null)
            {
                return null;
            }

            string result = objectReference.Reference?.ToString();
            if (string.IsNullOrWhiteSpace(result))
            {
                result = objectReference.Type?.Name;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = objectReference.TypeName;
                }
            }

            if (complexReference is PropertyReference)
            {
                string propertyName = ((PropertyReference)complexReference).PropertyName;
                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    result = string.IsNullOrWhiteSpace(result) ? propertyName : string.Format("{0} {1}", result, propertyName);
                }
            }

            return result;
        }
    }
}