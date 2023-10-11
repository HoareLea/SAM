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

        public static ObjectReference ObjectReference(string @string, out string string_out)
        {
            string_out = null;

            if (string.IsNullOrWhiteSpace(@string))
            {
                return null;
            }

            int index = -1;

            string string_Temp = @string.TrimStart();
            if(string_Temp.StartsWith("->"))
            {
                string_Temp = string_Temp.Substring(2);
                string_Temp = string_Temp.TrimStart();
            }

            string_out = string_Temp;
            if (string.IsNullOrWhiteSpace(string_out))
            {
                return null;
            }

            index = string_out.IndexOf("::");
            if (index == -1)
            {
                if (string_out.StartsWith(@""""))
                {
                    string propertyName_Temp = Query.QuotedText(string_out, out string_out);
                    if(!string.IsNullOrWhiteSpace(propertyName_Temp))
                    {
                        return new PropertyReference(propertyName_Temp);
                    }
                }
                else
                {
                    string typeName_Temp = string_out;
                    string_out = null;
                    return new ObjectReference(typeName_Temp);
                }
                return null;
            }

            string value_1 = null;
            string value_2 = null;
            string value_3 = null;

            value_1 = string_out.Substring(0, index);
            int index_Temp = value_1.IndexOf("->");
            if (index_Temp != -1) 
            { 
                string typeName_Temp = value_1.Substring(0, index_Temp);
                string_out = string_out.Substring(index_Temp);
                return new ObjectReference(typeName_Temp);
            }

            string_out = string_out.Substring(index + 2);
            if(string_out.StartsWith(@""""))
            {
                value_3 = Query.QuotedText(string_out, out string_out);
            }
            if (string_out.StartsWith(@"["))
            {
                int indexEnd = string_out.IndexOf("]");
                if (indexEnd == -1)
                {
                    value_2 = string_out.Substring(1, indexEnd);
                    string_out = string.Empty;
                }
                else
                {
                    value_2 = string_out.Substring(1, indexEnd - 1);
                    string_out = string_out.Substring(indexEnd + 1); ;
                }
            }

            if (string_out.StartsWith(@"::"))
            {
                string_out = string_out.Substring(2);
            }

            if (string_out.StartsWith(@""""))
            {
                value_3 = Query.QuotedText(string_out, out string_out);
            }

            if (value_1.StartsWith(@"["))
            {
                value_2 = value_1;
                value_1 = null;
            }

            string typenName = value_1;
            Reference? reference = null;
            string propertyName = null;

            if(!string.IsNullOrWhiteSpace(value_2))
            {
                if(value_2.StartsWith("["))
                {
                    value_2 = value_2.Substring(1);
                }

                if(value_2.EndsWith("]"))
                {
                    value_2 = value_2.Substring(0, value_2.Length - 1);
                }

                if(System.Guid.TryParse(value_2, out System.Guid guid))
                {
                    reference = new Reference(guid);
                }
                else if(int.TryParse(value_2, out int @int))
                { 
                    reference = new Reference(@int);
                }
                else
                {
                    reference = new Reference(value_2);
                }
            }

            if(!string.IsNullOrWhiteSpace(value_3))
            {
                if (value_3.StartsWith(@""""))
                {
                    value_3 = value_3.Substring(1);
                }

                if (value_3.EndsWith(@""""))
                {
                    value_3 = value_3.Substring(0, value_3.Length - 1);
                }

                propertyName = value_3;
            }

            ObjectReference objectReference = new ObjectReference(typenName, reference);

            if(!string.IsNullOrWhiteSpace(propertyName))
            {
                return new PropertyReference(objectReference, propertyName);
            }

            return objectReference;
        }
    }
}