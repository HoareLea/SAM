﻿using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(InternalCondition)), Description("Internal Condition Parameter")]
    public enum InternalConditionParameter
    {
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,

        [ParameterProperties("Area Per Person", "Area Per Person [m2/p]"), DoubleParameterValue(0)] AreaPerPerson,
        [ParameterProperties("Occupancy Profile Name", "Occupancy Profile Name"), ParameterValue(Core.ParameterType.String)] OccupancyProfileName,
        [ParameterProperties("Occupancy Sensible Gain Per Person", "Occupancy Sensible Gain Per Person [W/p]"), ParameterValue(Core.ParameterType.Double)] OccupancySensibleGainPerPerson,
        [ParameterProperties("Occupancy Latent Gain Per Person", "Occupancy Latent Gain Per Person [W/p]"), ParameterValue(Core.ParameterType.Double)] OccupancyLatentGainPerPerson,
        [ParameterProperties("Equipment Sensible Profile Name", "Equipment Sensible Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentSensibleProfileName,
        [ParameterProperties("Equipment Sensible Gain", "Equipment Sensible Gain [W]"), ParameterValue(Core.ParameterType.Double)] EquipmentSensibleGain,
        [ParameterProperties("Equipment Sensible Gain Per Area", "Equipment Sensible Gain Per Area [W/m2]"), ParameterValue(Core.ParameterType.Double)] EquipmentSensibleGainPerArea,
        [ParameterProperties("Equipment Sensible Gain Per Person", "Equipment Sensible Gain Per Person [W/p]"), ParameterValue(Core.ParameterType.Double)] EquipmentSensibleGainPerPerson,
        [ParameterProperties("Equipment Latent Profile Name", "Equipment Latent Profile Name"), ParameterValue(Core.ParameterType.String)] EquipmentLatentProfileName,
        [ParameterProperties("Equipment Latent Gain", "Equipment Latent Gain [W]"), ParameterValue(Core.ParameterType.Double)] EquipmentLatentGain,
        [ParameterProperties("Equipment Latent Gain Per Area", "Equipment Latent Gain Per Area [W/m2]"), ParameterValue(Core.ParameterType.Double)] EquipmentLatentGainPerArea,
        [ParameterProperties("Lighting Gain Per Area", "Lighting Gain Per Area [W/m2]"), ParameterValue(Core.ParameterType.Double)] LightingGainPerArea,
        [ParameterProperties("Lighting Gain Per Person", "Lighting Gain Per Person [W/p]"), ParameterValue(Core.ParameterType.Double)] LightingGainPerPerson,
        [ParameterProperties("Lighting Gain", "Lighting Gain [W]"), ParameterValue(Core.ParameterType.Double)] LightingGain,
        [ParameterProperties("Lighting Level", "Lighting Level [lux]"), DoubleParameterValue(0)] LightingLevel,
        [ParameterProperties("Lighting Profile Name", "Lighting Profile Name"), ParameterValue(Core.ParameterType.String)] LightingProfileName,
        [ParameterProperties("Lighting Efficiency", "Lighting Efficiency [W/m²/100lux]"), ParameterValue(Core.ParameterType.Double)] LightingEfficiency,
        [ParameterProperties("Infiltration Air Changes Per Hour", "Infiltration Air Changes Per Hour [ACH]"), ParameterValue(Core.ParameterType.Double)] InfiltrationAirChangesPerHour,
        [ParameterProperties("Infiltration Profile Name", "Infiltration Profile Name"), ParameterValue(Core.ParameterType.String)] InfiltrationProfileName,
        [ParameterProperties("Pollutant Generation Per Person", "Pollutant Generation Per Person [g/h/p]"), ParameterValue(Core.ParameterType.Double)] PollutantGenerationPerPerson,
        [ParameterProperties("Pollutant Generation Per Area", "Pollutant Generation Per Area [g/h/m2]"), ParameterValue(Core.ParameterType.Double)] PollutantGenerationPerArea,
        [ParameterProperties("Pollutant Profile Name", "Pollutant Profile Name"), ParameterValue(Core.ParameterType.String)] PollutantProfileName,
        [ParameterProperties("Heating Emitter Radiant Proportion", "Heating Emitter Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] HeatingEmitterRadiantProportion,
        [ParameterProperties("Heating Emitter Coefficient", "Heating Emitter Coefficient [0-1]"), DoubleParameterValue(0, 1)] HeatingEmitterCoefficient,
        [ParameterProperties("Heating Profile Name", "Heating Profile Name"), ParameterValue(Core.ParameterType.String)] HeatingProfileName,
        [ParameterProperties("Cooling Emitter Radiant Proportion", "Cooling Emitter Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] CoolingEmitterRadiantProportion,
        [ParameterProperties("Cooling Emitter Coefficient", "Cooling Emitter Coefficient [0-1]"), DoubleParameterValue(0, 1)] CoolingEmitterCoefficient,
        [ParameterProperties("Cooling Profile Name", "Cooling Profile Name"), ParameterValue(Core.ParameterType.String)] CoolingProfileName,
        [ParameterProperties("Humidification Profile Name", "Humidification Profile Name"), ParameterValue(Core.ParameterType.String)] HumidificationProfileName,
        [ParameterProperties("Dehumidification Profile Name", "Dehumidification Profile Name"), ParameterValue(Core.ParameterType.String)] DehumidificationProfileName,

        [ParameterProperties("Ventilation System Type Name", "Ventilation System Type Name"), ParameterValue(Core.ParameterType.String)] VentilationSystemTypeName,
        [ParameterProperties("Cooling System Type Name", "Cooling System Type Name"), ParameterValue(Core.ParameterType.String)] CoolingSystemTypeName,
        [ParameterProperties("Heating System Type Name", "Heating System Type Name"), ParameterValue(Core.ParameterType.String)] HeatingSystemTypeName,

        [ParameterProperties("Supply Air Flow Per Person", "Supply Air Flow Per Person [m3/s/p]"), ParameterValue(Core.ParameterType.Double)] SupplyAirFlowPerPerson,
        [ParameterProperties("Exhaust Air Flow Per Person", "Exhaust Air Flow Per Person [m3/s/p]"), ParameterValue(Core.ParameterType.Double)] ExhaustAirFlowPerPerson,
        [ParameterProperties("Supply Air Changes Per Hour", "Supply Air Changes Per Hour [ACH]"), ParameterValue(Core.ParameterType.Double)] SupplyAirChangesPerHour,
        [ParameterProperties("Exhaust Air Changes Per Hour", "Exhaust Air Changes Per Hour [ACH]"), ParameterValue(Core.ParameterType.Double)] ExhaustAirChangesPerHour,
        [ParameterProperties("Supply Air Flow Per Area", "Supply Air Flow Per Area [m3/s/m2]"), ParameterValue(Core.ParameterType.Double)] SupplyAirFlowPerArea,
        [ParameterProperties("Exhaust Air Flow Per Area", "Exhaust Air Flow Per Area [m3/s/m2]"), ParameterValue(Core.ParameterType.Double)] ExhaustAirFlowPerArea,
        [ParameterProperties("Supply Air Flow", "Supply Air Flow [m3/s]"), ParameterValue(Core.ParameterType.Double)] SupplyAirFlow,
        [ParameterProperties("Exhaust Air Flow", "Exhaust Air Flow [m3/s]"), ParameterValue(Core.ParameterType.Double)] ExhaustAirFlow,
        [ParameterProperties("Ventilation Profile Name", "Ventilation Profile Name"), ParameterValue(Core.ParameterType.String)] VentilationProfileName,

        [ParameterProperties("Lighting Radiant Proportion", "Lighting Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] LightingRadiantProportion,
        [ParameterProperties("Occupancy Radiant Proportion", "Occupancy Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] OccupancyRadiantProportion,
        [ParameterProperties("Equipment Radiant Proportion", "Equipment Radiant Proportion [0-1]"), DoubleParameterValue(0, 1)] EquipmentRadiantProportion,

        [ParameterProperties("Lighting View Coefficient", "Lighting View Coefficient [0-1]"), DoubleParameterValue(0, 1)] LightingViewCoefficient,
        [ParameterProperties("Occupancy View Coefficient", "Occupancy View Coefficient [0-1]"), DoubleParameterValue(0, 1)] OccupancyViewCoefficient,
        [ParameterProperties("Equipment View Coefficient", "Equipment View Coefficient [0-1]"), DoubleParameterValue(0, 1)] EquipmentViewCoefficient,

        [ParameterProperties("Lighting Control Function", "Lighting Control Function"), ParameterValue(Core.ParameterType.String)] LightingControlFunction,

        [ParameterProperties("Ventilation Function", "Ventilation Function"), ParameterValue(Core.ParameterType.String)] VentilationFunction,
        [ParameterProperties("Ventilation Function Factor", "Ventilation Function Factor"), ParameterValue(Core.ParameterType.Double)] VentilationFunctionFactor,
        [ParameterProperties("Ventilation Function Setback", "Ventilation Function Setback"), ParameterValue(Core.ParameterType.Double)] VentilationFunctionSetback,
        [ParameterProperties("Ventilation Function Description", "Ventilation Function Description"), ParameterValue(Core.ParameterType.String)] VentilationFunctionDescription,

        [ParameterProperties("NCM Data", "National Calculation Method (NCM) Data"), SAMObjectParameterValue(typeof(NCMData))] NCMData,

        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
    }
}