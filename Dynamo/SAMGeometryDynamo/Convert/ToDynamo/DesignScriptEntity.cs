using Autodesk.DesignScript.Runtime;

using SAM.Geometry;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.DesignScriptEntity ToDynamo(this ISAMGeometry geometry)
        {
            return Convert.ToDynamo(geometry as dynamic);
        }
    }
}