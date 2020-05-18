namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Angle(this Vector3D vector3D_1, Vector3D vector3D_2, Plane plane)
        {
            if (vector3D_1 == null || vector3D_2 == null || plane == null)
                return double.NaN;

            Vector3D vector3D_Project_1 = plane.Project(vector3D_1).Unit;
            Vector3D vector3D_Project_2 = plane.Project(vector3D_2).Unit;

            double dotProduct = vector3D_Project_1.DotProduct(vector3D_Project_2);
            Vector3D normal = plane.Normal.Unit;

            double determinant = vector3D_Project_1.X * vector3D_Project_2.Y * normal.Z + vector3D_Project_2.X * normal.Y * vector3D_Project_1.Z + normal.X * vector3D_Project_1.Y * vector3D_Project_2.Z - vector3D_Project_1.Z * vector3D_Project_2.Y * normal.X - vector3D_Project_2.Z * normal.Y * vector3D_Project_1.X - normal.Z * vector3D_Project_1.Y * vector3D_Project_2.X;

            double angle = System.Math.Atan2(determinant, dotProduct);
            return angle >= 0 ? angle : System.Math.PI * 2 + angle;
        }
    }
}