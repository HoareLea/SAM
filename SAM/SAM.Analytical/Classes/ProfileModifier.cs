using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class ProfileModifier : IndexedSimpleModifier
    {
        public Profile Profile { get; set; }

        public ProfileModifier(ArithmeticOperator arithmeticOperator, Profile profile)
        {
            ArithmeticOperator = arithmeticOperator;
            Profile = profile == null ? null : new Profile(profile);
        }

        public ProfileModifier(ProfileModifier profileModifier)
            : base(profileModifier)
        {
            if(profileModifier != null)
            {
                Profile = profileModifier?.Profile == null ? null : new Profile(profileModifier.Profile);
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

            return result;
        }

        public override bool ContainsIndex(int index)
        {
            if(Profile == null)
            {
                return false;
            }

            if (!Profile.TryGetValue(index, out Profile profile_Temp, out double value))
            {
                return false;
            }

            return true;
        }

        public override double GetCalculatedValue(int index, double value)
        {
            if (Profile == null)
            {
                return value;
            }

            if (!Profile.TryGetValue(index, out Profile profile_Temp, out double value_Temp))
            {
                return value;
            }

            if(double.IsNaN(value_Temp))
            {
                return double.NaN;
            }

            return Core.Query.Calculate(ArithmeticOperator, value, value_Temp);
        }
    }
}