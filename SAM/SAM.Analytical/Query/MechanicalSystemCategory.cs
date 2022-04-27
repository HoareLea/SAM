namespace SAM.Analytical
{
    public static partial class Query
    {
        public static MechanicalSystemCategory MechanicalSystemCategory(this MechanicalSystem mechanicalSystem)
        {
            if(mechanicalSystem == null)
            {
                return Analytical.MechanicalSystemCategory.Undefined;
            }

            if(mechanicalSystem is CoolingSystem)
            {
                return Analytical.MechanicalSystemCategory.Cooling;
            }

            if (mechanicalSystem is VentilationSystem)
            {
                return Analytical.MechanicalSystemCategory.Ventilation;
            }

            if (mechanicalSystem is HeatingSystem)
            {
                return Analytical.MechanicalSystemCategory.Heating;
            }

            return Analytical.MechanicalSystemCategory.Other;
        }
    }
}