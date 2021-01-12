namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetOccupancyGains(this InternalCondition internalCondition, DegreeOfActivity degreeOfActivity)
        {
            if (internalCondition == null || degreeOfActivity == null)
                return false;

            bool result = false;


            double latent = degreeOfActivity.Latent;
            if (!double.IsNaN(latent))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, latent);
                result = true;
            }
                

            double sensible = degreeOfActivity.Sensible;
            if (!double.IsNaN(sensible))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, sensible);
                result = true;
            }

            return result;
        }

        public static bool SetOccupancyGains(this Space space, DegreeOfActivity degreeOfActivity)
        {
            if (space == null || degreeOfActivity == null)
                return false;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                internalCondition = new InternalCondition(space.Name);

            if (!internalCondition.SetOccupancyGains(degreeOfActivity))
                return false;

            space.InternalCondition = internalCondition;
            return true;
        }
    }
}