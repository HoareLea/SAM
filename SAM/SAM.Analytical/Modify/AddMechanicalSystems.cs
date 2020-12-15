using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<MechanicalSystem> AddMechanicalSystems(this AdjacencyCluster adjacencyCluster, Core.SystemTypeLibrary systemTypeLibrary, IEnumerable<Space> spaces = null, string supplyUnitName = null, string exhaustUnitName = null, string ventilationRiserName = null, string heatingRiserName = null, string coolingRiserName = null)
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

            Dictionary<System.Guid, MechanicalSystemType> dictionary_MechanicalSystemType = new Dictionary<System.Guid, MechanicalSystemType>();
            
            Dictionary<System.Guid, List<Space>> dictionary_Ventilation = new Dictionary<System.Guid, List<Space>>();
            Dictionary<System.Guid, List<Space>> dictionary_Cooling = new Dictionary<System.Guid, List<Space>>();
            Dictionary<System.Guid, List<Space>> dictionary_Heating = new Dictionary<System.Guid, List<Space>>();
            foreach (Space space in spaces_Temp)
            {
                InternalCondition internalCondition = space.InternalCondition;
                if (internalCondition == null)
                    continue;

                VentilationSystemType ventilationSystemType = internalCondition.GetSystemType<VentilationSystemType>(systemTypeLibrary);
                if(ventilationSystemType != null)
                {
                    List<Space> spaces_SystemType = null;
                    if (!dictionary_Ventilation.TryGetValue(ventilationSystemType.Guid, out spaces_SystemType))
                    {
                        spaces_SystemType = new List<Space>();
                        dictionary_Ventilation[ventilationSystemType.Guid] = spaces_SystemType;
                        dictionary_MechanicalSystemType[ventilationSystemType.Guid] = ventilationSystemType;
                    }

                    spaces_SystemType.Add(space);
                }

                CoolingSystemType coolingSystemType = internalCondition.GetSystemType<CoolingSystemType>(systemTypeLibrary);
                if (coolingSystemType != null)
                {
                    List<Space> spaces_SystemType = null;
                    if (!dictionary_Cooling.TryGetValue(coolingSystemType.Guid, out spaces_SystemType))
                    {
                        spaces_SystemType = new List<Space>();
                        dictionary_Cooling[coolingSystemType.Guid] = spaces_SystemType;
                        dictionary_MechanicalSystemType[coolingSystemType.Guid] = coolingSystemType;
                    }

                    spaces_SystemType.Add(space);
                }

                HeatingSystemType heatingSystemType = internalCondition.GetSystemType<HeatingSystemType>(systemTypeLibrary);
                if (heatingSystemType != null)
                {
                    List<Space> spaces_SystemType = null;
                    if (!dictionary_Heating.TryGetValue(heatingSystemType.Guid, out spaces_SystemType))
                    {
                        spaces_SystemType = new List<Space>();
                        dictionary_Heating[heatingSystemType.Guid] = spaces_SystemType;
                        dictionary_MechanicalSystemType[heatingSystemType.Guid] = heatingSystemType;
                    }

                    spaces_SystemType.Add(space);
                }

            }

            List<MechanicalSystem> result = new List<MechanicalSystem>();
            int index;

            index = 1;
            foreach(KeyValuePair<System.Guid, List<Space>> keyValuePair in dictionary_Ventilation)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddVentilationSystem(dictionary_MechanicalSystemType[keyValuePair.Key] as VentilationSystemType, index, keyValuePair.Value, supplyUnitName, exhaustUnitName);
                if (mechanicalSystem == null)
                    continue;

                if(ventilationRiserName != null)
                {
                    foreach(Space space in keyValuePair.Value)
                    {
                        space.SetValue(SpaceParameter.VentilationRiserName, ventilationRiserName);
                        adjacencyCluster.AddObject(space);
                    }
                }

                result.Add(mechanicalSystem);
                index++;
            }

            index = 1;
            foreach (KeyValuePair<System.Guid, List<Space>> keyValuePair in dictionary_Cooling)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(dictionary_MechanicalSystemType[keyValuePair.Key], index, keyValuePair.Value);
                if (mechanicalSystem == null)
                    continue;

                if (coolingRiserName != null)
                {
                    foreach (Space space in keyValuePair.Value)
                    {
                        space.SetValue(SpaceParameter.CoolingRiserName, coolingRiserName);
                        adjacencyCluster.AddObject(space);
                    }
                }

                result.Add(mechanicalSystem);
                index++;
            }

            index = 1;
            foreach (KeyValuePair<System.Guid, List<Space>> keyValuePair in dictionary_Heating)
            {
                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(dictionary_MechanicalSystemType[keyValuePair.Key], index, keyValuePair.Value);
                if (mechanicalSystem == null)
                    continue;

                if (heatingRiserName != null)
                {
                    foreach (Space space in keyValuePair.Value)
                    {
                        space.SetValue(SpaceParameter.HeatingRiserName, heatingRiserName);
                        adjacencyCluster.AddObject(space);
                    }
                }

                result.Add(mechanicalSystem);
                index++;
            }

            return result;
        }
    }
}