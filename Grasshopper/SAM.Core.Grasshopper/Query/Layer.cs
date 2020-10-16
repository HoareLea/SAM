using Grasshopper;
using Grasshopper.Kernel;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static Rhino.DocObjects.Layer Layer(this Rhino.DocObjects.Tables.LayerTable layerTable, string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (layerTable == null)
                return null;

            int count = layerTable.Count;
            for(int i=0; i < count; i++)
            {
                Rhino.DocObjects.Layer layer = layerTable[i];
                if (layer == null)
                    continue;

                string name = layer.Name;

                if (Core.Query.Compare(name, text, textComparisonType, caseSensitive))
                    return layer;
            }


            return null;
        }
    }
}