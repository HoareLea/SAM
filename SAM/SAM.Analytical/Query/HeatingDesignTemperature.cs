namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double HeatingDesignTemperature(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            Profile profile = internalCondition?.GetProfile(ProfileType.Heating, profileLibrary);

            return profile == null ? double.NaN : profile.Min;
        }
    }
}