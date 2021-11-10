using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Zone> MapZones(this ArchitecturalModel architecturalModel, TextMap textMap)
        {
            if(textMap == null || architecturalModel == null)
            {
                return null;
            }

            IEnumerable<string> zoneCategories = textMap.Keys;
            if (zoneCategories == null || zoneCategories.Count() == 0)
                return null;

            List<Space> spaces = architecturalModel.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return null;

            List<Zone> result = new List<Zone>();
            foreach (string zoneCategory in zoneCategories)
            {
                List<string> parameterNames = textMap.GetValues(zoneCategory);
                if (parameterNames == null || parameterNames.Count == 0)
                    continue;

                foreach (string parameterName in parameterNames)
                {
                    Dictionary<string, List<Space>> dictionary = Core.Query.Dictionary<string, Space>(spaces, parameterName);
                    if (dictionary == null || dictionary.Count == 0)
                        continue;

                    foreach (KeyValuePair<string, List<Space>> keyValuePair in dictionary)
                    {
                        Zone zone = architecturalModel.UpdateZone(keyValuePair.Key, zoneCategory, keyValuePair.Value?.ToArray());
                        if (zone == null)
                            continue;

                        result.Add(zone);
                    }
                }
            }

            return result;
        }
    }
}