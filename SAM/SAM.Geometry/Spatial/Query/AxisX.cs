namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D AxisX(this Vector3D normal)
        {
            if (normal == null)
                return null;

            if (normal.X == 0 && normal.Y == 0)
                return new Vector3D(1, 0, 0);

            return new Vector3D(normal.Y, -normal.X, 0).Unit;
        }
    }
}