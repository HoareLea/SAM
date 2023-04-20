namespace SAM.Analytical
{
    public static partial class Query
    {
        public static InternalConditionParameter? InternalConditionParameter(this ProfileType profileType)
        {
            if (profileType == ProfileType.Undefined || profileType == ProfileType.Other)
                return null;

            switch (profileType)
            {
                case ProfileType.Cooling:
                    return Analytical.InternalConditionParameter.CoolingProfileName;

                case ProfileType.Dehumidification:
                    return Analytical.InternalConditionParameter.DehumidificationProfileName;

                case ProfileType.EquipmentLatent:
                    return Analytical.InternalConditionParameter.EquipmentLatentProfileName;

                case ProfileType.EquipmentSensible:
                    return Analytical.InternalConditionParameter.EquipmentSensibleProfileName;

                case ProfileType.Heating:
                    return Analytical.InternalConditionParameter.HeatingProfileName;

                case ProfileType.Humidification:
                    return Analytical.InternalConditionParameter.HumidificationProfileName;

                case ProfileType.Infiltration:
                    return Analytical.InternalConditionParameter.InfiltrationProfileName;

                case ProfileType.Lighting:
                    return Analytical.InternalConditionParameter.LightingProfileName;

                case ProfileType.Occupancy:
                    return Analytical.InternalConditionParameter.OccupancyProfileName;

                case ProfileType.Pollutant:
                    return Analytical.InternalConditionParameter.PollutantProfileName;

                case ProfileType.Ventilation:
                    return Analytical.InternalConditionParameter.VentilationProfileName;
            }

            return null;
        }
    }
}