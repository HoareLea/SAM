using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Space)), Description("Space Parameter")]
    public enum SpaceParameter
    {
        [ParameterProperties("Design Heating Load", "Design Heating Load"), DoubleParameterValue(0)] DesignHeatingLoad,
        [ParameterProperties("Specified Exhaust Airflow", "Extract Airflow"), DoubleParameterValue(0)] ExtractAirflow,
        [ParameterProperties("Specified Supply Airflow", "Supply Airflow"), DoubleParameterValue(0)] SupplyAirflow,
        [ParameterProperties("Volume", "Volume"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Area", "Area"), DoubleParameterValue(0)] Area,
    }
}