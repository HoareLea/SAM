namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CoolingDesignRelativeHumidity(this Space space, AnalyticalModel analyticalModel)
        {
            return CoolingDesignRelativeHumidity(space, analyticalModel?.ProfileLibrary);
        }

        public static double CoolingDesignRelativeHumidity(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            return CoolingDesignRelativeHumidity(space.InternalCondition, profileLibrary);
        }

        public static double CoolingDesignRelativeHumidity(this InternalCondition internalCondition, ProfileLibrary profileLibrary)
        {
            if (internalCondition == null || profileLibrary == null)
                return double.NaN;

            Profile profile = internalCondition.GetProfile(ProfileType.Humidification, profileLibrary);

            return profile == null ? double.NaN : profile.MaxValue;
        }
    }
}