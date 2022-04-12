using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Profile Profile(this IEnumerable<Profile> profiles, string name, ProfileType profileType, bool includeProfileGroup = false)
        {
            if (string.IsNullOrEmpty(name) || profileType == ProfileType.Undefined || profiles == null)
                return null;

            List<Profile> profiles_Temp = profiles.ToList().FindAll(x => x.ProfileType == profileType);
            if (profiles_Temp != null)
            {
                foreach (Profile profile in profiles_Temp)
                {
                    if (name.Equals(profile.Name))
                        return profile;
                }
            }

            if (!includeProfileGroup)
                return null;

            profiles_Temp = profiles.ToList().FindAll(x => x.ProfileGroup == profileType.ProfileGroup());
            if (profiles_Temp == null || profiles_Temp.Count == 0)
                return null;

            foreach (Profile profile in profiles)
            {
                if (name.Equals(profile.Name))
                    return profile;
            }

            return null;
        }
    }
}