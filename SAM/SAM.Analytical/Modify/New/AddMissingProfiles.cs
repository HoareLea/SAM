using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Profile> AddMissingProfiles(this BuildingModel buildingModel, ProfileLibrary profileLibrary)
        {
            return AddMissingProfiles(buildingModel, profileLibrary, out Dictionary<ProfileType, List<string>> missingProfileNames);
        }

        public static List<Profile> AddMissingProfiles(this BuildingModel buildingModel, ProfileLibrary profileLibrary, out Dictionary<ProfileType, List<string>> missingProfileNames)
        {
            missingProfileNames = null;

            if (buildingModel == null || profileLibrary == null)
            {
                return null;
            }

            List<Profile> result = new List<Profile>();

            List<Space> spaces = buildingModel.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                return result;
            }

            missingProfileNames = new Dictionary<ProfileType, List<string>>();
            foreach(Space space in spaces)
            {
                InternalCondition internalCondition = space?.InternalCondition;
                if(internalCondition == null)
                {
                    continue;
                }

                IEnumerable<ProfileType> profileTypes = internalCondition.GetProfileTypes();
                if(profileTypes == null || profileTypes.Count() == 0)
                {
                    continue;
                }

                foreach(ProfileType profileType in profileTypes)
                {
                    Profile profile = buildingModel.GetProfile(internalCondition, profileType);
                    if(profile == null)
                    {
                        if(!missingProfileNames.TryGetValue(profileType, out List<string> names))
                        {
                            names = new List<string>();
                            missingProfileNames[profileType] = names;
                        }

                        string name = internalCondition.GetProfileName(profileType);
                        if(!names.Contains(name))
                        {
                            names.Add(name);
                        }
                    }
                    else
                    {
                        result.Add(profile);
                        buildingModel.Add(profile);
                    }
                }
            }

            return result;
        }
    }
}