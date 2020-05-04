using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SAM.Geometry.Spatial.Segment3D ToSAM(this Autodesk.DesignScript.Geometry.Line line)
        {
            return new SAM.Geometry.Spatial.Segment3D(line.StartPoint.ToSAM(), line.EndPoint.ToSAM());
        }
    }
}