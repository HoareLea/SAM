using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ProfileLibrary : SAMLibrary<Profile>, IAnalyticalObject
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

        public ProfileLibrary(string name, IEnumerable<Profile> profiles)
            : base(name)
        {
            if(profiles != null)
            {
                foreach (Profile profile in profiles)
                    Add(profile);
            }
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

        public override string GetUniqueId(Profile profile)
        {
            if (profile == null)
                return null;

            string name = profile.Name;
            string category = profile.Category;

            if (name == null && category == null)
                return null;

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(name))
                return string.Empty;
            
            if (string.IsNullOrEmpty(category))
                return name;

            if (string.IsNullOrEmpty(name))
                return category;

            return string.Format("{0}::{1}", category, name);
        }

        public override bool IsValid(Profile profile)
        {
            if (!base.IsValid(profile))
                return false;

            return true;
        }

        public List<Profile> GetProfiles()
        {
            return GetObjects<Profile>();
        }

        public Profile GetProfile(string name, ProfileGroup profileGroup, bool includeProfileType = false)
        {
            if (string.IsNullOrEmpty(name) || profileGroup == ProfileGroup.Undefined)
                return null;

            List<Profile> profiles = GetProfiles(profileGroup, false);
            if (profiles != null)
            {
                foreach (Profile profile in profiles)
                {
                    if (name.Equals(profile.Name))
                        return profile;
                }
            }

            if (!includeProfileType)
                return null;

            profiles = GetProfiles(profileGroup, true);
            if (profiles == null || profiles.Count == 0)
                return null;

            foreach (Profile profile in profiles)
            {
                if (name.Equals(profile.Name))
                    return profile;
            }

            return null;
        }

        public Profile GetProfile(string name, ProfileType profileType, bool includeProfileGroup = false)
        {
            if (string.IsNullOrEmpty(name) || profileType == ProfileType.Undefined)
                return null;

            List<Profile> profiles = GetProfiles(profileType);
            if(profiles != null)
            {
                foreach(Profile profile in profiles)
                {
                    if (name.Equals(profile.Name))
                        return profile;
                }
            }

            if (!includeProfileGroup)
                return null;

            profiles = GetProfiles(profileType.ProfileGroup());
            if (profiles == null || profiles.Count == 0)
                return null;

            foreach(Profile profile in profiles)
            {
                if (name.Equals(profile.Name))
                    return profile;
            }

            return null;
        }
        
        public List<Profile> GetProfiles(params ProfileType[] profileTypes)
        {
            if (profileTypes == null)
                return null;
            
            return GetObjects<Profile>()?.FindAll(x => profileTypes.Contains(x.ProfileType));
        }

        public List<Profile> GetProfiles(ProfileGroup profileGroup, bool includeProfileTypes = false)
        {
            if (profileGroup == ProfileGroup.Undefined)
                return null;

            List<Profile> profiles = GetObjects<Profile>();
            if (profiles == null)
                return null;

            if (!includeProfileTypes)
                return profiles.FindAll(x => x.ProfileGroup.Equals(profileGroup));

            return profiles.FindAll(x => x.ProfileGroup.Equals(profileGroup) || Query.ProfileGroup(x.ProfileType) == profileGroup);
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