using SAM.Core;
using System;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
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

            //TODO: Use SetValue Insetad SetDescription
            construction.SetValue(ConstructionParameter.Description, description);

            return construction.Add(parameterSet);
        }

        [Obsolete]
        public static bool SetDescription(this ApertureConstruction apertureConstruction, string description)
        {
            if (apertureConstruction == null)
                return false;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string parameterName = Query.ParameterName_Description();


            if (apertureConstruction.SetParameter(assembly, parameterName, description))
                return true;

            ParameterSet parameterSet = new ParameterSet(assembly);
            if (!parameterSet.Add(parameterName, description))
                return false;

            //TODO: Use SetValue Insetad SetDescription
            apertureConstruction.SetValue(ApertureConstructionParameter.Description, description);

            return apertureConstruction.Add(parameterSet);
        }
    }
}
