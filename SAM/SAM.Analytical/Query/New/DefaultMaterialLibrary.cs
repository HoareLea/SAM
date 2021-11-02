using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static MaterialLibrary DefaultMaterialLibrary()
        {
            return ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);
        }
    }
}