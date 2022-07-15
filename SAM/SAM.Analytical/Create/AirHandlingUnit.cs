namespace SAM.Analytical
{
    public static partial class Create
    {
        public static AirHandlingUnit AirHandlingUnit(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            AirHandlingUnit result = new AirHandlingUnit(name);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilFluidReturnTemperature, 12);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilFluidFlowTemperature, 6);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilOffTemperature, 24);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilOnTemperature, 21);
            result.SetValue(AirHandlingUnitParameter.SummerSupplyTemperature, 23);
            result.SetValue(AirHandlingUnitParameter.SummerHeatRecoverySensibleEfficiency, 75);
            result.SetValue(AirHandlingUnitParameter.SummerHeatRecoveryLatentEfficiency, 0);
            result.SetValue(AirHandlingUnitParameter.WinterHeatRecoverySensibleEfficiency, 75);
            result.SetValue(AirHandlingUnitParameter.WinterHeatRecoveryLatentEfficiency, 0);
            result.SetValue(AirHandlingUnitParameter.SummerHeatingCoil, false);

            return result;
        }
    }
}