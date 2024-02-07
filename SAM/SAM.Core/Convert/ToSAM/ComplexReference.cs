using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static T ComplexReference<T>(string @string) where T : IComplexReference
        {
            IComplexReference complexReference = ComplexReference(@string);
            if (complexReference == null)
            {
                return default;
            }

            if (complexReference is T)
            {
                return (T)complexReference;
            }

            return default;
        }

        public static IComplexReference ComplexReference(string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                return null;
            }

            ObjectReference objectReference = ObjectReference(@string, out string string_out);
            if (string.IsNullOrWhiteSpace(string_out))
            {
                return objectReference;
            }

            List<ObjectReference> objectReferences = new List<ObjectReference>() { objectReference };
            while(!string.IsNullOrWhiteSpace(string_out) && @string != string_out)
            {
                @string = string_out;
                objectReference = ObjectReference(@string, out string_out);
                if(objectReference == null)
                {
                    break;
                }

                objectReferences.Add(objectReference);
            }

            return new PathReference(objectReferences);
        }
    }
}