using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SAM.Geometry.Spatial.Vector3D ToSAM(this Autodesk.DesignScript.Geometry.Vector vector)
        {
            return new SAM.Geometry.Spatial.Vector3D(vector.X, vector.Y, vector.Z);
        }
    }
}