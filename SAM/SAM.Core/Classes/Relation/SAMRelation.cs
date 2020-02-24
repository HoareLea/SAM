using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class SAMRelation : ISAMRelation, IJSAMObject
    {
        private object @object;
        private object relatedObject;

        public SAMRelation(object @object, object relatedObject)
        {
            this.@object = @object;
            this.relatedObject = relatedObject;

        }

        public SAMRelation(SAMRelation sAMRelation)
        {
            this.@object = sAMRelation.@object;
            this.relatedObject = sAMRelation.relatedObject;

        }

        public object Object
        {
            get
            {
                return @object;
            }
        }

        public object RelatedObject
        {
            get
            {
                return relatedObject;
            }
        }

        public override bool Equals(object @object)
        {
            SAMRelation sAMRelation = @object as SAMRelation;

            if (sAMRelation == null)
                return false;

            return ReferenceEquals(this.@object, sAMRelation.@object) && ReferenceEquals(this.relatedObject, sAMRelation.relatedObject);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + (!ReferenceEquals(null, @object) ? @object.GetHashCode() : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, relatedObject) ? relatedObject.GetHashCode() : 0);
            return hash;
        }

        public T GetObject<T>()
        {
            if (@object == null)
                return default;
            
            if (typeof(T).IsAssignableFrom(@object.GetType()))
                return (T)(object)@object;

            return default;
        }

        public T GetRelatedObject<T>()
        {
            if (relatedObject == null)
                return default;

            if (typeof(T).IsAssignableFrom(relatedObject.GetType()))
                return (T)(object)relatedObject;

            return default;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if(jObject.ContainsKey("Object"))
            {
                JToken jToken = jObject.GetValue("Object");
                switch(jToken.Type)
                {
                    case JTokenType.Object:
                        @object = Create.IJSAMObject((JObject)jToken);
                        break;
                    case JTokenType.Boolean:
                        @object = jToken.Value<bool>();
                        break;
                    case JTokenType.Integer:
                        @object = jToken.Value<int>();
                        break;
                    case JTokenType.String:
                        @object = jToken.Value<string>();
                        break;
                    case JTokenType.Float:
                        @object = jToken.Value<double>();
                        break;
                }
            }

            if (jObject.ContainsKey("RelatedObject"))
            {
                JToken jToken = jObject.GetValue("RelatedObject");
                switch (jToken.Type)
                {
                    case JTokenType.Object:
                        relatedObject = Create.IJSAMObject((JObject)jToken);
                        break;
                    case JTokenType.Boolean:
                        relatedObject = jToken.Value<bool>();
                        break;
                    case JTokenType.Integer:
                        relatedObject = jToken.Value<int>();
                        break;
                    case JTokenType.String:
                        relatedObject = jToken.Value<string>();
                        break;
                    case JTokenType.Float:
                        relatedObject = jToken.Value<double>();
                        break;
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.TypeName(this));

            if (@object != null)
            {
                if (@object is IJSAMObject)
                    jObject.Add("Object", ((IJSAMObject)@object).ToJObject());
                else if (@object is double)
                    jObject.Add("Object", (double)@object);
                else if (@object is string)
                    jObject.Add("Object", (string)@object);
                else if (@object is int)
                    jObject.Add("Object", (int)@object);
                else if (@object is bool)
                    jObject.Add("Object", (bool)@object);
            }

            if (relatedObject != null)
            {
                if (relatedObject is IJSAMObject)
                    jObject.Add("RelatedObject", ((IJSAMObject)relatedObject).ToJObject());
                else if (relatedObject is double)
                    jObject.Add("RelatedObject", (double)relatedObject);
                else if (relatedObject is string)
                    jObject.Add("RelatedObject", (string)relatedObject);
                else if (relatedObject is int)
                    jObject.Add("RelatedObject", (int)relatedObject);
                else if (relatedObject is bool)
                    jObject.Add("RelatedObject", (bool)relatedObject);
            }

            return jObject;
        }
    }
}
