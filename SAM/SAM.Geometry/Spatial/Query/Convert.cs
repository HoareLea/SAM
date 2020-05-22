namespace SAM.Geometry.Spatial
{
    /// <summary>
    /// Converts 2D point on given plane to 3D point. Plane is represented as origin and normal
    /// </summary>
    public static partial class Query
    {
        public static Point3D Convert(this Point3D origin, Vector3D normal, Planar.Point2D point2D)
        {
			if (normal == null || origin == null || point2D == null)
				return null;

			Vector3D baseX = null;
			if (normal.X == 0 && normal.Y == 0)
				baseX = new Vector3D(1, 0, 0);
			else
				baseX = new Vector3D(normal.Y, -normal.X, 0).Unit;

			Vector3D baseY = new Vector3D((normal.Y * baseX.Z) - (normal.Z * baseX.Y), (normal.Z * baseX.X) - (normal.X * baseX.Z), (normal.X * baseX.Y) - (normal.Y * baseX.X));

			Vector3D u = new Vector3D(baseX.X * point2D.X, baseX.Y * point2D.X, baseX.Z * point2D.X);
			Vector3D v = new Vector3D(baseY.X * point2D.Y, baseY.Y * point2D.Y, baseY.Z * point2D.Y);

			return new Point3D(origin.X + u.X + v.X, origin.Y + u.Y + v.Y, origin.Z + u.Z + v.Z);
		}
    }
}