using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Vector ToDynamo(this SAM.Geometry.Spatial.Vector3D vector3D)
        {
            return Autodesk.DesignScript.Geometry.Vector.ByCoordinates(vector3D.X, vector3D.Y, vector3D.Z);
        }
    }
}