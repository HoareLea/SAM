namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double HeatingDesignHumidity(this Space space, AnalyticalModel analyticalModel)
        {
            return HeatingDesignHumidity(space, analyticalModel?.ProfileLibrary);
        }

        public static double HeatingDesignHumidity(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            return HeatingDesignHumidity(space.InternalCondition, profileLibrary);
        }

        public static double HeatingDesignHumidity(this InternalCondition internalCondition, ProfileLibrary profileLibrary)
        {
            if (internalCondition == null || profileLibrary == null)
                return double.NaN;

            Profile profile = internalCondition.GetProfile(ProfileType.Dehumidification, profileLibrary);

            return profile == null ? double.NaN : profile.MinValue;
        }
    }
}