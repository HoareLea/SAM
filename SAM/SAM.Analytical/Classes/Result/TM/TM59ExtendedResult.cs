using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public abstract class TM59ExtendedResult : TMExtendedResult
    {
        private HashSet<TM59SpaceApplication> tM59SpaceApplications;

        public TM59ExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures, params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, tM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {
            this.tM59SpaceApplications = tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
        }

        public TM59ExtendedResult(TM59ExtendedResult tM59SpaceExtendedResult)
            : base(tM59SpaceExtendedResult)
        {
            if (tM59SpaceExtendedResult != null)
            {
                tM59SpaceApplications = tM59SpaceExtendedResult.tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceExtendedResult.tM59SpaceApplications);
            }
        }

        public TM59ExtendedResult(TM59ExtendedResult tM59SpaceExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(tM59SpaceExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {
            if (tM59SpaceExtendedResult != null)
            {
                tM59SpaceApplications = tM59SpaceExtendedResult.tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
            }
        }

        public TM59ExtendedResult(JObject jObject)
            : base(jObject)
        {
        }

        public HashSet<TM59SpaceApplication> TM59SpaceApplications
        {
            get
            {
                return tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("TM59SpaceApplications"))
            {
                JArray jArray = jObject.Value<JArray>("TM59SpaceApplications");
                if (jArray != null)
                {
                    tM59SpaceApplications = new HashSet<TM59SpaceApplication>();
                    foreach (string text in jArray)
                    {
                        TM59SpaceApplication tM59SpaceApplication = Core.Query.Enum<TM59SpaceApplication>(text);
                        tM59SpaceApplications.Add(tM59SpaceApplication);
                    }
                }
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return null;
            }

            if(tM59SpaceApplications != null)
            {
                JArray jArray = new JArray();
                foreach(TM59SpaceApplication tM59SpaceApplication in tM59SpaceApplications)
                {
                    jArray.Add(tM59SpaceApplication.ToString());
                }

                result.Add("TM59SpaceApplications", jArray);
            }

            return result;
        }
    }
}