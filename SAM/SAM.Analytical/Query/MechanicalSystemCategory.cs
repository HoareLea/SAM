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

        public static MechanicalSystemCategory MechanicalSystemCategory(this MechanicalSystemType mechanicalSystemType)
        {
            if (mechanicalSystemType == null)
            {
                return Analytical.MechanicalSystemCategory.Undefined;
            }

            if (mechanicalSystemType is CoolingSystemType)
            {
                return Analytical.MechanicalSystemCategory.Cooling;
            }

            if (mechanicalSystemType is VentilationSystemType)
            {
                return Analytical.MechanicalSystemCategory.Ventilation;
            }

            if (mechanicalSystemType is HeatingSystemType)
            {
                return Analytical.MechanicalSystemCategory.Heating;
            }

            return Analytical.MechanicalSystemCategory.Other;
        }
    }
}