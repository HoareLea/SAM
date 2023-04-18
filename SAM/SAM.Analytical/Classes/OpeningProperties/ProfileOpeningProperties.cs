using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Scheduled Opening Properties
    /// </summary>
    public class ProfileOpeningProperties : OpeningProperties
    {
        private Profile profile;

        public ProfileOpeningProperties()
        {

        }

        public ProfileOpeningProperties(double dischargeCoefficient)
            :base(dischargeCoefficient)
        {

        }

        public ProfileOpeningProperties(JObject jObject)
            :base(jObject)
        {
        }

        public ProfileOpeningProperties(double dischargeCoefficient, Profile profile)
            : base(dischargeCoefficient)
        {
            this.profile = profile == null ? null : new Profile(profile); 
        }

        public ProfileOpeningProperties(ProfileOpeningProperties profileOpeningProperties)
            : base(profileOpeningProperties)
        {

        }

        public ProfileOpeningProperties(IOpeningProperties openingProperties, double dischargeCoefficient)
            : base(openingProperties, dischargeCoefficient)
        {

        }

        public Profile Profile
        {
            get
            {
                return profile == null ? null : new Profile(profile);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Profile"))
            {
                profile = Core.Query.IJSAMObject<Profile>(jObject.Value<JObject>("Profile"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(profile != null)
            {
                jObject.Add("Profile", profile.ToJObject());
            }

            return jObject;
        }
    }
}