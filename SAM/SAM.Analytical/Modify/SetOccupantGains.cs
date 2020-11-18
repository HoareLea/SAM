namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetOccupantGains(this InternalCondition internalCondition, DegreeOfActivity degreeOfActivity)
        {
            if (internalCondition == null || degreeOfActivity == null)
                return false;

            bool result = false;


            double latent = degreeOfActivity.Latent;
            if (!double.IsNaN(latent))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupantLatentGain, latent);
                result = true;
            }
                

            double sensible = degreeOfActivity.Sensible;
            if (!double.IsNaN(sensible))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupantSensibleGain, sensible);
                result = true;
            }

            return result;
        }

        public static bool SetOccupantGains(this Space space, DegreeOfActivity degreeOfActivity)
        {
            if (space == null || degreeOfActivity == null)
                return false;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                internalCondition = new InternalCondition(space.Name);

            if (!internalCondition.SetOccupantGains(degreeOfActivity))
                return false;

            space.InternalCondition = internalCondition;
            return true;
        }
    }
}