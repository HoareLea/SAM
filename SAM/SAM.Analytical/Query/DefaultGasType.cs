using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static DefaultGasType DefaultGasType(this GasMaterial gasMaterial)
        {
            if (gasMaterial == null)
                return Analytical.DefaultGasType.Undefined;

            List<string> names = new List<string>();
            names.Add(gasMaterial.Name);
            names.Add(gasMaterial.DisplayName);
            names.Add(gasMaterial.Description);

            names.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            for(int i=0; i < names.Count; i++)
                names[i] = names[i].Trim().ToUpper();

            names.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            DefaultGasType defaultGasType = Analytical.DefaultGasType.Undefined;
            foreach (DefaultGasType defaultGasType_Temp in System.Enum.GetValues(typeof(DefaultGasType)))
            {
                string name = defaultGasType_Temp.ToString().ToUpper();
                List<string> names_DefaultGasType = names.FindAll(x => Core.Query.Compare(x, name, TextComparisonType.Contains, false));
                if(names_DefaultGasType.Count == 0 && defaultGasType_Temp == Analytical.DefaultGasType.SulfurHexaFluoride)
                    names_DefaultGasType = names.FindAll(x => Core.Query.Compare(x, "SF6", TextComparisonType.Contains, false));

                if (names_DefaultGasType == null || names_DefaultGasType.Count == 0)
                    continue;

                defaultGasType = defaultGasType_Temp;
                break;
            }

            return defaultGasType;
        }
    }
}