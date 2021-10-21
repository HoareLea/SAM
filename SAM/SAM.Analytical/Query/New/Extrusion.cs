using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Extrusion Extrusion(this IHostPartition hostPartition, double tolernace = Core.Tolerance.Distance)
        {
            HostPartitionType hostPartitionType = hostPartition?.Type();
            if (hostPartitionType == null)
            {
                return null;
            }

            double thickness = hostPartitionType.GetThickness();
            if (double.IsNaN(thickness) || thickness == 0)
            {
                return null;
            }

            Face3D face3D = hostPartition.Face3D;
            if (face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if (plane == null)
            {
                return null;
            }


            Geometry.Planar.IClosed2D externalEdge2D = face3D.ExternalEdge2D;
            if (externalEdge2D == null)
            {
                return null;
            }

            Vector3D vector3D_Extrusion = null;
            Face3D face3D_Extrusion = null;

            Vector3D normal = plane.Normal;

            if(hostPartition is Floor)
            {
                if (normal.SameHalf(Vector3D.WorldZ))
                    normal.Negate();

                vector3D_Extrusion = normal * thickness;
                face3D_Extrusion = face3D;
            }
            else if(hostPartition is Roof)
            {
                if (!normal.SameHalf(Vector3D.WorldZ))
                    normal.Negate();

                vector3D_Extrusion = normal * thickness;
                face3D_Extrusion = face3D;
            }
            else
            {
                bool rectangular = Geometry.Planar.Query.Rectangular(externalEdge2D, out Geometry.Planar.Rectangle2D rectangle2D, tolernace);
                if (!rectangular)
                {
                    vector3D_Extrusion = normal * thickness;
                    face3D_Extrusion = face3D.GetMoved(normal.GetNegated() * (thickness / 2)) as Face3D;
                }
                else
                {
                    Vector3D xAxis = plane.Convert(rectangle2D.WidthDirection);
                    Vector3D yAxis = normal;
                    Vector3D zAxis = plane.Convert(rectangle2D.HeightDirection);

                    double width = rectangle2D.Width;
                    double height = rectangle2D.Height;

                    vector3D_Extrusion = zAxis * height;

                    Point3D origin = plane.Convert(rectangle2D.Origin);
                    origin = origin.GetMoved(yAxis.GetNegated() * (thickness / 2)) as Point3D;
                    Plane plane_Rectangle3D = Geometry.Spatial.Create.Plane(origin, xAxis, yAxis);

                    Geometry.Planar.Point2D point2D_Origin = plane_Rectangle3D.Convert(origin);

                    Geometry.Planar.Rectangle2D rectangle2D_Extrusion = new Geometry.Planar.Rectangle2D(point2D_Origin, width, thickness, plane_Rectangle3D.Convert(yAxis));
                    face3D_Extrusion = new Face3D(new Rectangle3D(plane_Rectangle3D, rectangle2D_Extrusion));
                }

            }

            if (face3D_Extrusion == null || vector3D_Extrusion == null)
                return null;

            return new Extrusion(face3D_Extrusion, vector3D_Extrusion);
        }
    }
}