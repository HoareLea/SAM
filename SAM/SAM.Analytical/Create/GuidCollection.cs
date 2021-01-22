using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static GuidCollection GuidCollection(System.Guid guid, string name, AnalyticalZoneType analyticalZoneType)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            GuidCollection result = new GuidCollection(guid, name);
            result.SetValue(AnalyticalZoneParameter.AnalyticalZoneType, analyticalZoneType.Text());

            return result;
        }
    }
}