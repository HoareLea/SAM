using System.Collections.Generic;
using Rhino.DocObjects;
using System.Linq;
using Rhino.DocObjects.Tables;

namespace SAM.Core.Rhino
{
    public static partial class Modify
    {
        public static Layer AddSAMLayer(this LayerTable layerTable)
        {
            if (layerTable == null)
                return null;

            string prefix = "SAM_";

            List<int> indexes = new List<int>();
            List<Layer> layers = Query.Layers(layerTable, prefix, TextComparisonType.StartsWith, true);
            if(layers != null || layers.Count != 0)
            {
                foreach(Layer layer in layers)
                {
                    string text = layer.Name.Substring(prefix.Length);
                    if(!string.IsNullOrEmpty(text))
                    {
                        int index = -1;
                        if (int.TryParse(text, out index))
                            indexes.Add(index);
                    }
                }
            }

            int next = 0;
            if (indexes.Count != 0)
                next = indexes.Max();

            next++;

            int index_Layer = layerTable.Add();
            layerTable[index_Layer].Name = string.Format("{0}{1}", prefix, next);

            return layerTable[index_Layer];

        }
    }
}