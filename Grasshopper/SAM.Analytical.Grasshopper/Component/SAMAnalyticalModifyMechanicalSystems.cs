using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyMechanicalSystems : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d9266a28-5770-469d-ae06-bea164ef2ce5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalModifyMechanicalSystems()
          : base("SAMAnalytical.ModifyMechanicalSystems", "SAMAnalytical.ModifyMechanicalSystems",
              "Modify MechanicalSystems to SAM Analytical Model",
              "SAM", "Analytical02")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analytical", "_analytical", "SAM Analytical", GH_ParamAccess.item);

            GooSystemTypeLibraryParam gooSystemTypeLibraryParam = new GooSystemTypeLibraryParam();
            gooSystemTypeLibraryParam.Optional = true;
            inputParamManager.AddParameter(gooSystemTypeLibraryParam, "_systemTypeLibrary_", "_systemTypeLibrary_", "SAM SystemTypeLibrary", GH_ParamAccess.item);

            GooSpaceParam gooSpaceParam = new GooSpaceParam();
            gooSpaceParam.Optional = true;
            inputParamManager.AddParameter(gooSpaceParam, "_spaces_", "_spaces_", "SAM Analytical Spaces", GH_ParamAccess.list);

            int index = -1;

            index = inputParamManager.AddTextParameter("supplyUnitName_", "_upplyUnitName", "Supply Unit Name", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddTextParameter("exhaustUnitName_", "exhaustUnitName_", "Exhaust Unit Name", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooSystemTypeParam(), "coolingSystemType_", "coolingSystemType_", "Cooling System Type", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooSystemTypeParam(), "heatingSystemType_", "heatingSystemType_", "Heating System Type", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooSystemTypeParam(), "ventilationSystemType_", "ventilationSystemType_", "Ventilation System Type", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "analytical", "analytical", "SAM Analytical", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooSystemParam(), "mechanicalSystems", "mechanicalSystems", "mechanicalSystems", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            IAnalyticalObject analyticalObject = null;
            if(!dataAccess.GetData(0, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SystemTypeLibrary systemTypeLibrary = null;
            dataAccess.GetData(1, ref systemTypeLibrary);

            if (systemTypeLibrary == null)
                systemTypeLibrary = ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);

            List<Space> spaces = new List<Space>();
            if (!dataAccess.GetDataList(2, spaces))
                spaces = null;

            string supplyUnitName = null;
            dataAccess.GetData(3, ref supplyUnitName);

            string exhaustUnitName = null;
            dataAccess.GetData(4, ref exhaustUnitName);

            ISystemType coolingSystemType = null;
            dataAccess.GetData(5, ref coolingSystemType);

            ISystemType heatingSystemType = null;
            dataAccess.GetData(6, ref heatingSystemType);

            ISystemType ventilationSystemType = null;
            dataAccess.GetData(7, ref ventilationSystemType);

            List<MechanicalSystem> mechanicalSystems = new List<MechanicalSystem>();

            AdjacencyCluster adjacencyCluster = null;
            if(analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            if(spaces == null || spaces.Count == 0)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            if(spaces != null)
            {
                if (ventilationSystemType != null)
                {
                    MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(ventilationSystemType as VentilationSystemType, spaces);
                    if(mechanicalSystem != null)
                    {
                        mechanicalSystems.Add(mechanicalSystem);
                    }
                }

                if (heatingSystemType != null)
                {
                    MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(heatingSystemType as HeatingSystemType, spaces);
                    if (mechanicalSystem != null)
                    {
                        mechanicalSystems.Add(mechanicalSystem);
                    }
                }

                if (coolingSystemType != null)
                {
                    MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(coolingSystemType as CoolingSystemType, spaces);
                    if (mechanicalSystem != null)
                    {
                        mechanicalSystems.Add(mechanicalSystem);
                    }
                }

                if(!string.IsNullOrWhiteSpace(supplyUnitName) || !string.IsNullOrWhiteSpace(exhaustUnitName))
                {
                    VentilationSystem ventilationSystem = null;
                    foreach(Space space in spaces)
                    {
                        List<VentilationSystem> ventilationSystems = adjacencyCluster.GetRelatedObjects<VentilationSystem>(space);
                        if(ventilationSystems != null && ventilationSystems.Count != 0)
                        {
                            foreach(VentilationSystem ventilationSystem_Temp in ventilationSystems)
                            {
                                if(ventilationSystem_Temp != null)
                                {
                                    ventilationSystem = ventilationSystem_Temp;
                                    break;
                                }
                            }

                            if(ventilationSystem != null)
                            {
                                break;
                            }
                        }

                    }

                    if(ventilationSystem == null)
                    {
                        ventilationSystem = adjacencyCluster.AddMechanicalSystem(systemTypeLibrary?.GetSystemTypes<VentilationSystemType>().FirstOrDefault(), spaces) as VentilationSystem;
                    }

                    if(ventilationSystem != null)
                    {
                        if(!string.IsNullOrWhiteSpace(supplyUnitName))
                        {
                            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, supplyUnitName);
                        }

                        if (!string.IsNullOrWhiteSpace(exhaustUnitName))
                        {
                            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, exhaustUnitName);
                        }

                        adjacencyCluster.AddObject(ventilationSystem);
                    }
                }
            }

            if(analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if(analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }

            dataAccess.SetData(0, analyticalObject);
            dataAccess.SetDataList(1, mechanicalSystems);
        }
    }
}