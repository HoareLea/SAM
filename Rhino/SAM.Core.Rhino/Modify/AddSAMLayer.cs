using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace SAM.Core.Rhino
{
    public static partial class Modify
    {
        public static Layer AddSAMLayer(this LayerTable layerTable)
        {
            return AddLayer(layerTable, "SAM_");
        }
    }
}