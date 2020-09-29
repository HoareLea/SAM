using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class ApertureConstructionLibrary : SAMLibrary
    {
        public ApertureConstructionLibrary(string name)
            : base(name)
        {

        }

        public ApertureConstructionLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public ApertureConstructionLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public ApertureConstructionLibrary(ApertureConstructionLibrary apertureConstructionLibrary)
            : base(apertureConstructionLibrary)
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
                return jObject;

            return jObject;
        }

        public override string GetUniqueId(IJSAMObject jSAMObject)
        {
            ApertureConstruction apertureConstruction = jSAMObject as ApertureConstruction;
            if (apertureConstruction == null)
                return null;

            return apertureConstruction.Guid.ToString();
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is ApertureConstruction;
        }

        public List<ApertureConstruction> GetApertureConstructions()
        {
            return GetObjects<ApertureConstruction>();
        }
    }
}