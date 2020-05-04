using Autodesk.DesignScript.Runtime;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Curve ToDynamo(this SAM.Geometry.Spatial.ICurve3D curve3D)
        {
            if (curve3D is SAM.Geometry.Spatial.Segment3D)
                return ToDynamo((SAM.Geometry.Spatial.Segment3D)curve3D);

            if (curve3D is SAM.Geometry.Spatial.ICurvable3D)
                return ToDynamo((SAM.Geometry.Spatial.ICurvable3D)curve3D);

            return ToDynamo(curve3D as dynamic);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Autodesk.DesignScript.Geometry.Curve ToDynamo(this SAM.Geometry.Spatial.IClosed3D closed3D)
        {
            if (closed3D is SAM.Geometry.Spatial.ICurvable3D)
                return ((SAM.Geometry.Spatial.ICurvable3D)closed3D).ToDynamo();

            return null;
        }
    }
}