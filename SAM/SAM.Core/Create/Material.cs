namespace SAM.Core
{
    public static partial class Create
    {
        public static Material Material(this Material material, string name, string displayName = null, string description = null)
        {
            if(name == null || material == null)
            {
                return null;
            }

            string displayName_Temp = displayName;
            if(displayName_Temp == null)
            {
                displayName_Temp = material.DisplayName;
            }

            string description_Temp = description;
            if (description_Temp == null)
            {
                description_Temp = material.Description;
            }

            if (material is GasMaterial)
            {
                return new GasMaterial(name, System.Guid.NewGuid(), (GasMaterial)material, displayName_Temp, description_Temp);
            }
            

            if(material is LiquidMaterial)
            {
                return new LiquidMaterial(name, System.Guid.NewGuid(), (LiquidMaterial)material, displayName_Temp, description_Temp);
            }

            if (material is OpaqueMaterial)
            {
                return new OpaqueMaterial(name, System.Guid.NewGuid(), (OpaqueMaterial)material, displayName_Temp, description_Temp);
            }

            throw new System.NotImplementedException();

        }
    }
}