using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static List<MaterialLayer> MaterialLayers(params object[] values)
        {
            if (values == null)
                return null;

            List<string> names = new List<string>();
            List<double> thicknesses = new List<double>();
            foreach(object @object in values)
            {
                if (@object is string)
                    names.Add((string)@object);
                else if (Core.Query.IsNumeric(@object))
                    thicknesses.Add(System.Convert.ToDouble(@object));
            }

            if (names.Count != thicknesses.Count)
                return null;

            List<MaterialLayer> result = new List<MaterialLayer>();
            for (int i = 0; i < names.Count; i++)
                result.Add(new MaterialLayer(names[i], thicknesses[i]));

            return result;
        }
    }
}