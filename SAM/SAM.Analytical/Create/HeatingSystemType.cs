namespace SAM.Analytical
{
    public static partial class Create
    {
        public static HeatingSystemType HeatingSystemType(System.Guid guid, string name, string description, double radiantProportion = double.NaN, double viewCoefficient = double.NaN, double supplyCircuitTemperature = double.NaN, double returnCircuitTemperature = double.NaN)
        {
            HeatingSystemType result = new HeatingSystemType(guid, name, description);

            if (!double.IsNaN(radiantProportion))
                result.SetValue(HeatingSystemTypeParameter.RadiantProportion, radiantProportion);

            if (!double.IsNaN(viewCoefficient))
                result.SetValue(HeatingSystemTypeParameter.ViewCoefficient, viewCoefficient);

            if (!double.IsNaN(supplyCircuitTemperature))
                result.SetValue(HeatingSystemTypeParameter.SupplyCircuitTemperature, supplyCircuitTemperature);

            if (!double.IsNaN(returnCircuitTemperature))
                result.SetValue(HeatingSystemTypeParameter.ReturnCircuitTemperature, returnCircuitTemperature);

            return result;
        }
    }
}