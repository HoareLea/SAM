// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Rhino
{
    public static partial class Modify
    {
        public static Layer AddLayer(this LayerTable layerTable, string prefix)
        {
            if (layerTable == null)
                return null;

            List<int> indexes = new List<int>();
            List<Layer> layers = Query.Layers(layerTable, prefix, TextComparisonType.StartsWith, true);
            if (layers != null || layers.Count != 0)
            {
                foreach (Layer layer in layers)
                {
                    string text = layer.Name.Substring(prefix.Length);
                    if (!string.IsNullOrEmpty(text))
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
