using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Core
{
    [AssociatedTypes(typeof(IParameterizedSAMObject)), Description("ParameterizedSAMObject Parameter")]
    public enum ParameterizedSAMObjectParameter
    {
        [ParameterProperties("Category", "Category"), SAMObjectParameterValue(typeof(Category))] Category,
    }
}