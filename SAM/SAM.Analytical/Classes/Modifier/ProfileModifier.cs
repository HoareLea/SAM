using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class ProfileModifier : IndexedSimpleModifier
    {
        private Profile Profile { get; set; }
        public double Setback { get; set; }

        public ProfileModifier(ArithmeticOperator arithmeticOperator, Profile profile, double setback)
        {
            ArithmeticOperator = arithmeticOperator;
            Profile = Core.Query.Clone(profile); 
            Setback = setback;
        }

        public ProfileModifier(ProfileModifier scheduleModifier)
            : base(scheduleModifier)
        {
            if(scheduleModifier != null)
            {
                Profile = Core.Query.Clone(scheduleModifier.Profile);
                Setback = scheduleModifier.Setback;
            }
        }

        public ProfileModifier(JObject jObject)
            :base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("Profile"))
            {
                Profile = Core.Query.IJSAMObject<Profile>(jObject.Value<JObject>("Profile"));
            }

            if(jObject.ContainsKey("Setback"))
            {
                Setback = jObject.Value<double>("Setback");
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            if(Profile != null)
            {
                result.Add("Profile", Profile.ToJObject());
            }

            if(!double.IsNaN(Setback))
            {
                result.Add("Setback", Setback);
            }

            return result;
        }

        public override bool ContainsIndex(int index)
        {
            if(Profile == null)
            {
                return false;
            }

            return Profile.TryGetValue(index, out Profile profile, out double value);
        }

        public override double GetCalculatedValue(int index, double value)
        {
            if (!Profile.TryGetValue(index, out Profile profile, out double value_Temp))
            {
                return double.NaN;
            }

            if(value == 1)
            {
                return value;
            }

            if(value == 0)
            {
                return Setback;
            }

            return Setback * value;
        }
    }
}