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

            AirHandlingUnit result = new AirHandlingUnit(name, 23, double.NaN);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilFluidReturnTemperature, 12);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilFluidFlowTemperature, 6);
            result.SetValue(AirHandlingUnitParameter.CoolingCoilContactFactor, 0.9);
            result.SetValue(AirHandlingUnitParameter.SummerSupplyTemperature, 23);
            result.SetValue(AirHandlingUnitParameter.SummerHeatRecoverySensibleEfficiency, 75);
            result.SetValue(AirHandlingUnitParameter.SummerHeatRecoveryLatentEfficiency, 0);
            result.SetValue(AirHandlingUnitParameter.WinterHeatRecoverySensibleEfficiency, 75);
            result.SetValue(AirHandlingUnitParameter.WinterHeatRecoveryLatentEfficiency, 0);
            result.SetValue(AirHandlingUnitParameter.SummerHeatingCoil, false);

            HeatRecoveryUnit heatRecoveryUnit = new HeatRecoveryUnit("Heat Recovery Unit", 75, 0, 75, 0, double.NaN, double.NaN, double.NaN, double.NaN);

            Filter filter_Intake = new Filter("Intake Filter");
            Fan fan_Exhaust = new Fan("Exhaust Fan");
            Fan fan_Supply = new Fan("Supply Fan");
            CoolingCoil coolingCoil = new CoolingCoil("Cooling Coil", 6, 12, 0.9, double.NaN);

            result.AddSimpleEquipments(FlowClassification.Intake, filter_Intake, heatRecoveryUnit);
            result.AddSimpleEquipments(FlowClassification.Exhaust, heatRecoveryUnit, fan_Exhaust);
            result.AddSimpleEquipments(FlowClassification.Supply, heatRecoveryUnit, fan_Supply, coolingCoil);
            
            return result;
        }
    }
}