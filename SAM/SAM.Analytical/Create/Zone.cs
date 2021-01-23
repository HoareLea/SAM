using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Zone Zone(System.Guid guid, string name, ZoneType zoneType)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Zone result = new Zone(guid, name);
            result.SetValue(ZoneParameter.ZoneCategory, zoneType.Text());

            return result;
        }
    }
}