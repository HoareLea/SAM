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
            result.SetValue(AnalyticalSettingParameter.InternaConditionTextMaplFileName, "SAM_InternalConditionTextMap.JSON");

            string path = null;

            path = Query.DefaultConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultConstructionLibrary, Core.Create.IJSAMObject<ConstructionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultMaterialLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultGasMaterialLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultGasMaterialLibrary, Core.Create.IJSAMObject<MaterialLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultApertureConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultApertureConstructionLibrary, Core.Create.IJSAMObject<ApertureConstructionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultInternalConditionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultInternalConditionLibrary, Core.Create.IJSAMObject<InternalConditionLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.DefaultDegreeOfActivityLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary, Core.Create.IJSAMObject<DegreeOfActivityLibrary>(System.IO.File.ReadAllText(path)));

            path = Query.InternalConditionTextMapPath(result);
            if (System.IO.File.Exists(path))
                result.SetValue(AnalyticalSettingParameter.InternalConditionTextMap, Core.Create.IJSAMObject<TextMap>(System.IO.File.ReadAllText(path)));

            return result;
        }
    }
}