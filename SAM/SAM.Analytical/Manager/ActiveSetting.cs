using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class ActiveSetting
    {
        private static Setting setting = Load();

        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
                setting = GetDefault();

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting result = new Setting(Assembly.GetExecutingAssembly());

            //File Names
            result.SetValue(AnalyticalSettingParameter.DefaultMaterialLibraryFileName, "SAM_MaterialLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultConstructionLibraryFileName, "SAM_ConstructionLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultApertureConstructionLibraryFileName, "SAM_ApertureConstructionLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultGasMaterialLibraryFileName, "SAM_GasMaterialLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultInternalConditionLibraryFileName, "SAM_InternalConditionLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultDegreeOfActivityLibraryFileName, "SAM_DegreeOfActivityLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultProfileLibraryFileName, "SAM_ProfileLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.InternaConditionTextMaplFileName, "SAM_InternalConditionTextMap.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultSystemTypeLibraryFileName, "SAM_SystemTypeLibrary.JSON");

            result.SetValue(AnalyticalSettingParameter.DefaultHostPartitionTypeLibraryFileName, "SAM_HostPartitionTypeLibrary.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultOpeningTypeLibraryFileName, "SAM_OpeningTypeLibrary.JSON");

            result.SetValue(AnalyticalSettingParameter.DefaultInternaConditionTextMaplFileName_TM59, "SAM_InternalConditionTextMap_TM59.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultInternalConditionLibraryFileName_TM59, "SAM_InternalConditionLibrary_TM59.JSON");
            result.SetValue(AnalyticalSettingParameter.DefaultProfileLibraryFileName_TM59, "SAM_ProfileLibrary_TM59.JSON");


            string path = null;

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultConstructionLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultConstructionLibrary, Core.Create.IJSAMObject<ConstructionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultMaterialLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultGasMaterialLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultGasMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultApertureConstructionLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultApertureConstructionLibrary, Core.Create.IJSAMObject<ApertureConstructionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultInternalConditionLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultInternalConditionLibrary, Core.Create.IJSAMObject<InternalConditionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultDegreeOfActivityLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary, Core.Create.IJSAMObject<DegreeOfActivityLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultProfileLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultProfileLibrary, Core.Create.IJSAMObject<ProfileLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.InternaConditionTextMaplFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.InternalConditionTextMap, Core.Create.IJSAMObject<TextMap>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultSystemTypeLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultSystemTypeLibrary, Core.Create.IJSAMObject<SystemTypeLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultHostPartitionTypeLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultHostPartitionTypeLibrary, Core.Create.IJSAMObject<HostPartitionTypeLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultOpeningTypeLibraryFileName);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultOpeningTypeLibrary, Core.Create.IJSAMObject<OpeningTypeLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultInternalConditionLibraryFileName_TM59);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultInternalConditionLibrary_TM59, Core.Create.IJSAMObject<InternalConditionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultInternaConditionTextMaplFileName_TM59);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.InternalConditionTextMap_TM59, Core.Create.IJSAMObject<TextMap>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultPath(result, AnalyticalSettingParameter.DefaultProfileLibraryFileName_TM59);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultProfileLibrary_TM59, Core.Create.IJSAMObject<ProfileLibrary>(System.IO.File.ReadAllText(path)));

            return result;
        }
    }
}