using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetDescription(this Construction construction, string description)
        {
            if (construction == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_Description();
             

            if (construction.SetParameter(assembly, parameterName, description))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, description))
                return false;

            return construction.Add(parameterSet);
        }
    }
}
