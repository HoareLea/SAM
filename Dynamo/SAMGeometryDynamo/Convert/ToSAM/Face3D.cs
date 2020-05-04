using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;

namespace SAMGeometryDynamo
{
    public static partial class Convert
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SAM.Geometry.Spatial.Face3D ToSAM(this Autodesk.DesignScript.Geometry.Surface surface)
        {
            List<SAM.Geometry.Spatial.Point3D> points = new List<SAM.Geometry.Spatial.Point3D>();
            foreach (Autodesk.DesignScript.Geometry.Vertex vertex in surface.Vertices)
            {
                points.Add(vertex.PointGeometry.ToSAM());
            }

            if (points == null || points.Count == 0)
                return null;

            SAM.Geometry.Spatial.Polygon3D polygon3D = new SAM.Geometry.Spatial.Polygon3D(points);

            return new SAM.Geometry.Spatial.Face3D(polygon3D);
        }
    }
}