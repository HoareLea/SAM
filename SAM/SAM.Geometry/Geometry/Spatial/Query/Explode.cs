using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<ICurve3D> Explode(this ICurve3D curve3D)
        {
            if (curve3D == null)
                return null;

            List<ICurve3D> result = new List<ICurve3D>();
            if (curve3D is ICurvable3D)
            {
                List<ICurve3D> curve3Ds = ((ICurvable3D)curve3D).GetCurves();
                if (curve3Ds == null || curve3Ds.Count < 2)
                {
                    result.Add(curve3D);
                }
                else
                {
                    foreach (ICurve3D curve3D_Temp in curve3Ds)
                    {
                        List<ICurve3D> curve3Ds_Temp = Explode(curve3D_Temp);
                        if (curve3Ds_Temp != null && curve3Ds_Temp.Count > 0)
                            result.AddRange(curve3Ds_Temp);
                    }
                }
            }
            else
            {
                result.Add(curve3D);
            }

            return result;
        }

        public static List<ICurve3D> Explode(this IEnumerable<ICurve3D> curve3Ds)
        {
            if (curve3Ds == null)
                return null;

            List<ICurve3D> result = new List<ICurve3D>();
            foreach (ICurve3D curve3D in curve3Ds)
            {
                List<ICurve3D> curve3Ds_Temp = Explode(curve3D);
                if (curve3Ds_Temp != null && curve3Ds_Temp.Count > 0)
                    result.AddRange(curve3Ds_Temp);
            }

            return result;
        }
    }
}