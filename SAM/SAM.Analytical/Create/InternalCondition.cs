﻿namespace SAM.Analytical
{
    public static partial class Create
    {
        public static InternalCondition InternalCondition(
            string name, 
            double areaPerPerson,
            string occupancyProfileName,
            double equipmentSensibleGain,
            string equipmentSensibleProfileName,
            double equipmentLatentGain,
            string equipmentLatentProfileName,
            double lightingGain,
            string lightingProfileName,
            double lightingLevel,
            double lightingEfficiency,
            double infiltrationAirChangesPerHour,
            string infiltrationProfileName,
            double pollutantGenerationPerArea,
            double pollutantGenerationPerPerson,
            string pollutantProfileName,
            string heatingProfileName,
            string coolingProfileName,
            string humidificationProfileName,
            string dehumidificationProfileName
            )
        {
            InternalCondition result = new InternalCondition(name);

            //Occupancy
            result.SetValue(InternalConditionParameter.AreaPerPerson, areaPerPerson);
            result.SetValue(InternalConditionParameter.OccupancyProfileName, occupancyProfileName);

            //Equipment
            result.SetValue(InternalConditionParameter.EquipmentSensibleGain, equipmentSensibleGain);
            result.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, equipmentSensibleProfileName);
            result.SetValue(InternalConditionParameter.EquipmentLatentGain, equipmentLatentGain);
            result.SetValue(InternalConditionParameter.EquipmentLatentProfileName, equipmentLatentProfileName);

            //Lighting
            result.SetValue(InternalConditionParameter.LightingGain, lightingGain);
            result.SetValue(InternalConditionParameter.LightingProfileName, lightingProfileName);
            result.SetValue(InternalConditionParameter.LightingLevel, lightingLevel);
            result.SetValue(InternalConditionParameter.LightingEfficiency, lightingEfficiency);

            //Infiltration
            result.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, infiltrationAirChangesPerHour);
            result.SetValue(InternalConditionParameter.InfiltrationProfileName, infiltrationProfileName);

            //Pollutant
            result.SetValue(InternalConditionParameter.PollutantGenerationPerArea, pollutantGenerationPerArea);
            result.SetValue(InternalConditionParameter.PollutantGenerationPerPerson, pollutantGenerationPerPerson);
            result.SetValue(InternalConditionParameter.PollutantProfileName, pollutantProfileName);

            //Heating
            result.SetValue(InternalConditionParameter.HeatingProfileName, heatingProfileName);

            //Cooling
            result.SetValue(InternalConditionParameter.CoolingProfileName, coolingProfileName);

            //Humidification
            result.SetValue(InternalConditionParameter.HumidificationProfileName, humidificationProfileName);

            //Dehumidification
            result.SetValue(InternalConditionParameter.DehumidificationProfileName, dehumidificationProfileName);

            return result;
        }

        public static InternalCondition InternalCondition(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            InternalCondition result = new InternalCondition(name);
            result.SetValue(InternalConditionParameter.LightingRadiantProportion, 0.3);
            result.SetValue(InternalConditionParameter.OccupancyRadiantProportion, 0.2);
            result.SetValue(InternalConditionParameter.EquipmentRadiantProportion, 0.1);

            result.SetValue(InternalConditionParameter.LightingViewCoefficient, 0.49);
            result.SetValue(InternalConditionParameter.OccupancyViewCoefficient, 0.227);
            result.SetValue(InternalConditionParameter.OccupancyViewCoefficient, 0.372);

            result.SetValue(InternalConditionParameter.LightingLevel, 0);

            return result;
        }
    }
}