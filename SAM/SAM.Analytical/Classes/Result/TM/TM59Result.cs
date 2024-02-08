using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TM59Result : TMResult
    {
        private HashSet<TM59SpaceApplication> tM59SpaceApplications;
        
        private int occupiedHours;
        private int maxExceedableHours;

        private bool pass;

        public TM59Result(
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours, 
            int maxExceedableHours,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHours = occupiedHours;
            this.maxExceedableHours = maxExceedableHours;
            this.pass = pass;
            this.tM59SpaceApplications = tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication> (tM59SpaceApplications);
        }

        public TM59Result(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(guid, name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHours = occupiedHours;
            this.maxExceedableHours = maxExceedableHours;
            this.pass = pass;
            this.tM59SpaceApplications = tM59SpaceApplications == null ? null : new HashSet<TM59SpaceApplication>(tM59SpaceApplications);
        }

        public override int OccupiedHours
        {
            get
            {
                return occupiedHours;
            }
        }

        public override int MaxExceedableHours
        {
            get
            {
                return maxExceedableHours;
            }
        }

        public override bool Pass
        {
            get
            {
                return pass;
            }
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

            if (jObject.ContainsKey("OccupiedHours"))
            {
                occupiedHours = jObject.Value<int>("OccupiedHours");
            }

            if (jObject.ContainsKey("MaxExceedableHours"))
            {
                maxExceedableHours = jObject.Value<int>("MaxExceedableHours");
            }

            if (jObject.ContainsKey("Pass"))
            {
                pass = jObject.Value<bool>("Pass");
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

            if (tM59SpaceApplications != null)
            {
                JArray jArray = new JArray();
                foreach (TM59SpaceApplication tM59SpaceApplication in tM59SpaceApplications)
                {
                    jArray.Add(tM59SpaceApplication.ToString());
                }

                result.Add("TM59SpaceApplications", jArray);
            }

            if (occupiedHours != int.MinValue)
            {
                result.Add("OccupiedHours", occupiedHours);
            }

            if (maxExceedableHours != int.MinValue)
            {
                result.Add("MaxExceedableHours", maxExceedableHours);
            }

            result.Add("Pass", pass);

            return result;
        }
    }
}