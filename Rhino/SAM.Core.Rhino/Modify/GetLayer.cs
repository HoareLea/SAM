// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace SAM.Core.Rhino
{
    public static partial class Modify
    {
        public static Layer GetLayer(this LayerTable layerTable, System.Guid parentId, string name, System.Drawing.Color color)
        {
            if (layerTable == null || string.IsNullOrWhiteSpace(name))
                return null;

            int index = layerTable.Find(parentId, name, -1);
            if (index == -1)
            {
                index = layerTable.Add();
                layerTable[index].Name = name;
            }

            layerTable[index].ParentLayerId = parentId;
            layerTable[index].Color = color;

            return layerTable[index];
        }
    }
}
