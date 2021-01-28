namespace SAM.Analytical
{
    public static partial class Create
    {
        public static SpaceSimulationResult SpaceSimulationResult(
            string name, 
            string reference = null,
            double volume = double.NaN,
            double area = double.NaN,
            LoadType? loadType = null,
            double load = double.NaN,
            double loadIndex = -1,
            SizingMethod? sizingMethod = null,
            double dryBulbTemperature = double.NaN, 
            double resultantTemperature = double.NaN,
            double solarGain = double.NaN,
            double lightingGain = double.NaN,
            double infiltartionGain = double.NaN,
            double airMovementGain = double.NaN,
            double buildingHeatTransfer = double.NaN,
            double glazingExternalConduction = double.NaN,
            double opaqueExternalConduction = double.NaN,
            double occupancySensibleGain = double.NaN,
            double occupancyLatentGain = double.NaN,
            double equipmentSensibleGain = double.NaN,
            double equipmentLatentGain = double.NaN,
            double humidityRatio = double.NaN,
            double relativeHumidity = double.NaN,
            double apertureFlowIn = double.NaN,
            double apertureFlowOut = double.NaN,
            double pollutant = double.NaN)
        {
            SpaceSimulationResult result = new SpaceSimulationResult(name, reference);

            if (!double.IsNaN(volume))
                result.SetValue(SpaceSimulationResultParameter.Volume, volume);

            if (!double.IsNaN(area))
                result.SetValue(SpaceSimulationResultParameter.Area, area);

            if (sizingMethod != null && sizingMethod.HasValue)
                result.SetValue(SpaceSimulationResultParameter.SizingMethod, sizingMethod.Text());

            if (loadType != null && loadType.HasValue)
                result.SetValue(SpaceSimulationResultParameter.LoadType, loadType.Text());

            if (!double.IsNaN(dryBulbTemperature))
                result.SetValue(SpaceSimulationResultParameter.DryBulbTempearture, dryBulbTemperature);

            if (!double.IsNaN(dryBulbTemperature))
                result.SetValue(SpaceSimulationResultParameter.DryBulbTempearture, dryBulbTemperature);

            if (!double.IsNaN(resultantTemperature))
                result.SetValue(SpaceSimulationResultParameter.ResultantTemperature, resultantTemperature);

            if (!double.IsNaN(load))
                result.SetValue(SpaceSimulationResultParameter.Load, load);

            if (loadIndex != -1)
                result.SetValue(SpaceSimulationResultParameter.LoadIndex, loadIndex);

            if (!double.IsNaN(solarGain))
                result.SetValue(SpaceSimulationResultParameter.SolarGain, solarGain);

            if (!double.IsNaN(lightingGain))
                result.SetValue(SpaceSimulationResultParameter.LightingGain, lightingGain);

            if (!double.IsNaN(infiltartionGain))
                result.SetValue(SpaceSimulationResultParameter.InfiltrationGain, infiltartionGain);

            if (!double.IsNaN(airMovementGain))
                result.SetValue(SpaceSimulationResultParameter.AirMovementGain, airMovementGain);

            if (!double.IsNaN(buildingHeatTransfer))
                result.SetValue(SpaceSimulationResultParameter.BuildingHeatTransfer, buildingHeatTransfer);

            if (!double.IsNaN(glazingExternalConduction))
                result.SetValue(SpaceSimulationResultParameter.GlazingExternalConduction, glazingExternalConduction);

            if (!double.IsNaN(opaqueExternalConduction))
                result.SetValue(SpaceSimulationResultParameter.OpaqueExternalConduction, opaqueExternalConduction);

            if (!double.IsNaN(occupancySensibleGain))
                result.SetValue(SpaceSimulationResultParameter.OccupancySensibleGain, occupancySensibleGain);

            if (!double.IsNaN(occupancyLatentGain))
                result.SetValue(SpaceSimulationResultParameter.OccupancyLatentGain, occupancyLatentGain);

            if (!double.IsNaN(equipmentSensibleGain))
                result.SetValue(SpaceSimulationResultParameter.EquipmentSensibleGain, equipmentSensibleGain);

            if (!double.IsNaN(equipmentLatentGain))
                result.SetValue(SpaceSimulationResultParameter.EquipmentLatentGain, equipmentLatentGain);

            if (!double.IsNaN(humidityRatio))
                result.SetValue(SpaceSimulationResultParameter.HumidityRatio, humidityRatio);

            if (!double.IsNaN(relativeHumidity))
                result.SetValue(SpaceSimulationResultParameter.RelativeHumidity, relativeHumidity);

            if (!double.IsNaN(apertureFlowIn))
                result.SetValue(SpaceSimulationResultParameter.ApertureFlowIn, apertureFlowIn);

            if (!double.IsNaN(apertureFlowOut))
                result.SetValue(SpaceSimulationResultParameter.ApertureFlowOut, apertureFlowOut);

            if (!double.IsNaN(pollutant))
                result.SetValue(SpaceSimulationResultParameter.Pollutant, pollutant);

            return result;
        }
    }
}