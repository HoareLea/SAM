namespace SAM.Analytical
{
    public static partial class Query
    {
        public static MergeSettings DefaultMergeSettings()
        {
            return ActiveSetting.Setting.GetValue<MergeSettings>(AnalyticalSettingParameter.DefaultMergeSettings);
        }
    }
}