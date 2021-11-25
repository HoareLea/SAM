using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class BuildingModelSimulationResult : Result, IAnalyticalObject
    {
        public BuildingModelSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public BuildingModelSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public BuildingModelSimulationResult(BuildingModelSimulationResult buildingModelSimulationResult)
            : base(buildingModelSimulationResult)
        {

        }

        public BuildingModelSimulationResult(Guid guid, BuildingModelSimulationResult buildingModelSimulationResult)
            : base(guid, buildingModelSimulationResult)
        {

        }

        public BuildingModelSimulationResult(JObject jObject)
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