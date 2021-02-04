using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class Result : SAMObject, IResult
    {
        private string source;
        private string reference;
        private DateTime dateTime;

        public Result(string name, string source, string reference)
            : base(name)
        {
            this.source = source;
            this.reference = reference;
            dateTime = DateTime.Now;
        }

        public Result(Guid guid, string name, string source, string reference)
            : base(guid, name)
        {
            this.source = source;
            this.reference = reference;
            dateTime = DateTime.Now;
        }

        public Result(Result result)
            : base(result)
        {
            source = result?.source;
            reference = result?.reference;
            dateTime = result == null ? DateTime.MinValue : result.dateTime;
        }

        public Result(Guid guid, Result result)
            : base(guid, result)
        {
            source = result?.source;
            reference = result?.reference;
            dateTime = result == null ? DateTime.MinValue : result.dateTime;
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

        public string Source
        {
            get
            {
                return source;
            }
        }

        public DateTime DateTime
        {
            get
            {
                return dateTime;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Source"))
                reference = jObject.Value<string>("Source");

            if (jObject.ContainsKey("Reference"))
                reference = jObject.Value<string>("Reference");

            if (jObject.ContainsKey("DateTime"))
                dateTime = jObject.Value<DateTime>("DateTime");
            else
                dateTime = DateTime.MinValue;

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (source != null)
                jObject.Add("Source", source);

            if (reference != null)
                jObject.Add("Reference", reference);

            if (dateTime != DateTime.MinValue)
                jObject.Add("DateTime", dateTime);

            return jObject;
        }
    }
}