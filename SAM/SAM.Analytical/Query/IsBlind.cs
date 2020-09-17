using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool IsBlind(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return true;

            bool result = true;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_IsBlind(), out result))
                return true;

            return result;
        }
    }
}