using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool SetPanelType(this Construction construction, PanelType panelType)
        {
            return SetPanelType(construction as SAMObject, panelType);
        }

        public static bool SetPanelType(this Construction construction, PanelType panelType, Setting setting)
        {
            return SetPanelType(construction as SAMObject, panelType, setting);
        }

        public static bool SetPanelType(this ApertureConstruction apertureConstruction, PanelType panelType)
        {
            return SetPanelType(apertureConstruction as SAMObject, panelType);
        }

        public static bool SetPanelType(this ApertureConstruction apertureConstruction, PanelType panelType, Setting setting)
        {
            return SetPanelType(apertureConstruction as SAMObject, panelType, setting);
        }


        private static bool SetPanelType(this SAMObject sAMObject, PanelType panelType, Setting setting)
        {
            if (sAMObject == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_PanelType(setting);

            string value = panelType.ToString();

            if (sAMObject.SetParameter(assembly, parameterName, value))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, value))
                return false;

            return sAMObject.Add(parameterSet);
        }

        private static bool SetPanelType(this SAMObject sAMObject, PanelType panelType)
        {
            if (sAMObject == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_PanelType();

            string value = panelType.ToString();

            if (sAMObject.SetParameter(assembly, parameterName, value))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, value))
                return false;

            return sAMObject.Add(parameterSet);
        }
    }
}
