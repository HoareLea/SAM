using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SAM.Geometry.Spatial.Point3D ToSAM(this Autodesk.DesignScript.Geometry.Point point)
        {
            return new SAM.Geometry.Spatial.Point3D(point.X, point.Y, point.Z);
        }
    }
}