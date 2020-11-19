using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ProfileLibrary : SAMLibrary
    {
        public ProfileLibrary(string name)
            : base(name)
        {

        }

        public ProfileLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public ProfileLibrary(JObject jObject)
            : base(jObject)
        {

        }

        public ProfileLibrary(ProfileLibrary profileLibrary)
            : base(profileLibrary)
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
            Profile profile = jSAMObject as Profile;
            if (profile == null)
                return null;

            return string.Format("{0}::{1}", profile.ProfileType.ToString(), profile.Name);
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!base.IsValid(jSAMObject))
                return false;

            return jSAMObject is Profile;
        }

        public List<Profile> GetProfiles()
        {
            return GetObjects<Profile>();
        }

        public List<Profile> GetProfiles(ProfileType[] profileTypes)
        {
            if (profileTypes == null)
                return null;
            
            return GetObjects<Profile>()?.FindAll(x => profileTypes.Contains(x.ProfileType));
        }

        public List<Profile> GetProfiles(string text, TextComparisonType textComparisonType = TextComparisonType.Equals, bool caseSensitive = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            
            List<Profile> profiles = GetProfiles();
            if (profiles == null || profiles.Count == 0)
                return null;

            return profiles.FindAll(x => Core.Query.Compare(x.Name, text, textComparisonType, caseSensitive));
        }
    }
}