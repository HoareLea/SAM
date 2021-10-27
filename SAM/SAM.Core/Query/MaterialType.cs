namespace SAM.Core
{
    public static partial class Query
    {
        public static MaterialType MaterialType(this IMaterial material)
        {
            if (material == null)
                return Core.MaterialType.Undefined;

            if (material is OpaqueMaterial)
                return Core.MaterialType.Opaque;

            if (material is TransparentMaterial)
                return Core.MaterialType.Transparent;

            if (material is GasMaterial)
                return Core.MaterialType.Gas;

            return Core.MaterialType.Undefined;
        }
    }
}