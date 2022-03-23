using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical ConstructionLibrary
        /// </summary>
        /// <returns name="constructionLibrary"> Default SAM Analytical ConstructionLibrary</returns>
        /// <search>Default SAM Analytical Construction, PanelType</search> 
        public static string DefaultConstructionLibraryPath()
        {
            return DefaultPath(ActiveSetting.Setting, AnalyticalSettingParameter.DefaultConstructionLibraryFileName);
        }
    }
}