using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Space)), Description("Space Parameter")]
    public enum SpaceParameter
    {
        [ParameterProperties("Design Heating Load", "Design Heating Load"), DoubleParameterType(0)] DesignHeatingLoad,
        [ParameterProperties("Specified Exhaust Airflow", "Extract Airflow"), DoubleParameterType(0)] ExtractAirflow,
        [ParameterProperties("Specified Supply Airflow", "Supply Airflow"), DoubleParameterType(0)] SupplyAirflow,
    }
}