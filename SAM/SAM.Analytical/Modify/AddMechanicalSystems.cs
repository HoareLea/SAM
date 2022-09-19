using System.Collections.Generic;
using System.Linq;

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
                List<Space> spaces_Filtered = new List<Space>();
                foreach (Space space in spaces)
                {
                    if (space == null)
                        continue;

                    Space space_Filtered = spaces_Temp.Find(x => x.Guid == space.Guid);
                    if (spaces_Filtered == null)
                        continue;

                    spaces_Filtered.Add(space_Filtered);
                }

                spaces_Temp = spaces_Filtered;
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
            foreach(KeyValuePair<System.Guid, List<Space>> keyValuePair in dictionary_Ventilation)
            {
                VentilationSystemType ventilationSystemType = dictionary_MechanicalSystemType[keyValuePair.Key] as VentilationSystemType;
                if (ventilationSystemType == null)
                    continue;

                string supplyUnitName_Temp = supplyUnitName;
                string exhaustUnitName_Temp = exhaustUnitName;

                string name = ventilationSystemType.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (name.Equals("NV") || name.Equals("UV"))
                    {
                        supplyUnitName_Temp = null;
                        exhaustUnitName_Temp = null;
                    }
                    
                    if(name.Equals("EOC") || name.Equals("EOL"))
                    {
                        supplyUnitName_Temp = null;
                    }
                }

                MechanicalSystem mechanicalSystem = adjacencyCluster.AddVentilationSystem(ventilationSystemType, keyValuePair.Value, supplyUnitName_Temp, exhaustUnitName_Temp);
                if (mechanicalSystem == null)
                    continue;

                if(ventilationRiserName != null)
                {
                    if(!string.IsNullOrWhiteSpace(name) && !name.Equals("NV") && !name.Equals("UV"))
                    {
                        foreach (Space space in keyValuePair.Value)
                        {
                            space.SetValue(SpaceParameter.VentilationRiserName, ventilationRiserName);
                            adjacencyCluster.AddObject(space);
                        }
                    }
                }

                //Add Supply Air Handling Unit
                if (!string.IsNullOrWhiteSpace(supplyUnitName_Temp))
                {
                    AirHandlingUnit airHandlingUnit = adjacencyCluster?.GetObjects((AirHandlingUnit x) => x.Name == supplyUnitName_Temp)?.FirstOrDefault();
                    if (airHandlingUnit == null)
                    {
                        airHandlingUnit = Create.AirHandlingUnit(supplyUnitName_Temp);
                    }

                    if (airHandlingUnit != null)
                    {
                        adjacencyCluster.AddObject(airHandlingUnit);
                    }
                }

                //Add Exhaust Air Handling Unit
                if (!string.IsNullOrWhiteSpace(exhaustUnitName_Temp))
                {
                    AirHandlingUnit airHandlingUnit = adjacencyCluster?.GetObjects((AirHandlingUnit x) => x.Name == exhaustUnitName_Temp)?.FirstOrDefault();
                    if (airHandlingUnit == null)
                    {
                        airHandlingUnit = Create.AirHandlingUnit(exhaustUnitName_Temp);
                    }

                    if (airHandlingUnit != null)
                    {
                        adjacencyCluster.AddObject(airHandlingUnit);
                    }
                }

                result.Add(mechanicalSystem);
            }

            foreach (KeyValuePair<System.Guid, List<Space>> keyValuePair in dictionary_Cooling)
            {
                MechanicalSystemType mechanicalSystemType = dictionary_MechanicalSystemType[keyValuePair.Key];
                if (mechanicalSystemType == null)
                    continue;

                string name = mechanicalSystemType.Name;

                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(mechanicalSystemType, keyValuePair.Value);
                if (mechanicalSystem == null)
                    continue;

                if (coolingRiserName != null)
                {
                    if (!string.IsNullOrWhiteSpace(name) && !name.Equals("AHU") && !name.Equals("UC"))
                    {
                        foreach (Space space in keyValuePair.Value)
                        {
                            space.SetValue(SpaceParameter.CoolingRiserName, coolingRiserName);
                            adjacencyCluster.AddObject(space);
                        }
                    }
                }

                result.Add(mechanicalSystem);
            }

            foreach (KeyValuePair<System.Guid, List<Space>> keyValuePair in dictionary_Heating)
            {
                MechanicalSystemType mechanicalSystemType = dictionary_MechanicalSystemType[keyValuePair.Key];
                if (mechanicalSystemType == null)
                    continue;

                string name = mechanicalSystemType.Name;

                MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(mechanicalSystemType, keyValuePair.Value);
                if (mechanicalSystem == null)
                    continue;

                if (heatingRiserName != null)
                {
                    if (!string.IsNullOrWhiteSpace(name) && !name.Equals("AHU") && !name.Equals("UH"))
                    {
                        foreach (Space space in keyValuePair.Value)
                        {
                            space.SetValue(SpaceParameter.HeatingRiserName, heatingRiserName);
                            adjacencyCluster.AddObject(space);
                        }
                    }
                }

                result.Add(mechanicalSystem);
            }

            return result;
        }
    }
}