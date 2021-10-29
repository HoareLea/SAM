using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static Log Log(this IEnumerable<MaterialLayer> materialLayers, string name, System.Guid guid)
        {
            string name_Temp = name;
            if (string.IsNullOrEmpty(name))
                name_Temp = "???";

            Log result = new Log();
            if (materialLayers == null || materialLayers.Count() == 0)
            {
                result.Add(string.Format("{0} (Guid: {1}) has no construction layers", name_Temp, guid), LogRecordType.Warning);
                return result;
            }

            for (int i = 0; i < materialLayers.Count(); i++)
            {
                MaterialLayer materialLayer = materialLayers.ElementAt(i);

                if (string.IsNullOrWhiteSpace(materialLayer.Name))
                    result.Add(string.Format("{0} (Guid: {1}) has layer with no name (Construction Layer Index: {2})", name_Temp, guid, i), LogRecordType.Error);

                if (materialLayer.Thickness <= 0)
                    result.Add(string.Format("{0} (Guid: {1}) has layer with thickness equal or less than 0 (Construction Layer Index: {2})", name_Temp, guid, i), LogRecordType.Error);
            }

            return result;
        }
    }
}