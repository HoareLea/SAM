using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static Layer GetLayer(this LayerTable layerTable, System.Guid parentId, string name)
        {
            if (layerTable == null || string.IsNullOrWhiteSpace(name))
                return null;

            int index = layerTable.Find(parentId, name, -1);
            if(index == -1)
            {
                index = layerTable.Add();
                layerTable[index].Name = name;
                layerTable[index].ParentLayerId = parentId; 
            }

            return layerTable[index];
        }
    }
}