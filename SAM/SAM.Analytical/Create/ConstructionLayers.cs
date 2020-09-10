using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<ConstructionLayer> ConstructionLayers(params object[] values)
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
                    thicknesses.Add(Convert.ToDouble(@object));
            }

            if (names.Count != thicknesses.Count)
                return null;

            List<ConstructionLayer> result = new List<ConstructionLayer>();
            for (int i = 0; i < names.Count; i++)
                result.Add(new ConstructionLayer(names[i], thicknesses[i]));

            return result;
        }
    }
}