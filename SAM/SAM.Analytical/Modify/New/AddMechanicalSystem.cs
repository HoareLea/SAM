using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static MechanicalSystem AddMechanicalSystem(this ArchitecturalModel architecturalModel, MechanicalSystemType mechanicalSystemType, int index = -1, IEnumerable<Space> spaces = null)
        {
            if (architecturalModel == null || mechanicalSystemType == null)
            {
                return null;
            }

            MechanicalSystem mechanicalSystem = Create.MechanicalSystem(mechanicalSystemType, index);
            if (mechanicalSystem == null)
            {
                return null;
            }

            if(!architecturalModel.Add(mechanicalSystem, spaces))
            {
                return null;
            }

            return mechanicalSystem;
        }
    }
}