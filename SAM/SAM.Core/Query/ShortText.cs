using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string ShortText(this IComplexReference complexReference)
        {
            if(complexReference == null)
            {
                return null;
            }

            if(complexReference is PathReference)
            {
                ObjectReference objectReference_Last = ((PathReference)complexReference)?.LastOrDefault();
                if(objectReference_Last == null)
                {
                    return null;
                }

                return ShortText(objectReference_Last);
            }

            if(complexReference is PropertyReference)
            {
                string propertyName = ((PropertyReference)complexReference).PropertyName;
                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    return propertyName;
                }
            }

            ObjectReference objectReference = complexReference as ObjectReference;
            if(objectReference == null)
            {
                return null;
            }

            string result = objectReference.Reference?.ToString();
            if(!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }


            result = objectReference.Type?.Name;
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            return objectReference.TypeName;
        }
    }
}