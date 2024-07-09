using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddVentilationSystem : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("44cad435-1777-4b53-a8cc-feef84862c3e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddVentilationSystem()
          : base("SAMAnalytical.AddVentilationSystem", "SAMAnalytical.AddVentilationSystem",
              "Add VentilationSystem to SAM Analytical Model",
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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAirHandlingUnitParam() { Name = "_aHU", NickName = "_aHU", Description = "SAM Analytical Air Handling Unit", Access = GH_ParamAccess.item}, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "_ventilationSystemType_", NickName = "_ventilationSystemType_", Description = "SAM VentilationSystemType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new GooSystemTypeLibraryParam() { Name = "_systemTypeLibrary_", NickName = "_systemTypeLibrary_", Description = "SAM SystemTypeLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_removeAssignments_", NickName = "_removeAssignments_", Description = "All existing spaces' ventilation system type assignments will be removed, and new assignments will be adopted.", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooSystemParam() { Name = "ventilationSystem", NickName = "ventilationSystem", Description = "SAM Ventilation System", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAirHandlingUnitParam() { Name = "aHU", NickName = "aHU", Description = "SAM Air HAndling Unit", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                index = Params.IndexOfOutputParam("analytical");
                if (index != -1)
                {
                    dataAccess.SetData(index, analyticalObject);
                }

                return;
            }

            index = Params.IndexOfInputParam("_aHU");
            AirHandlingUnit airHandlingUnit = null;
            if (!dataAccess.GetData(index, ref airHandlingUnit) || airHandlingUnit == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_ventilationSystemType_");
            VentilationSystemType ventilationSystemType = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref ventilationSystemType);
            }

            if(ventilationSystemType == null)
            {
                index = Params.IndexOfInputParam("_systemTypeLibrary_");
                SystemTypeLibrary systemTypeLibrary = null;
                if (index != -1)
                {
                    dataAccess.GetData(index, ref systemTypeLibrary);
                }

                if(systemTypeLibrary == null)
                {
                    systemTypeLibrary = ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);
                }

                ventilationSystemType = systemTypeLibrary?.GetSystemTypes<VentilationSystemType>().FirstOrDefault();
            }

            if (ventilationSystemType == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_spaces_");
            List<Space> spaces = new List<Space>();
            if (index == -1 || !dataAccess.GetDataList(index, spaces))
            {
                spaces = null;
            }

            if(spaces == null)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            bool allowMultipleSystems = false;
            index = Params.IndexOfInputParam("_removeAssignments_");
            if (index == -1 || !dataAccess.GetData(index, ref allowMultipleSystems))
            {
                allowMultipleSystems = false;
            }


            VentilationSystem ventilationSystem = adjacencyCluster.AddVentilationSystem(airHandlingUnit, ventilationSystemType, spaces, allowMultipleSystems);

            if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }

            index = Params.IndexOfOutputParam("analytical");
            if(index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("ventilationSystem");
            if (index != -1)
            {
                dataAccess.SetData(index, ventilationSystem);
            }

            index = Params.IndexOfOutputParam("aHU");
            if (index != -1)
            {
                dataAccess.SetData(index, airHandlingUnit);
            }
        }
    }
}