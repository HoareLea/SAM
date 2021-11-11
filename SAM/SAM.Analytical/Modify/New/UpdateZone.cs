using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Zone UpdateZone(this ArchitecturalModel architecturalModel, string name, ZoneType zoneType, params Space[] spaces)
        {
            if (architecturalModel == null || name == null)
                return null;

            return UpdateZone(architecturalModel, name, zoneType.Text(), spaces);
        }

        public static Zone UpdateZone(this ArchitecturalModel architecturalModel, string name, string zoneCategory, params Space[] spaces)
        {
            if (architecturalModel == null || name == null)
                return null;

            Zone result = null;

            List<Zone> zones = architecturalModel.GetObjects<Zone>(x => x.Name == name);

            if (zoneCategory != null)
            {
                result = zones?.Find(x => x.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory_Temp) && zoneCategory.Equals(zoneCategory_Temp));
            }

            if (result == null)
            {
                System.Guid guid;
                do
                {
                    guid = System.Guid.NewGuid();
                }
                while (architecturalModel.GetObject<Zone>(guid) != null);

                result = Create.Zone(guid, name, zoneCategory);
                if (result == null)
                    return null;
            }

            architecturalModel.Add(result, spaces);

            return result;
        }
    }
}