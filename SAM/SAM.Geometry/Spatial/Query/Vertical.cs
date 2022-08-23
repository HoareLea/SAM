namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Vertical(this Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null)
                return false;

            return System.Math.Abs(plane.Normal.DotProduct(Plane.WorldXY.AxisZ)) < tolerance;
        }

        public static bool Vertical(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return false;

            return Vertical(face3D.GetPlane(), tolerance);
        }

        public static bool Vertical(this IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance)
        {
            if (face3DObject == null)
                return false;

            return Vertical(face3DObject.Face3D, tolerance);
        }
    }
}