namespace SAM.Analytical
{
    public static partial class Create
    {
        public static MechanicalSystem MechanicalSystem(MechanicalSystemType mechanicalSystemType, int index = -1)
        {
            if (mechanicalSystemType == null)
                return null;

            string name = mechanicalSystemType.Name;
            if (index != -1)
            {
                if (string.IsNullOrEmpty(name))
                    name = index.ToString();
                else
                    name += " " + index.ToString();
            }

            MechanicalSystem result = null;
            if(mechanicalSystemType is VentilationSystemType)
                result = new VentilationSystem(name, (VentilationSystemType)mechanicalSystemType);
            else if(mechanicalSystemType is HeatingSystemType)
                result = new HeatingSystem(name, (HeatingSystemType)mechanicalSystemType);
            else if(mechanicalSystemType is CoolingSystemType)
                result = new CoolingSystem(name, (CoolingSystemType)mechanicalSystemType);

            if (result == null)
                return null;

            return result;
        }
    }
}