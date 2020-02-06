using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static JArray JArray(this IEnumerable<ICurve3D> curve3Ds)
        {
            if (curve3Ds == null)
                return null;

            JArray jArray = new JArray();
            foreach (ICurve3D curve3D in curve3Ds)
                jArray.Add(curve3D.ToJObject());

            return jArray;
        }

    }
}
