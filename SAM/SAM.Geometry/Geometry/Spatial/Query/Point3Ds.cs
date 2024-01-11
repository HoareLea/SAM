using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Point3D> Point3Ds(this IClosed3D closed3D)
        {
            if(closed3D is Face3D)
            {
                return Point3Ds((Face3D)closed3D, true, true);
            }
            
            
            if(!(closed3D is ICurvable3D))
            {
                throw new System.NotImplementedException();
            }

            return ((ICurvable3D)closed3D).GetCurves()?.ConvertAll(x => x.GetStart());

        }
        
        public static List<Point3D> Point3Ds(this Face3D face3D, bool externalEdge = true, bool internalEdges = true)
        {
            if (face3D == null)
                return null;

            List<Point3D> result = new List<Point3D>();

            if(externalEdge)
            {
                ICurvable3D curvable3D = face3D.GetExternalEdge3D() as ICurvable3D;
                if (curvable3D != null)
                {
                    List<Point3D> point3Ds = curvable3D.GetCurves()?.ConvertAll(x => x.GetStart());
                    if (point3Ds != null && point3Ds.Count > 0)
                        result.AddRange(point3Ds);
                }
            }

            if (internalEdges)
            {
                List<IClosedPlanar3D> closedPlanar3Ds = face3D.GetInternalEdge3Ds();
                if(closedPlanar3Ds != null)
                {
                    foreach(IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
                    {
                        ICurvable3D curvable3D = closedPlanar3D as ICurvable3D;
                        if (curvable3D != null)
                        {
                            List<Point3D> point3Ds = curvable3D.GetCurves()?.ConvertAll(x => x.GetStart());
                            if (point3Ds != null && point3Ds.Count > 0)
                                result.AddRange(point3Ds);
                        }
                    }
                }
            }

            return result;

        }
    }
}