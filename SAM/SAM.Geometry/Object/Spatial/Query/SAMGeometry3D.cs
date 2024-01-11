using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static T SAMGeometry3D<T>(this ISAMGeometry3DObject sAMGeometry3DObject) where T : ISAMGeometry3D
        {
            if(sAMGeometry3DObject is IFace3DObject)
            {
                Face3D result = ((IFace3DObject)sAMGeometry3DObject).Face3D;
                return result is T ? (T)(object)result : default(T);
            }

            if (sAMGeometry3DObject is ISegment3DObject)
            {
                Segment3D result = ((ISegment3DObject)sAMGeometry3DObject).Segment3D;
                return result is T ? (T)(object)result : default(T);
            }

            if (sAMGeometry3DObject is IPolygon3DObject)
            {
                Polygon3D result = ((IPolygon3DObject)sAMGeometry3DObject).Polygon3D;
                return result is T ? (T)(object)result : default(T);
            }

            return default(T);
        }
    }
}