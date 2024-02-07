using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public abstract class TMResult : Result, IAnalyticalObject
    {
        public TMResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public TMResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public TMResult(TMResult tMResult)
            : base(tMResult)
        {

        }

        public TMResult(Guid guid, TMResult tMResult)
            : base(guid, tMResult)
        {

        }

        public TMResult(JObject jObject)
            : base(jObject)
        {

        }

        public abstract int OccupiedHours { get; }

        public abstract int MaxExceedableHours { get; }

        public abstract bool Pass { get; }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return null;
            }

            return result;
        }
    }
}