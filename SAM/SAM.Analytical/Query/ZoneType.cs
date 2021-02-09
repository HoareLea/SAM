namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ZoneType ZoneType(this Zone zone)
        {
            if (zone == null)
                return Analytical.ZoneType.Undefined;


            if (!zone.TryGetValue(ZoneParameter.ZoneCategory, out string category) || string.IsNullOrWhiteSpace(category))
                return Analytical.ZoneType.Undefined;

            return Core.Query.Enum<ZoneType>(category);
        }
    }
}