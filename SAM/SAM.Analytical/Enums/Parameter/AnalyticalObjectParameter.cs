using System.ComponentModel;
using SAM.Core;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(IAnalyticalObject)), Description("AnalyticalObject Parameter")]
    public enum AnalyticalObjectParameter
    {
        [ParameterProperties("Category", "Category"), SAMObjectParameterValue(typeof(Category))] Category,
    }
}