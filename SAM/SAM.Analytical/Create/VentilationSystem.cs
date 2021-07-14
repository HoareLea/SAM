namespace SAM.Analytical
{
    public static partial class Create
    {
        public static VentilationSystem VentilationSystem(VentilationSystemType ventilationSystemType, int index = -1, string supplyUnitName = null, string exhaustUnitName = null)
        {

            VentilationSystem result = MechanicalSystem(ventilationSystemType, index) as VentilationSystem;
            if (result == null)
                return null;

            if (supplyUnitName != null)
                result.SetValue(VentilationSystemParameter.SupplyUnitName, supplyUnitName);

            if (exhaustUnitName != null)
                result.SetValue(VentilationSystemParameter.ExhaustUnitName, exhaustUnitName);

            return result;
        }
    }
}