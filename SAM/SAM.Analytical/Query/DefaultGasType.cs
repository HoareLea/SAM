using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static DefaultGasType DefaultGasType(this GasMaterial gasMaterial)
        {
            if (gasMaterial == null)
                return Analytical.DefaultGasType.Undefined;

            List<string> names = new List<string>();
            
            if (gasMaterial.TryGetValue(GasMaterialParameter.DefaultGasType, out string defaultGasType_String) && !string.IsNullOrWhiteSpace(defaultGasType_String))
            {
                DefaultGasType defaultGasType = Core.Query.Enum<DefaultGasType>(defaultGasType_String);
                if (defaultGasType != Analytical.DefaultGasType.Undefined)
                {
                    return defaultGasType;
                }
                
                names.Add(defaultGasType_String);
            }
            
            names.Add(gasMaterial.Name);
            names.Add(gasMaterial.DisplayName);
            names.Add(gasMaterial.Description);

            return DefaultGasType(names.ToArray());
        }

        public static DefaultGasType DefaultGasType(params string[] values)
        {
            if(values == null || values.Length == 0)
            {
                return Analytical.DefaultGasType.Undefined;
            }

            List<string> values_Temp = values.ToList();

            values_Temp.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            for (int i = 0; i < values_Temp.Count; i++)
                values_Temp[i] = values_Temp[i].Trim().ToUpper();

            values_Temp.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            DefaultGasType result = Analytical.DefaultGasType.Undefined;
            foreach (string value in values_Temp)
            {
                result = Core.Query.Enum<DefaultGasType>(value);
                if (result != Analytical.DefaultGasType.Undefined)
                {
                    return result;
                }
            }

            foreach (DefaultGasType defaultGasType_Temp in Enum.GetValues(typeof(DefaultGasType)))
            {
                string name = defaultGasType_Temp.ToString().ToUpper();
                List<string> names_DefaultGasType = values_Temp.FindAll(x => Core.Query.Compare(x, name, TextComparisonType.Contains, false));
                if (names_DefaultGasType.Count == 0 && defaultGasType_Temp == Analytical.DefaultGasType.SulfurHexaFluoride)
                    names_DefaultGasType = values_Temp.FindAll(x => Core.Query.Compare(x, "SF6", TextComparisonType.Contains, false));

                if (names_DefaultGasType == null || names_DefaultGasType.Count == 0)
                    continue;

                result = defaultGasType_Temp;
                break;
            }

            return result;


        }
    }
}