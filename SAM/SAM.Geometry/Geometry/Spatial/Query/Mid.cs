namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Mid(this Point3D point3D_1, Point3D point3D_2)
        {
            if (point3D_1 == null || point3D_2 == null )
                return null;

            return new Point3D((point3D_1.X + point3D_2.X) / 2, (point3D_1.Y + point3D_2.Y) / 2, (point3D_1.Z + point3D_2.Z) / 2);
        }

        public static Point3D Mid(this Segment3D segment3D)
        {
            if (segment3D == null)
                return null;

            return Mid(segment3D.GetStart(), segment3D.GetEnd());
        }
    }
}