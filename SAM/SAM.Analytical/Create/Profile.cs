using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Profile Profile(this ProfileLibrary profileLibrary, string name, ProfileType profileType, IEnumerable<string> names, bool includeProfileGroup = false)
        {
            if (profileLibrary == null || names == null)
                return null;

            List<Profile> profiles = new List<Profile>();
            foreach(string name_Profile in names)
            {
                Profile profile = profileLibrary.GetProfile(name, profileType, includeProfileGroup);
                if (profile == null)
                    continue;

                profiles.Add(profile);
            }

            if (profiles.Count == 0)
                return null;

            Profile result = new Profile(name, profileType);
            profiles.ForEach(x => result.Add(x));

            return result;
        }

        public static Profile Profile(this ProfileLibrary profileLibrary, string name, ProfileGroup profileGroup, IEnumerable<string> names, bool includeProfileTypes = false)
        {
            if (profileLibrary == null || names == null)
                return null;

            List<Profile> profiles = new List<Profile>();
            foreach (string name_Profile in names)
            {
                Profile profile = profileLibrary.GetProfile(name_Profile, profileGroup, includeProfileTypes);
                if (profile == null)
                    continue;

                profiles.Add(profile);
            }

            if (profiles.Count == 0)
                return null;

            Profile result = new Profile(name, profileGroup);
            profiles.ForEach(x => result.Add(x));

            return result;
        }
    
        public static Profile Profile(string name, ProfileType profileType, IEnumerable<double> values)
        {
            return Profile(name, profileType.Text(), values);
        }

        public static Profile Profile(string name, ProfileGroup profileGroup, IEnumerable<double> values)
        {
            return Profile(name, profileGroup.Text(), values);
        }

        public static Profile Profile(string name, string category, IEnumerable<double> values)
        {
            if (string.IsNullOrEmpty(name) || values == null)
                return null;

            Profile result = new Profile(name, category);

            int count = values.Count();
            int min = -1;

            for(int i=0; i < count; i++)
            {

            }

            return result;
        }
    }
}