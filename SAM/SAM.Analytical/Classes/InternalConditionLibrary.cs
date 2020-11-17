using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class InternalConditionLibrary : SAMLibrary
    {
        public InternalConditionLibrary(string name)
            : base(name)
        {

        }

        public InternalConditionLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public InternalConditionLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public InternalConditionLibrary(InternalConditionLibrary internalConditionLibrary)
            : base(internalConditionLibrary)
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
            InternalCondition internalCondition = jSAMObject as InternalCondition;
            if (internalCondition == null)
                return null;

            return internalCondition.Name;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is InternalCondition;
        }

        public List<InternalCondition> GetInternalConditions()
        {
            return GetObjects<InternalCondition>();
        }

        public List<InternalCondition> GetInternalConditions(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            
            List<InternalCondition> internalConditions = GetInternalConditions();
            if (internalConditions == null || internalConditions.Count == 0)
                return null;

            return internalConditions.FindAll(x => Core.Query.Compare(x.Name, text, textComparisonType, caseSensitive));
        }
    }
}