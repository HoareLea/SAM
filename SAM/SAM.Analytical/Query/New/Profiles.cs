using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IEnumerable<Profile> Profiles(this BuildingModel buildingModel, ProfileLibrary profileLibrary, bool includeProfileGroup = true)
        {
            if (buildingModel == null || profileLibrary == null)
                return null;

            return Profiles(buildingModel.GetSpaces(), profileLibrary, includeProfileGroup);
        }
    }
}