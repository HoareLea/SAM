namespace SAM.Analytical
{
    public static partial class Create
    {
        public static MechanicalSystem MechanicalSystem(MechanicalSystemType mechanicalSystemType, int index = -1)
        {
            if (mechanicalSystemType == null)
                return null;

            string id = null;
            if (index != -1)
            {
                id = index.ToString();
            }

            MechanicalSystem result = null;
            if(mechanicalSystemType is VentilationSystemType)
                result = new VentilationSystem(id, (VentilationSystemType)mechanicalSystemType);
            else if(mechanicalSystemType is HeatingSystemType)
                result = new HeatingSystem(id, (HeatingSystemType)mechanicalSystemType);
            else if(mechanicalSystemType is CoolingSystemType)
                result = new CoolingSystem(id, (CoolingSystemType)mechanicalSystemType);

            if (result == null)
                return null;

            return result;
        }
    }
}