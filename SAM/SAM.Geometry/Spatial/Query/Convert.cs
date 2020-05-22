namespace SAM.Geometry.Spatial
{
    /// <summary>
    /// Converts 2D point on given plane to 3D point. Plane is represented as origin and normal
    /// </summary>
    public static partial class Query
    {
		public static Point3D Convert(this Point3D origin, Vector3D normal, Planar.Point2D point2D, Vector3D axisX = null)
		{
			if (normal == null || origin == null || point2D == null)
				return null;

			Vector3D normal_Temp = normal.Unit;
			Vector3D axisX_Temp = axisX;

			if (axisX_Temp == null)
			{
				if (normal_Temp.X == 0 && normal_Temp.Y == 0)
					normal_Temp = new Vector3D(1, 0, 0);
				else
					normal_Temp = new Vector3D(normal_Temp.Y, -normal_Temp.X, 0).Unit;
			}

			Vector3D axisY = new Vector3D((normal_Temp.Y * axisX_Temp.Z) - (normal_Temp.Z * axisX_Temp.Y), (normal_Temp.Z * axisX_Temp.X) - (normal_Temp.X * axisX_Temp.Z), (normal_Temp.X * axisX_Temp.Y) - (normal_Temp.Y * axisX_Temp.X));

			Vector3D u = new Vector3D(axisX_Temp.X * point2D.X, axisX_Temp.Y * point2D.X, axisX_Temp.Z * point2D.X);
			Vector3D v = new Vector3D(axisY.X * point2D.Y, axisY.Y * point2D.Y, axisY.Z * point2D.Y);

			return new Point3D(origin.X + u.X + v.X, origin.Y + u.Y + v.Y, origin.Z + u.Z + v.Z);
		}
    }
}