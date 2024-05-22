using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddMechanicalSystems : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("63bb61dc-69b7-4b0f-b363-482ba95ed20d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.5";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddMechanicalSystems()
          : base("SAMAnalytical.AddMechanicalSystems", "SAMAnalytical.AddMechanicalSystems",
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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_supplyUnitName_", NickName = "_supplyUnitName_", Description = "Supply Unit Name", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("AHU1");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_exhaustUnitName_", NickName = "_exhaustUnitName_", Description = "Exhaust Unit Name", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("AHU1");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_ventilationRiserName_", NickName = "_ventilationRiserName_", Description = "Ventilation Riser Name", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("RV1");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_heatingRiserName_", NickName = "_heatingRiserName_", Description = "Heating Riser Name", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("RH1");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_coolingRiserName_", NickName = "_coolingRiserName_", Description = "Cooling Riser Name", Access = GH_ParamAccess.item, Optional = true };
                param_String.SetPersistentData("RC1");
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
                result.Add(new GH_SAMParam(new GooSystemParam() { Name = "equipments", NickName = "equipments", Description = "SAM Mechanical Equipments", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_spaces_");
            List<Space> spaces = new List<Space>();
            if (index == -1 || !dataAccess.GetDataList(index, spaces))
            {
                spaces = null;
            }

            index = Params.IndexOfInputParam("_supplyUnitName_");
            string supplyUnitName = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref supplyUnitName);
            }

            if (string.IsNullOrEmpty(supplyUnitName))
            {
                supplyUnitName = "AHU1";
            }

            index = Params.IndexOfInputParam("_exhaustUnitName_");
            string exhaustUnitName = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref exhaustUnitName);
            }

            if (string.IsNullOrEmpty(exhaustUnitName))
            {
                exhaustUnitName = "AHU1";
            }

            index = Params.IndexOfInputParam("_ventilationRiserName_");
            string ventilationRiserName = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref ventilationRiserName);
            }

            if (string.IsNullOrEmpty(ventilationRiserName))
            {
                ventilationRiserName = "RV1";
            }

            index = Params.IndexOfInputParam("_heatingRiserName_");
            string heatingRiserName = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref heatingRiserName);
            }

            if (string.IsNullOrEmpty(heatingRiserName))
            {
                heatingRiserName = "RH1";
            }

            index = Params.IndexOfInputParam("_coolingRiserName_");
            string coolingRiserName = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref coolingRiserName);
            }

            if (string.IsNullOrEmpty(coolingRiserName))
            {
                coolingRiserName = "RC1";
            }

            List<MechanicalSystem> mechanicalSystems = null;
            List<IAnalyticalEquipment> analyticalEquipments = null;

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            if (adjacencyCluster != null)
            {
                mechanicalSystems = adjacencyCluster.AddMechanicalSystems(systemTypeLibrary, spaces, supplyUnitName, exhaustUnitName, ventilationRiserName, heatingRiserName, coolingRiserName);
                if (mechanicalSystems != null && mechanicalSystems.Count > 0)
                {
                    analyticalEquipments = new List<IAnalyticalEquipment>();

                    foreach(MechanicalSystem mechanicalSystem in mechanicalSystems)
                    {
                        List<IAnalyticalEquipment> analyticalEquipments_System = adjacencyCluster.GetRelatedObjects<IAnalyticalEquipment>(mechanicalSystem);
                        if(analyticalEquipments_System != null)
                        {
                            analyticalEquipments.AddRange(analyticalEquipments_System);
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