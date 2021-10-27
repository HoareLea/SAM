namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double MinDimension(this BoundingBox3D boundingBox3D)
        {
            if(boundingBox3D == null)
            {
                return double.NaN;
            }

            return Math.Query.Min(boundingBox3D.Width, boundingBox3D.Height, boundingBox3D.Depth);

        }
    }
}