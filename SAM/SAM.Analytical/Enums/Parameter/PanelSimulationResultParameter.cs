using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(PanelSimulationResult)), Description("PanelSimulationResult Parameter")]
    public enum PanelSimulationResultParameter
    {
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue()] Area,
        [ParameterProperties("Inside Conduction Heat Transfer", "Inside Conduction Heat Transfer [W]"), DoubleParameterValue()] InsideConductionHeatTransfer,
        [ParameterProperties("Outside Conduction Heat Transfer", "Outside Conduction Heat Transfer [W]"), DoubleParameterValue()] OutsideConductionHeatTransfer,
        [ParameterProperties("Load Type", "Load Type"), ParameterValue(Core.ParameterType.String)] LoadType,
    }
}