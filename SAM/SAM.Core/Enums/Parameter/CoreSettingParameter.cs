using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Core
{
    [AssociatedTypes(typeof(Setting)), Description("Core Setting Parameter")]
    public enum CoreSettingParameter
    {
        [ParameterProperties("Resources Directory Name", "Resources Directory Name"), ParameterValue(ParameterType.String)] ResourcesDirectoryName,
    }
}