namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double HeatingDesignRelativeHumidity(this Space space, AnalyticalModel analyticalModel)
        {
            return HeatingDesignRelativeHumidity(space, analyticalModel?.ProfileLibrary);
        }

        public static double HeatingDesignRelativeHumidity(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            return HeatingDesignRelativeHumidity(space.InternalCondition, profileLibrary);
        }

        public static double HeatingDesignRelativeHumidity(this InternalCondition internalCondition, ProfileLibrary profileLibrary)
        {
            if (internalCondition == null || profileLibrary == null)
                return double.NaN;

            Profile profile = internalCondition.GetProfile(ProfileType.Dehumidification, profileLibrary);

            return profile == null ? double.NaN : profile.MinValue;
        }
    }
}