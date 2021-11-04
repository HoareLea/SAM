using System.Collections.Generic;

namespace SAM.Core.Rhino
{
    public static partial class Query
    {
        public static List<global::Rhino.DocObjects.Layer> Layers(this global::Rhino.DocObjects.Tables.LayerTable layerTable, string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (layerTable == null)
                return null;

            List<global::Rhino.DocObjects.Layer> result = new List<global::Rhino.DocObjects.Layer>();

            int count = layerTable.Count;
            for(int i=0; i < count; i++)
            {
                global::Rhino.DocObjects.Layer layer = layerTable[i];
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