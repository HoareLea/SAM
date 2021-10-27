using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial Material(this Architectural.MaterialLayer materialLayer, MaterialLibrary materialLibrary)
        {
            if (materialLayer == null || materialLibrary == null)
                return null;

            return materialLibrary.GetObject<IMaterial>(materialLayer.Name);
        }
    }
}