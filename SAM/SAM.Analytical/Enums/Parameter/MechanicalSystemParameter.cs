using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(MechanicalSystem)), Description("Mechanical System Parameter")]
    public enum MechanicalSystemParameter
    {
        [ParameterProperties("Supply Unit Name", "Supply Unit Name"), ParameterValue(Core.ParameterType.String)] SupplyUnitName,
        [ParameterProperties("Exhaust Unit Name", "Exhaust Unit Name"), ParameterValue(Core.ParameterType.String)] ExhaustUnitName,
    }
}