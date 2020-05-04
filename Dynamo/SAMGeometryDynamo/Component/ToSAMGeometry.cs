namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        public static object ToSAMGeometry(object geometry)
        {
            return (geometry as Autodesk.DesignScript.Geometry.DesignScriptEntity)?.ToSAM();
        }
    }
}