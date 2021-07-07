using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IEnumerable<Profile> Profiles(this IEnumerable<InternalCondition> internalConditions, ProfileLibrary profileLibrary, bool includeProfileGroup = true)
        {
            if (internalConditions == null || profileLibrary == null)
                return null;

            Dictionary<Guid, Profile> dictionary = new Dictionary<Guid, Profile>();
            foreach (InternalCondition internalCondition in internalConditions)
            {
                if (internalCondition == null)
                    continue;

                IEnumerable<Profile> profiles = internalCondition.GetProfiles(profileLibrary, includeProfileGroup);
                if (profiles != null)
                    continue;

                foreach (Profile profile in profiles)
                {
                    if (profile == null)
                        continue;

                    dictionary[profile.Guid] = profile;
                }
            }

            return dictionary.Values;
        }

        public static IEnumerable<Profile> Profiles(this IEnumerable<Space> spaces, ProfileLibrary profileLibrary, bool includeProfileGroup = true)
        {
            if (spaces == null || profileLibrary == null)
                return null;

            Dictionary<Guid, Profile> dictionary = new Dictionary<Guid, Profile>();
            foreach(Space space in spaces)
            {
                InternalCondition internalCondition = space?.InternalCondition;
                if (internalCondition == null)
                    continue;

                IEnumerable<Profile> profiles = internalCondition.GetProfiles(profileLibrary, includeProfileGroup);
                if (profiles == null)
                    continue;

                foreach(Profile profile in profiles)
                {
                    if (profile == null)
                        continue;

                    dictionary[profile.Guid] = profile;
                }
            }

            return dictionary.Values;
        }

        public static IEnumerable<Profile> Profiles(this AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary, bool includeProfileGroup = true)
        {
            if (adjacencyCluster == null || profileLibrary == null)
                return null;

            return Profiles(adjacencyCluster.GetSpaces(), profileLibrary, includeProfileGroup);
        }
    }
}