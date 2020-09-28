using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class ConstructionLibrary : SAMLibrary
    {
        public ConstructionLibrary(string name)
            : base(name)
        {

        }

        public ConstructionLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public ConstructionLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public ConstructionLibrary(ConstructionLibrary constructionLibrary)
            : base(constructionLibrary)
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
            Construction construction = jSAMObject as Construction;
            if (construction == null)
                return null;

            string uniqueName = construction.UniqueName();
            if (string.IsNullOrWhiteSpace(uniqueName))
                uniqueName = construction.Name;

            if (string.IsNullOrWhiteSpace(uniqueName))
                uniqueName = construction.Guid.ToString();

            return uniqueName;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is Construction;
        }

        public List<Construction> GetConstructions
        {

            get
            {
                return GetObjects<Construction>();
            }
        }
    }
}