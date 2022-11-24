namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Openable(this Aperture aperture)
        {
            if(aperture == null)
            {
                return false;
            }

            double dichargeCoefficient = 0;
            if (aperture.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties) && openingProperties != null)
            {
                dichargeCoefficient = openingProperties.GetDischargeCoefficient();
            }

            return dichargeCoefficient != 0;
        }
    }
}