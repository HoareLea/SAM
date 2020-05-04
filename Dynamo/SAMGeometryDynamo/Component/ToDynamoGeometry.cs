using SAM.Geometry;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        public static object ToDynamoGeometry(object geometry)
        {
            return ((ISAMGeometry)geometry).ToDynamo();
        }
    }
}