namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default TM59 SAM Analytical InternalConditionLibrary
        /// </summary>
        /// <returns name="constructionLibrary"> Default TM59 SAM Analytical InternalConditionLibrary</returns>
        /// <search>Default SAM Analytical InternalCondition Library</search> 
        public static InternalConditionLibrary DefaultInternalConditionLibrary_TM59()
        {
            return ActiveSetting.Setting.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary_TM59);
        }
    }
}