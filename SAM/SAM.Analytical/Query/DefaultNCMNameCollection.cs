namespace SAM.Analytical
{
    public static partial class Query
    {
        public static NCMNameCollection DefaultNCMNameCollection()
        {
            return ActiveSetting.Setting.GetValue<NCMNameCollection>(AnalyticalSettingParameter.DefaultNCMNameCollection);
        }
    }
}