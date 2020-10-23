using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static bool IsBlind(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return true;

            return transparentMaterial.GetValue<bool>(TransparentMaterialParameter.IsBlind);

            bool result = true;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_IsBlind(), out result))
                return true;

            return result;
        }
    }
}