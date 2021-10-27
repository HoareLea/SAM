namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double HeatingDesignTemperature(this Space space, ArchitecturalModel architecturalModel)
        {
            Profile profile = architecturalModel?.GetProfile(space, ProfileType.Heating, true);
            if (profile == null)
            {
                return double.NaN;
            }

            return profile.MaxValue;
        }
    }
}