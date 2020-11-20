namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ProfileGroup ProfileGroup(this ProfileType profileType)
        {
            if (profileType == ProfileType.Undefined || profileType == ProfileType.Other)
                return Analytical.ProfileGroup.Undefined;

            switch(profileType)
            {
                case ProfileType.Dehumidification:
                case ProfileType.Humidification:
                    return Analytical.ProfileGroup.Humidistat;

                case ProfileType.EquipmentLatent:
                case ProfileType.EquipmentSensible:
                case ProfileType.Infiltration:
                case ProfileType.Lighting:
                case ProfileType.Occupancy:
                case ProfileType.Pollutant:
                    return Analytical.ProfileGroup.Gain;

                case ProfileType.Heating:
                case ProfileType.Cooling:
                    return Analytical.ProfileGroup.Thermostat;

            }

            return Analytical.ProfileGroup.Undefined;
        }
    }
}