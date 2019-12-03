using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core.Dynamo
{
    public static class ParameterSet
    {
        public static Core.ParameterSet ByName(string name)
        {
            return new Core.ParameterSet(name);
        }
    }
}
