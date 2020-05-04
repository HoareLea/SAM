using Autodesk.DesignScript.Runtime;
using SAM.Geometry;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static ISAMGeometry ToSAM(this Autodesk.DesignScript.Geometry.DesignScriptEntity geometry)
        {
            return Convert.ToSAM(geometry as dynamic);
        }
    }
}