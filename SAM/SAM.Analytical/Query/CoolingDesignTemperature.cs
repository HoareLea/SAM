namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CoolingDesignTemperature(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            Profile profile = internalCondition?.GetProfile(ProfileType.Cooling, profileLibrary);

            return profile == null ? double.NaN : profile.MaxValue;
        }
    }
}