using Grasshopper.Kernel.Types;

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static IGH_Goo ToGrasshopper(this ISAMGeometry geometry)
        {
            if (geometry is Polygon3D)
                return ((Polygon3D)geometry).ToGrasshopper();

            if (geometry is Polyline3D)
            {
                Polyline3D polyline3D = (Polyline3D)geometry;
                return polyline3D.ToGrasshopper(polyline3D.GetStart() == polyline3D.GetEnd());
            }

            if (geometry is Point3D)
                return ((Point3D)geometry).ToGrasshopper();

            if (geometry is Vector3D)
                return ((Vector3D)geometry).ToGrasshopper();

            if (geometry is Segment3D)
                return ((Segment3D)geometry).ToGrasshopper();

            if (geometry is Face3D)
                return ((Face3D)geometry).ToGrasshopper();

            if (geometry is Surface)
                return ((Surface)geometry).ToGrasshopper();

            if (geometry is Planar.Polygon2D)
                return ((Planar.Polygon2D)geometry).ToGrasshopper();

            if (geometry is Planar.Point2D)
                return ((Planar.Point2D)geometry).ToGrasshopper();

            if (geometry is Planar.Segment2D)
                return ((Planar.Segment2D)geometry).ToGrasshopper();

            if (geometry is Polycurve3D)
                return ((Polycurve3D)geometry).ToGrasshopper();

            if (geometry is Plane)
                return ((Plane)geometry).ToGrasshopper();

            return (geometry as dynamic).ToGrasshopper();
        }
    }
}