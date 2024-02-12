using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class TM59NaturalVentilationBedroomResult : TM59NaturalVentilationResult
    {
        private int annualNightOccupiedHours;
        private int summerOccupiedHours;
        private int maxExceedableSummerHours;
        private int maxExceedableNightHours;
        private int nightHoursNumberExceeding26;

        public TM59NaturalVentilationBedroomResult(
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours, 
            int maxExceedableHours,
            int hoursExceedingComfortRange,
            int annualNightOccupiedHours,
            int summerOccupiedHours,
            int maxExceedableSummerHours,
            int maxExceedableNightHours,
            int nightHoursNumberExceeding26,
            bool pass)
            : base(name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, hoursExceedingComfortRange, pass, TM59SpaceApplication.Sleeping)
        {
            this.annualNightOccupiedHours = annualNightOccupiedHours;
            this.summerOccupiedHours = summerOccupiedHours;
            this.maxExceedableNightHours = maxExceedableNightHours;
            this.nightHoursNumberExceeding26 = nightHoursNumberExceeding26;
            this.maxExceedableSummerHours = maxExceedableSummerHours;
        }

        public TM59NaturalVentilationBedroomResult(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceedingComfortRange,
            int annualNightOccupiedHours,
            int summerOccupiedHours,
            int maxExceedableSummerHours,
            int maxExceedableNightHours,
            int nightHoursNumberExceeding26,
            bool pass)
            : base(guid, name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, hoursExceedingComfortRange, pass, TM59SpaceApplication.Sleeping)
        {
            this.annualNightOccupiedHours = annualNightOccupiedHours;
            this.maxExceedableNightHours = maxExceedableNightHours;
            this.nightHoursNumberExceeding26 = nightHoursNumberExceeding26;
            this.summerOccupiedHours = summerOccupiedHours;
            this.maxExceedableSummerHours = maxExceedableSummerHours;
        }

        public int AnnualNightOccupiedHours
        {
            get
            {
                return annualNightOccupiedHours;
            }
        }

        public int MaxExceedableNightHours
        {
            get
            {
                return maxExceedableNightHours;
            }
        }

        public int NightHoursNumberExceeding26
        {
            get
            {
                return nightHoursNumberExceeding26;
            }
        }

        public int SummerOccupiedHours
        {
            get
            {
                return summerOccupiedHours;
            }
        }

        public int MaxExceedableSummerHours
        {
            get
            {
                return maxExceedableSummerHours;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("AnnualNightOccupiedHours"))
            {
                annualNightOccupiedHours = jObject.Value<int>("AnnualNightOccupiedHours");
            }

            if (jObject.ContainsKey("MaxExceedableSummerHours"))
            {
                maxExceedableSummerHours = jObject.Value<int>("MaxExceedableSummerHours");
            }

            if (jObject.ContainsKey("SummerOccupiedHours"))
            {
                summerOccupiedHours = jObject.Value<int>("SummerOccupiedHours");
            }

            if (jObject.ContainsKey("MaxExceedableNightHours"))
            {
                maxExceedableNightHours = jObject.Value<int>("MaxExceedableNightHours");
            }

            if (jObject.ContainsKey("NightHoursNumberExceeding26"))
            {
                nightHoursNumberExceeding26 = jObject.Value<int>("NightHoursNumberExceeding26");
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

            if (annualNightOccupiedHours != int.MinValue)
            {
                result.Add("AnnualNightOccupiedHours", annualNightOccupiedHours);
            }

            if (maxExceedableSummerHours != int.MinValue)
            {
                result.Add("MaxExceedableSummerHours", maxExceedableSummerHours);
            }

            if (summerOccupiedHours != int.MinValue)
            {
                result.Add("SummerOccupiedHours", summerOccupiedHours);
            }

            if (maxExceedableNightHours != int.MinValue)
            {
                result.Add("MaxExceedableNightHours", maxExceedableNightHours);
            }

            if (nightHoursNumberExceeding26 != int.MinValue)
            {
                result.Add("NightHoursNumberExceeding26", nightHoursNumberExceeding26);
            }

            return result;
        }
    }
}