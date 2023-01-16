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
        [ParameterProperties("Default SystemTypeLibrary", "Default SystemTypeLibrary"), SAMObjectParameterValue(typeof(SystemTypeLibrary))] DefaultSystemTypeLibrary,
        [ParameterProperties("InternalCondition TextMap", "InternalCondition TextMap"), SAMObjectParameterValue(typeof(TextMap))] InternalConditionTextMap,

        [ParameterProperties("TM59 InternalCondition TextMap", "TM59 InternalCondition TextMap"), SAMObjectParameterValue(typeof(TextMap))] InternalConditionTextMap_TM59,
        [ParameterProperties("TM59 Default InternalConditionLibrary", "TM59 Default InternalConditionLibrary"), SAMObjectParameterValue(typeof(InternalConditionLibrary))] DefaultInternalConditionLibrary_TM59,
        [ParameterProperties("TM59 Default ProfileLibrary", "TM59 Default ProfileLibrary"), SAMObjectParameterValue(typeof(InternalConditionLibrary))] DefaultProfileLibrary_TM59,

        [ParameterProperties("Default MaterialLibrary File Name", "Default MaterialLibrary File Name"), ParameterValue(ParameterType.String)] DefaultMaterialLibraryFileName,
        [ParameterProperties("Default Gas MaterialLibrary File Name", "Default Gas MaterialLibrary File Name"), ParameterValue(ParameterType.String)] DefaultGasMaterialLibraryFileName,
        [ParameterProperties("Default ConstructionLibrary File Name", "Default ConstructionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultConstructionLibraryFileName,
        [ParameterProperties("Default ApertureConstructionLibrary File Name", "Default ApertureConstructionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultApertureConstructionLibraryFileName,
        [ParameterProperties("Default InternalConditionLibrary File Name", "Default InternalConditionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultInternalConditionLibraryFileName,
        [ParameterProperties("Default DegreeOfActivityLibrary File Name", "Default DegreeOfActivityLibrary File Name"), ParameterValue(ParameterType.String)] DefaultDegreeOfActivityLibraryFileName,
        [ParameterProperties("Default ProfileLibrary File Name", "Default ProfileLibrary File Name"), ParameterValue(ParameterType.String)] DefaultProfileLibraryFileName,
        [ParameterProperties("InternaCondition TextMap File Name", "InternaCondition TextMap File Name"), ParameterValue(ParameterType.String)] InternaConditionTextMaplFileName,
        [ParameterProperties("Default SystemTypeLibrary File Name", "Default SystemTypeLibrary File Name"), ParameterValue(ParameterType.String)] DefaultSystemTypeLibraryFileName,

        [ParameterProperties("TM59 Default InternaCondition TextMap File Name", "TM59 Default InternaCondition TextMap File Name"), ParameterValue(ParameterType.String)] DefaultInternaConditionTextMaplFileName_TM59,
        [ParameterProperties("TM59 Default InternalConditionLibrary File Name", "TM59 Default InternalConditionLibrary File Name"), ParameterValue(ParameterType.String)] DefaultInternalConditionLibraryFileName_TM59,
        [ParameterProperties("TM59 Default ProfileLibrary File Name", "TM59 Default ProfileLibrary File Name"), ParameterValue(ParameterType.String)] DefaultProfileLibraryFileName_TM59,

        [ParameterProperties("Default HostPartitionTypeLibrary File Name", "Default HostPartitionTypeLibrary File Name"), ParameterValue(ParameterType.String)] DefaultHostPartitionTypeLibraryFileName,
        [ParameterProperties("Default HostPartitionTypeLibrary", "Default HostPartitionTypeLibrary"), SAMObjectParameterValue(typeof(HostPartitionTypeLibrary))] DefaultHostPartitionTypeLibrary,
        [ParameterProperties("Default OpeningTypeLibrary File Name", "Default OpeningTypeLibrary File Name"), ParameterValue(ParameterType.String)] DefaultOpeningTypeLibraryFileName,
        [ParameterProperties("Default OpeningTypeLibrary", "Default OpeningTypeLibrary"), SAMObjectParameterValue(typeof(OpeningTypeLibrary))] DefaultOpeningTypeLibrary,
    }
}