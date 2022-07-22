namespace SAM.Analytical
{
    public static partial class Convert
    {  
        public static ConstructionLayer ToSAM(this Architectural.MaterialLayer materialLayer)
        {
            if(materialLayer == null)
            {
                return null;
            }

            return new ConstructionLayer(materialLayer.Name, materialLayer.Thickness);
        }
    }
}