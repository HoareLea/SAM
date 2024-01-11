using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static ISAMGeometry3D ISAMGeometry3D(this ISAMGeometry3DObject sAMGeometry3DObject)
        {
            if(sAMGeometry3DObject == null)
            {
                return null;
            }

            if(sAMGeometry3DObject is IBoundingBox3DObject)
            {
                return ((IBoundingBox3DObject)sAMGeometry3DObject).BoundingBox3D;
            }

            if (sAMGeometry3DObject is IExtrusionObject)
            {
                return ((IExtrusionObject)sAMGeometry3DObject).Extrusion;
            }

            if (sAMGeometry3DObject is IFace3DObject)
            {
                return ((IFace3DObject)sAMGeometry3DObject).Face3D;
            }

            if (sAMGeometry3DObject is IMesh3DObject)
            {
                return ((IMesh3DObject)sAMGeometry3DObject).Mesh3D;
            }

            if (sAMGeometry3DObject is IPoint3DObject)
            {
                return ((IPoint3DObject)sAMGeometry3DObject).Point3D;
            }

            if (sAMGeometry3DObject is IPolygon3DObject)
            {
                return ((IPolygon3DObject)sAMGeometry3DObject).Polygon3D;
            }

            if (sAMGeometry3DObject is IPolyline3DObject)
            {
                return ((IPolyline3DObject)sAMGeometry3DObject).Polyline3D;
            }

            if (sAMGeometry3DObject is IRectangle3DObject)
            {
                return ((IRectangle3DObject)sAMGeometry3DObject).Rectangle3D;
            }

            if (sAMGeometry3DObject is ISAMGeometry3DGroupObject)
            {
                return ((ISAMGeometry3DGroupObject)sAMGeometry3DObject).SAMGeometry3DGroup;
            }

            if (sAMGeometry3DObject is ISegment3DObject)
            {
                return ((ISegment3DObject)sAMGeometry3DObject).Segment3D;
            }

            if (sAMGeometry3DObject is IShellObject)
            {
                return ((IShellObject)sAMGeometry3DObject).Shell;
            }

            if (sAMGeometry3DObject is ISphereObject)
            {
                return ((ISphereObject)sAMGeometry3DObject).Sphere;
            }

            if (sAMGeometry3DObject is ITriangle3DObject)
            {
                return ((ITriangle3DObject)sAMGeometry3DObject).Triangle3D;
            }

            return null;
        }
    }
}