namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ProfileLibrary DefaultProfileLibrary()
        {
            return ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);
        }
    }
}