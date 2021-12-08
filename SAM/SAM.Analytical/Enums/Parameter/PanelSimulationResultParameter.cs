using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(PanelSimulationResult)), Description("PanelSimulationResult Parameter")]
    public enum PanelSimulationResultParameter
    {
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue()] Area,
        [ParameterProperties("Inside Conduction Heat Loss", "Inside Conduction Heat Loss [W]"), DoubleParameterValue()] InsideConductionHeatLoss,
        [ParameterProperties("Outside Conduction Heat Loss", "Outside Conduction Heat Loss [W]"), DoubleParameterValue()] OutsideConductionHeatLoss,
    }
}