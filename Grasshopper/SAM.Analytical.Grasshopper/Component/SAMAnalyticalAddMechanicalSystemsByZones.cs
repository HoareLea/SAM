using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddMechanicalSystemsByZones : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ec8cb63a-71d2-44e1-9d22-1eb82811803b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddMechanicalSystemsByZones()
          : base("SAMAnalytical.AddMechanicalSystemsByZones", "SAMAnalytical.AddMechanicalSystemsByZones",
              "Add MechanicalSystems to SAM Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemTypeLibraryParam() { Name = "_systemTypeLibrary_", NickName = "_systemTypeLibrary_", Description = "SAM SystemTypeLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_zones_", NickName = "_zones_", Description = "SAM Analytical Zones", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_supplyUnitNames_", NickName = "_supplyUnitNames_", Description = "Supply Unit Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_exhaustUnitNames_", NickName = "_exhaustUnitNames_", Description = "Exhaust Unit Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_ventilationRiserNames_", NickName = "_ventilationRiserNames_", Description = "Ventilation Riser Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_heatingRiserNames_", NickName = "_heatingRiserNames_", Description = "Heating Riser Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_coolingRiserNames_", NickName = "_coolingRiserNames_", Description = "Cooling Riser Names", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemParam() { Name = "mechanicalSystems", NickName = "mechanicalSystems", Description = "SAM Mechanical Systems", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalEquipmentParam() { Name = "equipments", NickName = "equipments", Description = "SAM Mechanical Equipments", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_systemTypeLibrary_");
            SystemTypeLibrary systemTypeLibrary = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref systemTypeLibrary);
            }

            if (systemTypeLibrary == null)
            {
                systemTypeLibrary = ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);
            }

            index = Params.IndexOfInputParam("_zones_");
            List<Zone> zones = new List<Zone>();

            List<object> objects = new List<object>();
            if (index == -1 || !dataAccess.GetDataList(index, objects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            foreach(object @object in objects)
            {
                object object_Temp = @object;
                if(@object is global::Grasshopper.Kernel.Types.IGH_Goo)
                {
                    object_Temp = ((dynamic)@object).Value;
                }
                
                if(object_Temp is Zone)
                {
                    zones.Add((Zone)object_Temp);
                }
                else if(object_Temp is string)
                {
                    Zone zone = adjacencyCluster.GetZones()?.Find(x => x.Name == (string)object_Temp);
                    if(zone != null)
                    {
                        zones.Add((Zone)object_Temp);
                    }
                }
            }

            index = Params.IndexOfInputParam("_supplyUnitNames_");
            List<string> supplyUnitNames = new List<string>();
            if(index != -1)
            {
                dataAccess.GetDataList(index, supplyUnitNames);
            }

            index = Params.IndexOfInputParam("_exhaustUnitNames_");
            List<string> exhaustUnitNames = new List<string>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, exhaustUnitNames);
            }

            index = Params.IndexOfInputParam("_ventilationRiserNames_");
            List<string> ventilationRiserNames = new List<string>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, ventilationRiserNames);
            }

            index = Params.IndexOfInputParam("_heatingRiserNames_");
            List<string> heatingRiserNames = new List<string>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, heatingRiserNames);
            }

            index = Params.IndexOfInputParam("_coolingRiserNames_");
            List<string> coolingRiserNames = new List<string>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, coolingRiserNames);
            }

            List<MechanicalSystem> mechanicalSystems = new List<MechanicalSystem>();
            List<IAnalyticalEquipment> analyticalEquipments = new List<IAnalyticalEquipment>();

            if (adjacencyCluster != null)
            {
                for(int i =0; i < zones.Count; i++)
                {
                    Zone zone = zones[i];

                    List<Space> spaces = adjacencyCluster.GetSpaces(zone);
                    if(spaces == null || spaces.Count == 0)
                    {
                        continue;
                    }

                    string supplyUnitName = null;
                    string exhaustUnitName = null;
                    string ventilationRiserName = null;
                    string heatingRiserName = null;
                    string coolingRiserName = null;

                    if(supplyUnitNames.Count > i)
                    {
                        supplyUnitName = supplyUnitNames[i];
                    }

                    if (exhaustUnitNames.Count > i)
                    {
                        exhaustUnitName = exhaustUnitNames[i];
                    }

                    if (ventilationRiserNames.Count > i)
                    {
                        ventilationRiserName = ventilationRiserNames[i];
                    }

                    if (heatingRiserNames.Count > i)
                    {
                        heatingRiserName = heatingRiserNames[i];
                    }

                    if (coolingRiserNames.Count > i)
                    {
                        coolingRiserName = coolingRiserNames[i];
                    }

                    List<MechanicalSystem>  mechanicalSystems_Temp = adjacencyCluster.AddMechanicalSystems(systemTypeLibrary, spaces, supplyUnitName, exhaustUnitName, ventilationRiserName, heatingRiserName, coolingRiserName);
                    if (mechanicalSystems_Temp != null && mechanicalSystems_Temp.Count > 0)
                    {
                        foreach (MechanicalSystem mechanicalSystem in mechanicalSystems_Temp)
                        {
                            mechanicalSystems.Add(mechanicalSystem);

                            if (!(mechanicalSystem is VentilationSystem))
                            {
                                continue;
                            }

                            string[] names = new string[] { mechanicalSystem.GetValue<string>(VentilationSystemParameter.ExhaustUnitName), mechanicalSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName) };

                            foreach (string name in names)
                            {
                                if (string.IsNullOrEmpty(name))
                                {
                                    continue;
                                }

                                AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(x => x.Name == name);
                                if (airHandlingUnit != null)
                                {
                                    if (analyticalEquipments.Find(x => airHandlingUnit.Guid == (x as dynamic).Guid) == null)
                                    {
                                        analyticalEquipments.Add(airHandlingUnit);
                                    }
                                }
                            }
                        }
                    }
                }

                if (analyticalObject is AnalyticalModel)
                {
                    analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
                }
                else if (analyticalObject is AdjacencyCluster)
                {
                    analyticalObject = adjacencyCluster;
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if(index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("mechanicalSystems");
            if (index != -1)
            {
                dataAccess.SetDataList(index, mechanicalSystems);
            }

            index = Params.IndexOfOutputParam("equipments");
            if (index != -1)
            {
                dataAccess.SetDataList(index, analyticalEquipments);
            }
        }
    }
}