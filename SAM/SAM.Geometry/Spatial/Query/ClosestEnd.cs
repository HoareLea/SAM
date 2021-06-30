namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D ClosestEnd(this ICurve3D curve3D, Point3D point3D)
        {
            if (curve3D == null || point3D == null)
                return null;

            Point3D point3D_1 = curve3D.GetStart();
            Point3D point3D_2 = curve3D.GetEnd();
            
            if(point3D_1 == null && point3D_2 == null)
            {
                return null;
            }

            if(point3D_1 == null)
            {
                return point3D_2;
            }

            if(point3D_2 == null)
            {
                return point3D_1;
            }

            if(point3D_1.Distance(point3D) < point3D_2.Distance(point3D))
            {
                return point3D_1;
            }

            return point3D_2;
        }
    }
}