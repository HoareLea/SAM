using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class Result : SAMObject, IResult
    {
        private string reference;
        
        public Result(string name, string reference)
            : base(name)
        {
            this.reference = reference;
        }

        public Result(Guid guid, string name, string reference)
            : base(guid, name)
        {
            this.reference = reference;
        }

        public Result(Result result)
            : base(result)
        {
            reference = result?.reference;
        }

        public Result(JObject jObject)
            : base(jObject)
        {
        }

        public string Reference
        {
            get
            {
                return reference;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Reference"))
                reference = jObject.Value<string>("Reference");

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (reference != null)
                jObject.Add("Reference", reference);

            return jObject;
        }
    }
}