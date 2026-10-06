// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyMechanicalSystems : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d9266a28-5770-469d-ae06-bea164ef2ce5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSystemTypeLibraryParam() { Name = "_systemTypeLibrary_", NickName = "_systemTypeLibrary_", Description = "SAM SystemTypeLibrary", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "supplyUnitName_", NickName = "_upplyUnitName", Description = "Supply Unit Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "exhaustUnitName_", NickName = "exhaustUnitName_", Description = "Exhaust Unit Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "coolingSystemType_", NickName = "coolingSystemType_", Description = "Cooling System Type", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "heatingSystemType_", NickName = "heatingSystemType_", Description = "Heating System Type", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSystemTypeParam() { Name = "ventilationSystemType_", NickName = "ventilationSystemType_", Description = "Ventilation System Type", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSystemParam() { Name = "mechanicalSystems", NickName = "mechanicalSystems", Description = "mechanicalSystems", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_systemTypeLibrary_");
            SystemTypeLibrary systemTypeLibrary = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref systemTypeLibrary);
            }

            if (systemTypeLibrary == null)
                systemTypeLibrary = ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);

            index = Params.IndexOfInputParam("_spaces_");
            List<Space> spaces = new List<Space>();
            if (index != -1 && !dataAccess.GetDataList(index, spaces))
                spaces = null;

            index = Params.IndexOfInputParam("supplyUnitName_");
            string supplyUnitName = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref supplyUnitName);
            }

            index = Params.IndexOfInputParam("exhaustUnitName_");
            string exhaustUnitName = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref exhaustUnitName);
            }

            index = Params.IndexOfInputParam("coolingSystemType_");
            ISystemType coolingSystemType = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref coolingSystemType);
            }

            index = Params.IndexOfInputParam("heatingSystemType_");
            ISystemType heatingSystemType = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref heatingSystemType);
            }

            index = Params.IndexOfInputParam("ventilationSystemType_");
            ISystemType ventilationSystemType = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref ventilationSystemType);
            }

            List<MechanicalSystem> mechanicalSystems = new List<MechanicalSystem>();

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            if (spaces == null || spaces.Count == 0)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            if (spaces != null)
            {
                if (ventilationSystemType != null)
                {
                    MechanicalSystem mechanicalSystem = adjacencyCluster.AddMechanicalSystem(ventilationSystemType as VentilationSystemType, spaces);
                    if (mechanicalSystem != null)
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

                if (!string.IsNullOrWhiteSpace(supplyUnitName) || !string.IsNullOrWhiteSpace(exhaustUnitName))
                {
                    VentilationSystem ventilationSystem = null;
                    foreach (Space space in spaces)
                    {
                        List<VentilationSystem> ventilationSystems = adjacencyCluster.GetRelatedObjects<VentilationSystem>(space);
                        if (ventilationSystems != null && ventilationSystems.Count != 0)
                        {
                            foreach (VentilationSystem ventilationSystem_Temp in ventilationSystems)
                            {
                                if (ventilationSystem_Temp != null)
                                {
                                    ventilationSystem = ventilationSystem_Temp;
                                    break;
                                }
                            }

                            if (ventilationSystem != null)
                            {
                                break;
                            }
                        }

                    }

                    if (ventilationSystem == null)
                    {
                        ventilationSystem = adjacencyCluster.AddMechanicalSystem(systemTypeLibrary?.GetSystemTypes<VentilationSystemType>().FirstOrDefault(), spaces) as VentilationSystem;
                    }

                    if (ventilationSystem != null)
                    {
                        if (!string.IsNullOrWhiteSpace(supplyUnitName))
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

            if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("mechanicalSystems");
            if (index != -1)
            {
                dataAccess.SetDataList(index, mechanicalSystems);
            }
        }
    }
}
