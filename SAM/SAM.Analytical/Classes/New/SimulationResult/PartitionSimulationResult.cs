using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class PartitionSimulationResult : Result, IAnalyticalObject
    {
        public PartitionSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public PartitionSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public PartitionSimulationResult(PartitionSimulationResult partitionSimulationResult)
            : base(partitionSimulationResult)
        {

        }

        public PartitionSimulationResult(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            return jObject;
        }
    }
}