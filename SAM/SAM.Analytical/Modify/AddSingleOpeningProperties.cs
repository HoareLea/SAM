using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool AddSingleOpeningProperties(this Aperture aperture, ISingleOpeningProperties singleOpeningProperties)
        {
            if (aperture == null || singleOpeningProperties == null)
            {
                return false;
            }

            if (!aperture.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties) || openingProperties == null)
            {
                return aperture.SetValue(ApertureParameter.OpeningProperties, singleOpeningProperties);
            }

            if(openingProperties is MultipleOpeningProperties)
            {
                MultipleOpeningProperties multipleOpeningProperties = (MultipleOpeningProperties)openingProperties;

                List<ISingleOpeningProperties> singleOpeningProperties_Temp = multipleOpeningProperties.SingleOpeningProperties;
                if(singleOpeningProperties_Temp == null)
                {
                    singleOpeningProperties_Temp = new List<ISingleOpeningProperties>();
                }

                singleOpeningProperties_Temp.Add(singleOpeningProperties);

                multipleOpeningProperties = new MultipleOpeningProperties(multipleOpeningProperties, singleOpeningProperties_Temp);

                return aperture.SetValue(ApertureParameter.OpeningProperties, multipleOpeningProperties);
            }

            if(openingProperties is ISingleOpeningProperties)
            {
                List<ISingleOpeningProperties> singleOpeningProperties_Temp = new List<ISingleOpeningProperties>() { (ISingleOpeningProperties)openingProperties, singleOpeningProperties };

                MultipleOpeningProperties multipleOpeningProperties = new MultipleOpeningProperties(singleOpeningProperties_Temp);

                return aperture.SetValue(ApertureParameter.OpeningProperties, multipleOpeningProperties);
            }

            return false;
        }
    }
}