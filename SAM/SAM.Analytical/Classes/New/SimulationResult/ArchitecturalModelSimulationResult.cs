using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class ArchitecturalModelSimulationResult : Result
    {
        public ArchitecturalModelSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public ArchitecturalModelSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public ArchitecturalModelSimulationResult(ArchitecturalModelSimulationResult architecturalModelSimulationResult)
            : base(architecturalModelSimulationResult)
        {

        }

        public ArchitecturalModelSimulationResult(Guid guid, ArchitecturalModelSimulationResult architecturalModelSimulationResult)
            : base(guid, architecturalModelSimulationResult)
        {

        }

        public ArchitecturalModelSimulationResult(JObject jObject)
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