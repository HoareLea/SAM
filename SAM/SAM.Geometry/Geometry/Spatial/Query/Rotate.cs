namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Rotate a vector around a plane normal by a given angle using Rodrigues' rotation formula.
        /// </summary>
        /// <param name="vector3D">Vector to be rotated</param>
        /// <param name="plane">Rotation plane</param>
        /// <param name="angle">angle in radians</param>
        /// <returns></returns>
        public static Vector3D Rotate(this Vector3D vector3D, Plane plane, double angle)
        {
            if(vector3D is null || plane?.Normal is not Vector3D axis || double.IsNaN(angle))
            {
                return null;
            }

            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            // Rodrigues' rotation formula
            return vector3D * cos + axis.CrossProduct(vector3D) * sin + axis * axis.DotProduct(vector3D) * (1 - cos);
        }
    }
}