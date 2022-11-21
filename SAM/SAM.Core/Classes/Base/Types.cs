using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class Types : IJSAMObject
    {
        private List<object> types;

        public Types()
        {

        }

        public Types(IEnumerable<Type> types)
        {
            if(types != null)
            {
                this.types = new List<object>();
                foreach(Type type in types)
                {
                    this.types.Add(type);
                }
            }
        }

        public Types(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Types(Types displayTypes)
        {
            if (displayTypes != null)
            {
                if (displayTypes.types != null)
                {
                    types = new List<object>();
                    foreach (object @object in displayTypes.types)
                    {
                        types.Add(@object);
                    }
                }
            }
        }

        public bool Contains(string fullTypeName)
        {
            if(types == null || string.IsNullOrWhiteSpace(fullTypeName) || types.Count == 0)
            {
                return false;
            }

            foreach(object @object in types)
            {
                if(@object is string)
                {
                    if(fullTypeName.Equals((string)@object))
                    {
                        return true;
                    }
                }
                else if(@object is Type)
                {
                    string fullTypeName_Temp = Query.FullTypeName((Type)@object);
                    if(fullTypeName.Equals(fullTypeName_Temp))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Contains(Type type)
        {
            if(type == null || types == null || types.Count == 0)
            {
                return false;
            }

            string fullTypeName = Query.FullTypeName(type);

            foreach (object @object in types)
            {
                if(@object == null)
                {
                    continue;
                }

                if (@object is string)
                {
                    if (fullTypeName.Equals((string)@object))
                    {
                        return true;
                    }
                }
                else if (@object is Type)
                {
                    string fullTypeName_Temp = Query.FullTypeName((Type)@object);
                    if (fullTypeName.Equals(fullTypeName_Temp))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Types"))
            {
                types = new List<object>();

                JArray jArray = jObject.Value<JArray>("Types");
                foreach (string typeName in jArray)
                {
                    Type type = Core.Query.Type(typeName, true);
                    if (type == null)
                    {
                        types.Add(typeName);
                    }
                    else
                    {
                        types.Add(type);
                    }
                }
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (types != null)
            {
                JArray jArray = new JArray();
                foreach (object @object in types)
                {
                    string typeName = null;
                    if (@object is Type)
                    {
                        typeName = Core.Query.FullTypeName((Type)@object);
                    }
                    else if (@object is string)
                    {
                        typeName = (string)@object;
                    }

                    if (string.IsNullOrWhiteSpace(typeName))
                    {
                        continue;
                    }

                    jArray.Add(typeName);
                }

                jObject.Add("Types", jArray);
            }

            return jObject;
        }
    }
}