namespace SAM.Analytical
{
    public static partial class Query
    {
        public static HostPartitionTypeLibrary DefaultHostPartitionTypeLibrary()
        {
            return ActiveSetting.Setting.GetValue<HostPartitionTypeLibrary>(AnalyticalSettingParameter.DefaultHostPartitionTypeLibrary);
        }
    }
}