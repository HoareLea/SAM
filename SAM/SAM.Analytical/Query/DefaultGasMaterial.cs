using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static GasMaterial DefaultGasMaterial(this DefaultGasType defaultGasType)
        {
            if (defaultGasType == Analytical.DefaultGasType.Undefined)
                return null;

            MaterialLibrary materialLibrary = DefaultGasMaterialLibrary();
            if (materialLibrary == null)
                return null;

            string name = defaultGasType.ToString();

            List<GasMaterial> gasMaterials = materialLibrary.GetObjects<GasMaterial>(name, TextComparisonType.Contains, false);
            if (gasMaterials != null && gasMaterials.Count > 0)
            {
                if (gasMaterials.Count > 1)
                    gasMaterials.Sort((x, y) => (x.Name.Length - name.Length).CompareTo(y.Name.Length - name.Length));

                return gasMaterials[0];
            }

            gasMaterials = materialLibrary.GetObjects<GasMaterial>();
            if (gasMaterials == null || gasMaterials.Count == 0)
                return null;

            name = name.ToLower();

            foreach(GasMaterial gasMaterial in gasMaterials)
            {
                string name_Material = gasMaterial?.Name;
                if (string.IsNullOrWhiteSpace(name_Material))
                    continue;

                name_Material = name_Material.ToLower().Replace(" ", string.Empty);
                if (name_Material.Contains(name))
                    return gasMaterial;
            }

            return null;
        }

        public static GasMaterial DefaultGasMaterial(this GasMaterial gasMaterial)
        {
            if (gasMaterial == null)
                return null;

            DefaultGasType defaultGasType = Query.DefaultGasType(gasMaterial);
            if (defaultGasType == Analytical.DefaultGasType.Undefined)
                return null;

            return DefaultGasMaterial(defaultGasType);
        }
    }
}