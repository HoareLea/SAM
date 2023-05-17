using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default TM59 SAM Analytical InternalCondition TextMap
        /// </summary>
        /// <returns name="TextMap">Default TM59 SAM Analytical InternalCondition TextMap</returns>
        /// <search>Default SAM Analytical InternalCondition, IC, InternalCondition, TextMap</search> 
        public static TextMap DefaultInternalConditionTextMap_TM59()
        {
            return ActiveSetting.Setting?.GetValue<TextMap>(AnalyticalSettingParameter.InternalConditionTextMap_TM59);
        }
    }
}