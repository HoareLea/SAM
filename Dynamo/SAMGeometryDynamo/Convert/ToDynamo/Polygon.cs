using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Polygon ToDynamo(this SAM.Geometry.Spatial.Polygon3D polygon3D)
        {
            return Autodesk.DesignScript.Geometry.Polygon.ByPoints(polygon3D.GetPoints().ConvertAll(x => x.ToDynamo()));
        }
    }
}