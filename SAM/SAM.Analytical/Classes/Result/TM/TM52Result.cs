using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class TM52Result : TMResult
    {
        private int occupiedHours;
        private int maxExceedableHours;

        private int hoursExceedingComfortRange;

        private double peakDailyWeightedExceedance;
        private int hoursExceedingAbsoluteLimit;
        private bool pass;

        public TM52Result(
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours, 
            int maxExceedableHours,
            int hoursExceedingComfortRange,
            double peakDailyWeightedExceedance,
            int hoursExceedingAbsoluteLimit,
            bool pass)
            : base(name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHours = occupiedHours;
            this.maxExceedableHours = maxExceedableHours;
            this.hoursExceedingComfortRange = hoursExceedingComfortRange;
            this.peakDailyWeightedExceedance = peakDailyWeightedExceedance;
            this.hoursExceedingAbsoluteLimit = hoursExceedingAbsoluteLimit;
            this.pass = pass;
        }

        public TM52Result(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceedingComfortRange,
            double peakDailyWeightedExceedance,
            int hoursExceedingAbsoluteLimit,
            bool pass)
            : base(guid, name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHours = occupiedHours;
            this.maxExceedableHours = maxExceedableHours;
            this.hoursExceedingComfortRange = hoursExceedingComfortRange;
            this.peakDailyWeightedExceedance = peakDailyWeightedExceedance;
            this.hoursExceedingAbsoluteLimit = hoursExceedingAbsoluteLimit;
            this.pass = pass;
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

        public int HoursExceedingComfortRange
        {
            get
            {
                return hoursExceedingComfortRange;
            }
        }

        public double PeakDailyWeightedExceedance
        {
            get
            {
                return peakDailyWeightedExceedance;
            }
        }

        public int HoursExceedeingAbsoluteLimit
        {
            get
            {
                return hoursExceedingAbsoluteLimit;
            }
        }

        public override bool Pass
        {
            get
            {
                return pass;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("OccupiedHours"))
            {
                occupiedHours = jObject.Value<int>("OccupiedHours");
            }

            if (jObject.ContainsKey("MaxExceedableHours"))
            {
                maxExceedableHours = jObject.Value<int>("MaxExceedableHours");
            }

            if (jObject.ContainsKey("HoursExceedingComfortRange"))
            {
                hoursExceedingComfortRange = jObject.Value<int>("HoursExceedingComfortRange");
            }

            if (jObject.ContainsKey("PeakDailyWeightedExceedance"))
            {
                peakDailyWeightedExceedance = jObject.Value<double>("PeakDailyWeightedExceedance");
            }

            if (jObject.ContainsKey("HoursExceedeingAbsoluteLimit"))
            {
                hoursExceedingAbsoluteLimit = jObject.Value<int>("HoursExceedeingAbsoluteLimit");
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

            if(occupiedHours != int.MinValue)
            {
                result.Add("OccupiedHours", occupiedHours);
            }

            if (maxExceedableHours != int.MinValue)
            {
                result.Add("MaxExceedableHours", maxExceedableHours);
            }

            if (hoursExceedingComfortRange != int.MinValue)
            {
                result.Add("HoursExceedingComfortRange", hoursExceedingComfortRange);
            }

            if (!double.IsNaN(peakDailyWeightedExceedance))
            {
                result.Add("PeakDailyWeightedExceedance", peakDailyWeightedExceedance);
            }

            if (hoursExceedingAbsoluteLimit != int.MinValue)
            {
                result.Add("HoursExceedingAbsoluteLimit", hoursExceedingAbsoluteLimit);
            }

            result.Add("Pass", pass);

            return result;
        }
    }
}