using SAM.Core;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static IMaterial Material(this MaterialLayer materialLayer, MaterialLibrary materialLibrary)
        {
            if (materialLayer == null || materialLibrary == null)
                return null;

            return materialLibrary.GetObject<IMaterial>(materialLayer.Name);
        }
    }
}