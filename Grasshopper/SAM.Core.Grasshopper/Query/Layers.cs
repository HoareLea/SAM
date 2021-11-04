using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static List<Rhino.DocObjects.Layer> Layers(this Rhino.DocObjects.Tables.LayerTable layerTable, string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (layerTable == null)
                return null;

            List<Rhino.DocObjects.Layer> result = new List<Rhino.DocObjects.Layer>();

            int count = layerTable.Count;
            for(int i=0; i < count; i++)
            {
                Rhino.DocObjects.Layer layer = layerTable[i];
                if (layer == null)
                    continue;

                string name = layer.Name;

                if (Core.Query.Compare(name, text, textComparisonType, caseSensitive))
                    result.Add(layer);
            }

            return result;
        }
    }
}