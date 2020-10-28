using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static bool SetPanelType(this Construction construction, PanelType panelType)
        {
            return construction.SetValue(ConstructionParameter.DefaultPanelType, panelType);

            //return SetPanelType(construction as SAMObject, panelType);
        }

        [Obsolete]
        public static bool SetPanelType(this Construction construction, PanelType panelType, Setting setting)
        {
            return construction.SetValue(ConstructionParameter.DefaultPanelType, panelType);
            //return SetPanelType(construction as SAMObject, panelType, setting);
        }

        [Obsolete]
        public static bool SetPanelType(this ApertureConstruction apertureConstruction, PanelType panelType)
        {
            return apertureConstruction.SetValue(ApertureConstructionParameter.DefaultPanelType, panelType);
            //return SetPanelType(apertureConstruction as SAMObject, panelType);
        }

        [Obsolete]
        public static bool SetPanelType(this ApertureConstruction apertureConstruction, PanelType panelType, Setting setting)
        {
            return apertureConstruction.SetValue(ApertureConstructionParameter.DefaultPanelType, panelType);
            //return SetPanelType(apertureConstruction as SAMObject, panelType, setting);
        }

        //[Obsolete]
        //private static bool SetPanelType(this SAMObject sAMObject, PanelType panelType, Setting setting)
        //{
        //    if (sAMObject == null)
        //        return false;

        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    string parameterName = Query.ParameterName_Type(setting);

        //    string value = panelType.ToString();

        //    if (sAMObject.SetParameter(assembly, parameterName, value))
        //        return true;

        //    ParameterSet parameterSet = new ParameterSet(assembly);
        //    if (!parameterSet.Add(parameterName, value))
        //        return false;

        //    return sAMObject.Add(parameterSet);
        //}

        //[Obsolete]
        //private static bool SetPanelType(this SAMObject sAMObject, PanelType panelType)
        //{
        //    if (sAMObject == null)
        //        return false;

        //    Assembly assembly = Assembly.GetExecutingAssembly();
        //    string parameterName = Query.ParameterName_Type();

        //    string value = panelType.ToString();

        //    if (sAMObject.SetParameter(assembly, parameterName, value))
        //        return true;

        //    ParameterSet parameterSet = new ParameterSet(assembly);
        //    if (!parameterSet.Add(parameterName, value))
        //        return false;

        //    return sAMObject.Add(parameterSet);
        //}
    }
}
