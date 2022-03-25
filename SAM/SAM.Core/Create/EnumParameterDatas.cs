using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<EnumParameterData> EnumParameterDatas(params Type[] enumTypes)
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

            List<EnumParameterData> result = EnumParameterDatas(enums.ToArray());

            return result;
        }

        public static List<EnumParameterData> EnumParameterDatas(params Enum[] enums)
        {
            if(enums == null)
            {
                return null;
            }

            List<EnumParameterData> result = new List<EnumParameterData>();
            foreach(Enum @enum in enums)
            {
                EnumParameterData enumParameterData = new EnumParameterData(@enum);
                if(enumParameterData == null || enumParameterData.ParameterProperties == null)
                {
                    continue;
                }

                result.Add(enumParameterData);
            }

            return result;
        }
    }
}