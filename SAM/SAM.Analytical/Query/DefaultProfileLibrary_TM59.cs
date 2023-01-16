namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default TM59 SAM Analytical ProfileLibrary
        /// </summary>
        /// <returns name="constructionLibrary"> Default TM59 SAM Analytical ProfileLibrary</returns>
        /// <search>Default SAM Analytical Profile Library</search> 
        public static ProfileLibrary DefaultProfileLibrary_TM59()
        {
            return ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary_TM59);
        }
    }
}