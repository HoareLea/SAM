using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class AnalyticalModelSimulationResult : Result, IAnalyticalResult
    {
        public AnalyticalModelSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public AnalyticalModelSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public AnalyticalModelSimulationResult(AnalyticalModelSimulationResult analyticalModelSimulationResult)
            : base(analyticalModelSimulationResult)
        {

        }

        public AnalyticalModelSimulationResult(Guid guid, AnalyticalModelSimulationResult analyticalModelSimulationResult)
            : base(guid, analyticalModelSimulationResult)
        {

        }

        public AnalyticalModelSimulationResult(JObject jObject)
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