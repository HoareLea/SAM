using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class DegreeOfActivityLibrary : SAMLibrary
    {
        public DegreeOfActivityLibrary(string name)
            : base(name)
        {

        }

        public DegreeOfActivityLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public DegreeOfActivityLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public DegreeOfActivityLibrary(DegreeOfActivityLibrary degreeOfActivityLibrary)
            : base(degreeOfActivityLibrary)
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
            DegreeOfActivity degreeOfActivity = jSAMObject as DegreeOfActivity;
            if (degreeOfActivity == null)
                return null;

            return degreeOfActivity.Name;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is DegreeOfActivity;
        }

        public List<DegreeOfActivity> GetDegreeOfActivities()
        {
            return GetObjects<DegreeOfActivity>();
        }

        public List<DegreeOfActivity> GetDegreeOfActivities(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            
            List<DegreeOfActivity> degreeOfActivities = GetDegreeOfActivities();
            if (degreeOfActivities == null || degreeOfActivities.Count == 0)
                return null;

            return degreeOfActivities.FindAll(x => Core.Query.Compare(x.Name, text, textComparisonType, caseSensitive));
        }
    }
}