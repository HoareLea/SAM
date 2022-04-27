using SAM.Core;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Core SystemType Library
        /// </summary>
        /// <returns name="constructionLibrary"> Default SAM Core SystemType</returns>
        /// <search>Default SAM Core SystemType</search> 
        public static SystemTypeLibrary DefaultSystemTypeLibrary()
        {
            return ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);
        }
    }
}