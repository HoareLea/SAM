using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Create
    {
        public static List<T> ISAMGeometries<T>(this JArray jArray) where T : ISAMGeometry
        {
            return Core.Create.IJSAMObjects<T>(jArray);
        }
    }
}