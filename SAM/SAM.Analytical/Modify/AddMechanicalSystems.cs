using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<MechanicalSystem> AddMechanicalSystems(this AdjacencyCluster adjacencyCluster, Core.SystemTypeLibrary systemTypeLibrary, IEnumerable<Space> spaces = null, string supplyUnitName = null, string exhaustUnitName = null)
        {
            if (adjacencyCluster == null || systemTypeLibrary == null)
                return null;

            List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                foreach (Space space in spaces)
                    if (space != null)
                        spaces_Temp.RemoveAll(x => x.Guid == space.Guid);
            }

            Dictionary<VentilationSystemType, List<Space>> dictionary_Ventilation = new Dictionary<VentilationSystemType, List<Space>>();
            Dictionary<CoolingSystemType, List<Space>> dictionary_Cooling = new Dictionary<CoolingSystemType, List<Space>>();
            Dictionary<HeatingSystemType, List<Space>> dictionary_Heating = new Dictionary<HeatingSystemType, List<Space>>();
            foreach (Space space in spaces_Temp)
            {
                InternalCondition internalCondition = space.InternalCondition;
                if (internalCondition == null)
                    continue;

                VentilationSystemType ventilationSystemType = internalCondition.GetSystemType<VentilationSystemType>(systemTypeLibrary);
                if(ventilationSystemType != null)
                {
                    List<Space> spaces_SystemType = null;
                    if (!dictionary_Ventilation.TryGetValue(ventilationSystemType, out spaces_SystemType))
                    {
                        spaces_SystemType = new List<Space>();
                        dictionary_Ventilation[ventilationSystemType] = spaces_SystemType;
                    }

                    spaces_SystemType.Add(space);
                }

                CoolingSystemType coolingSystemType = internalCondition.GetSystemType<CoolingSystemType>(systemTypeLibrary);
                if (coolingSystemType != null)
                {
                    List<Space> spaces_SystemType = null;
                    if (!dictionary_Cooling.TryGetValue(coolingSystemType, out spaces_SystemType))
                    {
                        spaces_SystemType = new List<Space>();
                        dictionary_Cooling[coolingSystemType] = spaces_SystemType;
                    }

                    spaces_SystemType.Add(space);
                }

                HeatingSystemType heatingSystemType = internalCondition.GetSystemType<HeatingSystemType>(systemTypeLibrary);
                if (heatingSystemType != null)
                {
                    List<Space> spaces_SystemType = null;
                    if (!dictionary_Heating.TryGetValue(heatingSystemType, out spaces_SystemType))
                    {
                        spaces_SystemType = new List<Space>();
                        dictionary_Heating[heatingSystemType] = spaces_SystemType;
                    }

                    spaces_SystemType.Add(space);
                }

            }

            List<MechanicalSystem> result = new List<MechanicalSystem>();
            int index;

            index = 1;
            foreach(KeyValuePair<VentilationSystemType, List<Space>> keyValuePair in dictionary_Ventilation)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(keyValuePair.Key, index, keyValuePair.Value, supplyUnitName, exhaustUnitName);
                if (mechanicalSystem == null)
                    continue;

                result.Add(mechanicalSystem);
                index++;
            }

            index = 1;
            foreach (KeyValuePair<CoolingSystemType, List<Space>> keyValuePair in dictionary_Cooling)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(keyValuePair.Key, index, keyValuePair.Value, supplyUnitName, exhaustUnitName);
                if (mechanicalSystem == null)
                    continue;

                result.Add(mechanicalSystem);
                index++;
            }

            index = 1;
            foreach (KeyValuePair<HeatingSystemType, List<Space>> keyValuePair in dictionary_Heating)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(keyValuePair.Key, index, keyValuePair.Value, supplyUnitName, exhaustUnitName);
                if (mechanicalSystem == null)
                    continue;

                result.Add(mechanicalSystem);
                index++;
            }

            return result;
        }
    }
}