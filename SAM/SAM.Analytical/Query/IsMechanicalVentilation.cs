namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool IsMechanicalVentilation(this VentilationSystem ventilationSystem)
        {
            if(ventilationSystem == null)
            {
                return false;
            }

            MechanicalSystemType mechanicalSystemType = ventilationSystem?.Type;
            if(!string.IsNullOrWhiteSpace(mechanicalSystemType?.Name))
            {
                return IsMechanicalVentilation(mechanicalSystemType.Name);
            }

            return IsMechanicalVentilation(ventilationSystem.Name);
        }

        public static bool IsMechanicalVentilation(this VentilationSystemType ventilationSystemType)
        {
            return IsMechanicalVentilation(ventilationSystemType?.Name);
        }

        public static bool IsMechanicalVentilation(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            name = name.Trim().ToUpper();

            if (name != "UU" && name != "NV")
            {
                return true;
            }

            return false;
        }
    }
}