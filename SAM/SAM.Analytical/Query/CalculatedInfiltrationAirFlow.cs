namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedInfiltrationAirFlow(this Space space)
        {
            if (space == null)
                return double.NaN;

            double volume = double.NaN;
            space.TryGetValue(SpaceParameter.Volume, out volume);
            if (double.IsNaN(volume))
                return double.NaN;

            if (volume == 0)
                return 0;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double airChangesPerHour = double.NaN;
            internalCondition.TryGetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, out airChangesPerHour);
            if (double.IsNaN(airChangesPerHour))
                return double.NaN;

            if (airChangesPerHour == 0)
                return 0;

            return volume / airChangesPerHour;
        }
    }
}