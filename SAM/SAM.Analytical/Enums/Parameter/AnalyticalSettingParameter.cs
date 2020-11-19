using System.ComponentModel;
using SAM.Core;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Setting)), Description("Analytical Setting Parameter")]
    public enum AnalyticalSettingParameter
    {
        [ParameterProperties("Default MaterialLibrary", "Default MaterialLibrary"), SAMObjectParameterValue(typeof(MaterialLibrary))] DefaultMaterialLibrary,
        [ParameterProperties("Default Gas MaterialLibrary", "Default Gas MaterialLibrary"), SAMObjectParameterValue(typeof(MaterialLibrary))] DefaultGasMaterialLibrary,
        [ParameterProperties("Default ConstructionLibrary", "Default ConstructionLibrary"), SAMObjectParameterValue(typeof(ConstructionLibrary))] DefaultConstructionLibrary,
        [ParameterProperties("Default ApertureConstructionLibrary", "Default ApertureConstructionLibrary"), SAMObjectParameterValue(typeof(ApertureConstructionLibrary))] DefaultApertureConstructionLibrary,
        [ParameterProperties("Default InternalConditionLibrary", "Default InternalConditionLibrary"), SAMObjectParameterValue(typeof(InternalConditionLibrary))] DefaultInternalConditionLibrary,
        [ParameterProperties("Default DegreeOfActivityLibrary", "Default DegreeOfActivityLibrary"), SAMObjectParameterValue(typeof(DegreeOfActivityLibrary))] DefaultDegreeOfActivityLibrary,
        [ParameterProperties("Default ProfileLibrary", "Default ProfileLibrary"), SAMObjectParameterValue(typeof(ProfileLibrary))] DefaultProfileLibrary,
        [ParameterProperties("InternalCondition TextMap", "InternalCondition TextMap"), SAMObjectParameterValue(typeof(TextMap))] InternalConditionTextMap,

        [ParameterProperties("Default MaterialLibrary File Name", "Default MaterialLibrary File Name"), ParameterValue(ParameterType.String)] DefaultMaterialLibraryFileName,
        [ParameterProperties("Default Gas MaterialLibrary File Name", "Default Gas MaterialLibrary File Name"), ParameterValue(ParameterType.String)] DefaultGasMaterialLibraryFileName,
        [ParameterProperties("Default ConstructionLibrary File Name", "Default ConstructionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultConstructionLibraryFileName,
        [ParameterProperties("Default ApertureConstructionLibrary File Name", "Default ApertureConstructionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultApertureConstructionLibraryFileName,
        [ParameterProperties("Default InternalConditionLibrary File Name", "Default InternalConditionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultInternalConditionLibraryFileName,
        [ParameterProperties("Default DegreeOfActivityLibrary File Name", "Default DegreeOfActivityLibrary File Name"), ParameterValue(ParameterType.String)] DefaultDegreeOfActivityLibraryFileName,
        [ParameterProperties("Default ProfileLibrary File Name", "Default ProfileLibrary File Name"), ParameterValue(ParameterType.String)] DefaultProfileLibraryFileName,
        [ParameterProperties("InternaCondition TextMap File Name", "InternaCondition TextMap File Name"), ParameterValue(ParameterType.String)] InternaConditionTextMaplFileName
    }
}