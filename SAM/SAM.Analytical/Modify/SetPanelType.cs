using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetPanelType(this Construction construction, PanelType panelType)
        {
            if (construction == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_PanelType();

            string value = panelType.ToString();

            if (construction.SetParameter(assembly, parameterName, value))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, value))
                return false;

            return construction.Add(parameterSet);
        }
    }
}
