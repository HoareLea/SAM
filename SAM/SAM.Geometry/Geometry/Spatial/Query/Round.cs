namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Segment3D Round(this Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null)
            {
                return null;
            }

            Point3D point3D_1 = segment3D[0];
            point3D_1.Round(tolerance);

            Point3D point3D_2 = segment3D[1];
            point3D_2.Round(tolerance);

            return new Segment3D(point3D_1, point3D_2);
        }
    }
}