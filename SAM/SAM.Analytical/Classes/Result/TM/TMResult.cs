using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public abstract class TMResult : Result, IAnalyticalObject
    {
        private TM52BuildingCategory tM52BuildingCategory;

        public TMResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory)
            : base(name, source, reference)
        {
            this.tM52BuildingCategory = tM52BuildingCategory;
        }

        public TMResult(Guid guid, string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory)
            : base(guid, name, source, reference)
        {
            this.tM52BuildingCategory = tM52BuildingCategory;
        }

        public TMResult(TMResult tMResult)
            : base(tMResult)
        {
            if(tMResult != null)
            {
                tM52BuildingCategory = tMResult.tM52BuildingCategory;
            }
        }

        public TMResult(Guid guid, TMResult tMResult)
            : base(guid, tMResult)
        {
            if(tMResult != null)
            {
                tM52BuildingCategory = tMResult.tM52BuildingCategory;
            }
        }

        public TMResult(JObject jObject)
            : base(jObject)
        {

        }

        public TM52BuildingCategory TM52BuildingCategory
        {
            get
            {
                return tM52BuildingCategory;
            }
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

            if(jObject.ContainsKey("TM52BuildingCategory"))
            {
                tM52BuildingCategory = Core.Query.Enum<TM52BuildingCategory>(jObject.Value<string>("TM52BuildingCategory"));
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

            result.Add("TM52BuildingCategory", tM52BuildingCategory.ToString());

            return result;
        }
    }
}