using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static VentilationSystem AddVentilationSystem(this AdjacencyCluster adjacencyCluster, VentilationSystemType ventilationSystemType, IEnumerable<Space> spaces = null, string supplyUnitName = null, string exhaustUnitName = null)
        {
            if (adjacencyCluster == null || ventilationSystemType == null)
                return null;

            VentilationSystem ventilationSystem = AddMechanicalSystem(adjacencyCluster, ventilationSystemType, spaces) as VentilationSystem;
            if (ventilationSystem == null)
                return null;

            if (supplyUnitName != null)
                ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, supplyUnitName);

            if (exhaustUnitName != null)
                ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, exhaustUnitName);

            adjacencyCluster.AddObject(ventilationSystem);

            return ventilationSystem;
        }

        public static VentilationSystem AddVentilationSystem(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, VentilationSystemType ventilationSystemType, IEnumerable<Space> spaces = null, bool allowMultipleSystems = false)
        {
            if (adjacencyCluster == null || ventilationSystemType == null || airHandlingUnit == null)
            {
                return null;
            }

            VentilationSystem ventilationSystem = AddMechanicalSystem(adjacencyCluster, ventilationSystemType, spaces, allowMultipleSystems) as VentilationSystem;
            if (ventilationSystem == null)
            {
                return null;
            }

            adjacencyCluster.AddObject(airHandlingUnit);

            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, airHandlingUnit.Name);

            adjacencyCluster.AddObject(ventilationSystem);

            return ventilationSystem;
        }

        public static VentilationSystem AddVentilationSystem(this AdjacencyCluster adjacencyCluster, SystemTypeLibrary systemTypeLibrary, IEnumerable<Space> spaces, string supplyUnitName = null, string exhaustUnitName = null, bool allowMultipleSystems = false)
        {
            if (adjacencyCluster == null || systemTypeLibrary == null || spaces == null || spaces.Count() == 0)
            {
                return null;
            }


            InternalCondition internalCondition = spaces.ElementAt(0).InternalCondition;
            if (internalCondition == null)
            {
                return null;
            }
                
            VentilationSystemType ventilationSystemType = internalCondition.GetSystemType<VentilationSystemType>(systemTypeLibrary);
            string systemTypeName = ventilationSystemType?.Name;
            if (string.IsNullOrWhiteSpace(systemTypeName))
            {
                return null;
            }

            string supplyUnitName_Temp = supplyUnitName;
            string exhaustUnitName_Temp = exhaustUnitName;

            if (systemTypeName.Equals("NV") || systemTypeName.Equals("UV"))
            {
                supplyUnitName_Temp = null;
                exhaustUnitName_Temp = null;
            }

            if (systemTypeName.Equals("EOC") || systemTypeName.Equals("EOL"))
            {
                supplyUnitName_Temp = null;
            }

            VentilationSystem result = null;

            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetMechanicalSystems<VentilationSystem>();
            if (ventilationSystems != null && ventilationSystems.Count != 0)
            {
                ventilationSystems = ventilationSystems.FindAll(x => x.Type?.Name == systemTypeName);
                if(ventilationSystems != null && ventilationSystems.Count != 0)
                {
                    for (int i = ventilationSystems.Count - 1; i >= 0; i--)
                    {
                        VentilationSystem ventilationSystem_Temp = ventilationSystems[i];

                        if(!string.IsNullOrWhiteSpace(supplyUnitName_Temp))
                        {
                            string unitName = ventilationSystem_Temp.GetValue<string>(VentilationSystemParameter.SupplyUnitName);
                            if(unitName != supplyUnitName_Temp)
                            {
                                ventilationSystems.RemoveAt(i);
                                continue;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(exhaustUnitName_Temp))
                        {
                            string unitName = ventilationSystem_Temp.GetValue<string>(VentilationSystemParameter.ExhaustUnitName);
                            if (unitName != exhaustUnitName_Temp)
                            {
                                ventilationSystems.RemoveAt(i);
                                continue;
                            }
                        }
                    }
                }

                if(ventilationSystems != null && ventilationSystems.Count != 0)
                {
                    result = ventilationSystems[0];
                }
            }

            if(result != null)
            {
                adjacencyCluster.AssignMechanicalSystem(result, spaces, allowMultipleSystems);
            }
            else
            {
                result = adjacencyCluster.AddMechanicalSystem(ventilationSystemType, spaces, allowMultipleSystems) as VentilationSystem;
                if(result != null)
                {
                    //Supply Air Handling Unit
                    if (!string.IsNullOrWhiteSpace(supplyUnitName_Temp))
                    {
                        result.SetValue(VentilationSystemParameter.SupplyUnitName, supplyUnitName_Temp);

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

                    //Exhaust Air Handling Unit
                    if (!string.IsNullOrWhiteSpace(exhaustUnitName_Temp))
                    {
                        result.SetValue(VentilationSystemParameter.ExhaustUnitName, exhaustUnitName_Temp);

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
                }
            }

            return result;
        }
    }
}