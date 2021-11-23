using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IEnumerable<Profile> Profiles(this ArchitecturalModel architecturalModel, ProfileLibrary profileLibrary, bool includeProfileGroup = true)
        {
            if (architecturalModel == null || profileLibrary == null)
                return null;

            return Profiles(architecturalModel.GetSpaces(), profileLibrary, includeProfileGroup);
        }
    }
}