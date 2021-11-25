namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double HeatingDesignTemperature(this Space space, BuildingModel buildingModel)
        {
            Profile profile = buildingModel?.GetProfile(space, ProfileType.Heating, true);
            if (profile == null)
            {
                return double.NaN;
            }

            return profile.MaxValue;
        }
    }
}