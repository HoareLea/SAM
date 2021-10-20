namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CoolingDesignTemperature(this Space space, ArchitecturalModel architecturalModel)
        {
            Profile profile = architecturalModel?.GetProfile(space, ProfileType.Cooling, true);
            if(profile == null)
            {
                return double.NaN;
            }

            return profile.MinValue;
        }
    }
}