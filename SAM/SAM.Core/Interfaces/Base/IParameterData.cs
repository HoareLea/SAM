using SAM.Core.Attributes;

namespace SAM.Core
{
    public interface IParameterData
    {
        ParameterProperties ParameterProperties { get; }
        ParameterValue ParameterValue { get; }
    }
}
