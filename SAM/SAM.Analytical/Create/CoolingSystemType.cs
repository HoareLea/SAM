namespace SAM.Analytical
{
    public static partial class Create
    {
        public static CoolingSystemType CoolingSystemType(System.Guid guid, string name, string description, double radiantProportion = double.NaN, double viewCoefficient = double.NaN, double supplyCircuitTemperature = double.NaN, double returnCircuitTemperature = double.NaN, double temperatureDifference = double.NaN, double supplyTemperature = double.NaN)
        {
            CoolingSystemType result = new CoolingSystemType(guid, name, description);

            if (!double.IsNaN(radiantProportion))
                result.SetValue(CoolingSystemTypeParameter.RadiantProportion, radiantProportion);

            if (!double.IsNaN(viewCoefficient))
                result.SetValue(CoolingSystemTypeParameter.ViewCoefficient, viewCoefficient);

            if (!double.IsNaN(supplyCircuitTemperature))
                result.SetValue(CoolingSystemTypeParameter.SupplyCircuitTemperature, supplyCircuitTemperature);

            if (!double.IsNaN(returnCircuitTemperature))
                result.SetValue(CoolingSystemTypeParameter.ReturnCircuitTemperature, returnCircuitTemperature);

            if (!double.IsNaN(temperatureDifference))
                result.SetValue(CoolingSystemTypeParameter.TemperatureDifference, temperatureDifference);

            if (!double.IsNaN(supplyTemperature))
                result.SetValue(CoolingSystemTypeParameter.SupplyTemperature, supplyTemperature);

            return result;
        }
    }
}