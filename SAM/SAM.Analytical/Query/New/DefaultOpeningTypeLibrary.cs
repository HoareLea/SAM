namespace SAM.Analytical
{
    public static partial class Query
    {
        public static OpeningTypeLibrary DefaultOpeningTypeLibrary()
        {
            return ActiveSetting.Setting.GetValue<OpeningTypeLibrary>(AnalyticalSettingParameter.DefaultOpeningTypeLibrary);
        }
    }
}