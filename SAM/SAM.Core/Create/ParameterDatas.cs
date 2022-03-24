using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<ParameterData> ParameterDatas(params Type[] enumTypes)
        {
            if(enumTypes == null)
            {
                return null;
            }

            HashSet<Enum> enums = new HashSet<Enum>();
            foreach(Type type in enumTypes)
            {
                if(type == null || !type.IsEnum)
                {
                    continue;
                }

                Array array = Enum.GetValues(type);
                if(array == null)
                {
                    continue;
                }

                foreach(Enum @enum in array)
                {
                    enums.Add(@enum);
                }
            }

            List<ParameterData> result = ParameterDatas(enums.ToArray());

            return result;
        }

        public static List<ParameterData> ParameterDatas(params Enum[] enums)
        {
            if(enums == null)
            {
                return null;
            }

            List<ParameterData> result = new List<ParameterData>();
            foreach(Enum @enum in enums)
            {
                ParameterData parameterData = ParameterData(@enum);
                if(parameterData == null)
                {
                    continue;
                }

                result.Add(parameterData);
            }

            return result;
        }
    }
}