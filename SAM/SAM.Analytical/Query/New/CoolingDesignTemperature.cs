namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CoolingDesignTemperature(this Space space, BuildingModel buildingModel)
        {
            Profile profile = buildingModel?.GetProfile(space, ProfileType.Cooling, true);
            if(profile == null)
            {
                return double.NaN;
            }

            return profile.MinValue;
        }
    }
}