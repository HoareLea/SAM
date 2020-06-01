namespace SAM.Geometry.Spatial
{
    /// <summary>
    /// Converts 2D point on given plane to 3D point. Plane is represented as origin and normal
    /// </summary>
    public static partial class Query
    {
		public static Point3D Convert(this Point3D origin, Vector3D normal, Planar.Point2D point2D, Vector3D axisY)
		{
			if (normal == null || origin == null || point2D == null || axisY == null)
				return null;

			Vector3D normal_Temp = normal.Unit;
			Vector3D axisY_Temp = axisY.Unit;

			Vector3D axisX = normal_Temp.CrossProduct(axisY_Temp).Unit;

			Vector3D u = new Vector3D(axisY_Temp.X * point2D.Y, axisY_Temp.Y * point2D.Y, axisY_Temp.Z * point2D.Y);
			Vector3D v = new Vector3D(axisX.X * point2D.X, axisX.Y * point2D.X, axisX.Z * point2D.X);

			return new Point3D(origin.X + u.X + v.X, origin.Y + u.Y + v.Y, origin.Z + u.Z + v.Z);
		}
	}
}