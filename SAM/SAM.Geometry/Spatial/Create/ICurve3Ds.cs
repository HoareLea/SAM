using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<ICurve3D> ICurve3Ds(this JArray jArray)
        {
            if (jArray == null)
                return null;

            List<ICurve3D> result = new List<ICurve3D>();

            foreach (JObject jObject in jArray)
                result.Add(ICurve3D(jObject));

            return result;
        }
    }
}