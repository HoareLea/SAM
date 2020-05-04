using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Point ToDynamo(this SAM.Geometry.Spatial.Point3D point3D)
        {
            return Autodesk.DesignScript.Geometry.Point.ByCoordinates(point3D.X, point3D.Y, point3D.Z);
        }
    }
}