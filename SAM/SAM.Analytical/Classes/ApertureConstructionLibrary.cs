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

        public List<ApertureConstruction> GetApertureConstructions(ApertureType apertureType)
        {
            List<ApertureConstruction> apertureConstructions = GetObjects<ApertureConstruction>();
            if (apertureConstructions == null)
                return null;

            return apertureConstructions.FindAll(x => x.ApertureType.Equals(apertureType));
        }

        public List<ApertureConstruction> GetApertureConstructions(ApertureType apertureType, PanelType panelType)
        {
            List<ApertureConstruction> apertureConstructions = GetApertureConstructions(apertureType);
            if (apertureConstructions == null)
                return null;

            return apertureConstructions.FindAll(x => x.PanelType() == panelType);
        }

        public List<ApertureConstruction> GetApertureConstructions(ApertureType apertureType, PanelGroup panelGroup)
        {
            List<ApertureConstruction> apertureConstructions = GetApertureConstructions(apertureType);
            if (apertureConstructions == null)
                return null;

            return apertureConstructions.FindAll(x => x.PanelType().PanelGroup() == panelGroup);
        }

        public List<ApertureConstruction> GetApertureConstructions(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true, ApertureType apertureType = ApertureType.Undefined)
        {
            if (text == null)
                return null;

            List<ApertureConstruction> apertureConstructions = null;
            if (apertureType == ApertureType.Undefined)
                apertureConstructions = GetApertureConstructions();
            else
                apertureConstructions = GetApertureConstructions(apertureType);

            if (apertureConstructions == null)
                return null;

            List<ApertureConstruction> result = new List<ApertureConstruction>();
            foreach(ApertureConstruction apertureConstruction in apertureConstructions)
            {
                if (apertureConstruction == null)
                    continue;

                if (Core.Query.Compare(apertureConstruction.Name, text, textComparisonType, caseSensitive))
                    result.Add(apertureConstruction);
            }

            return result;
        }
    }
}