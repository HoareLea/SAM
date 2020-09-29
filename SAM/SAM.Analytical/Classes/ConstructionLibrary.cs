using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<Construction> GetConstructions()
        {
            return GetObjects<Construction>();
        }

        public List<Construction> GetConstructions(PanelType panelType)
        {
            List<Construction> constructions = GetConstructions();
            if (constructions == null || constructions.Count == 0)
                return null;

            return constructions.FindAll(x => x.PanelType() == panelType);
        }

        public List<Construction> GetConstructions(IEnumerable<PanelType> panelTypes)
        {
            if (panelTypes == null || panelTypes.Count() == 0)
                return null;
            
            List<Construction> constructions = GetConstructions();
            if (constructions == null || constructions.Count == 0)
                return null;

            return constructions.FindAll(x => panelTypes.Contains(x.PanelType()));
        }
    }
}