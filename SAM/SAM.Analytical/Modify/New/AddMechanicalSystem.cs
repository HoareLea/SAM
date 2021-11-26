using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static MechanicalSystem AddMechanicalSystem(this BuildingModel buildingModel, MechanicalSystemType mechanicalSystemType, int index = -1, IEnumerable<Space> spaces = null)
        {
            if (buildingModel == null || mechanicalSystemType == null)
            {
                return null;
            }

            MechanicalSystem mechanicalSystem = Create.MechanicalSystem(mechanicalSystemType, index);
            if (mechanicalSystem == null)
            {
                return null;
            }

            if(!buildingModel.Add(mechanicalSystem, spaces))
            {
                return null;
            }

            return mechanicalSystem;
        }
    }
}