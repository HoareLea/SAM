using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Surface ToDynamo(this SAM.Geometry.Spatial.Face3D face)
        {
            SAM.Geometry.Spatial.Polygon3D polygon3D = face.GetExternalEdge() as SAM.Geometry.Spatial.Polygon3D;

            if (polygon3D == null)
                return null;

            return Autodesk.DesignScript.Geometry.Surface.ByPatch(polygon3D.ToDynamo());
        }

        public static Autodesk.DesignScript.Geometry.Surface ToDynamo(this SAM.Geometry.Spatial.Surface surface)
        {
            SAM.Geometry.Spatial.IClosed3D closed3D = surface.GetExternalEdge();
            return Autodesk.DesignScript.Geometry.Surface.ByPatch(closed3D.ToDynamo());
        }
    }
}