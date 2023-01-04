using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical InternalCondition TextMap
        /// </summary>
        /// <returns name="TextMap">Default SAM Analytical InternalCondition TextMap</returns>
        /// <search>Default SAM Analytical InternalCondition, IC, InternalCondition, TextMap</search> 
        public static TextMap DefaultInternalConditionTextMap()
        {
            return ActiveSetting.Setting.GetValue<TextMap>(AnalyticalSettingParameter.InternalConditionTextMap);
        }
    }
}