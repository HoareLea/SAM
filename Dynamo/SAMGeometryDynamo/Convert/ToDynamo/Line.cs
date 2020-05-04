using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Line ToDynamo(this SAM.Geometry.Spatial.Segment3D segment3D)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(segment3D[0].ToDynamo(), segment3D[1].ToDynamo());
        }
    }
}