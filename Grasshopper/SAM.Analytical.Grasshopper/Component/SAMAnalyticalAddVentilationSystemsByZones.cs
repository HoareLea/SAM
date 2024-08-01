using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddVentilationSystemsByZones : GH_SAMVariableOutputParameterComponent
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
        public SAMAnalyticalAddVentilationSystemsByZones()
          : base("SAMAnalytical.AddVentilationSystemsByZones", "SAMAnalytical.AddVentilationSystemsByZones",
              "Add VentilationSystems to SAM Analytical Model",
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
                result.Add(new GH_SAMParam(new GooSystemParam() { Name = "ventilationSystems", NickName = "ventilationSystems", Description = "SAM Ventilation Systems", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalEquipmentParam() { Name = "aHUs", NickName = "aHUs", Description = "SAM Mechanical Air Handling Units", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
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

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

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

            List<MechanicalSystem> mechanicalSystems = new List<MechanicalSystem>();
            DataTree<IAnalyticalEquipment> dataTree_AnalyticalEquipments = new DataTree<IAnalyticalEquipment>();

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

                    if(supplyUnitNames.Count > i)
                    {
                        supplyUnitName = supplyUnitNames[i];
                    }

                    if (exhaustUnitNames.Count > i)
                    {
                        exhaustUnitName = exhaustUnitNames[i];
                    }

                    VentilationSystem  ventilationSystem = adjacencyCluster.AddVentilationSystem(systemTypeLibrary, spaces, supplyUnitName, exhaustUnitName);
                    if (ventilationSystem != null)
                    {
                        if(mechanicalSystems.Find(x => x.Guid == ventilationSystem.Guid) == null)
                        {
                            mechanicalSystems.Add(ventilationSystem);
                        }

                        string[] names = new string[] { ventilationSystem.GetValue<string>(VentilationSystemParameter.ExhaustUnitName), ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName) };

                        List<AirHandlingUnit> airHandlingUnits = new List<AirHandlingUnit>();
                        foreach (string name in names)
                        {
                            if (string.IsNullOrEmpty(name))
                            {
                                continue;
                            }

                            AirHandlingUnit airHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(x => x.Name == name);
                            if (airHandlingUnit != null)
                            {
                                airHandlingUnits.Add(airHandlingUnit);
                            }
                        }

                        if(airHandlingUnits != null && airHandlingUnits.Count != 0)
                        {
                            GH_Path path = new GH_Path(i);
                            airHandlingUnits.ForEach(x => dataTree_AnalyticalEquipments.Add(x, path));
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

            index = Params.IndexOfOutputParam("ventilationSystems");
            if (index != -1)
            {
                dataAccess.SetDataList(index, mechanicalSystems);
            }

            index = Params.IndexOfOutputParam("aHUs");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree_AnalyticalEquipments);
            }
        }
    }
}