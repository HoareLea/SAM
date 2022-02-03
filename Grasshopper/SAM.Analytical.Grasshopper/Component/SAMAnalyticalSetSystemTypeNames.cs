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
    public class SAMAnalyticalSetSystemTypeNames : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("98f6dda3-da1c-4102-8192-9bf9136cb552");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetSystemTypeNames()
          : base("SAMAnalytical.SetSystemTypeNames", "SAMAnalytical.SetSystemTypeNames",
              "Align Panels",
              "SAM WIP", "Analytical")
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

                GooAnalyticalModelParam analyticalModelParam = new GooAnalyticalModelParam() { Name = "analyticalModel_", NickName = "analyticalModel_", Description = "SAM Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                GooSpaceParam spaceParam = new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list };
                spaceParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(spaceParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String paramString;

                paramString = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "ventilationSystemTypeName_", NickName = "ventilationSystemTypeName_", Description = "Ventilation System Type Name", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramString, ParamVisibility.Binding));

                paramString = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "heatingSystemTypeName_", NickName = "heatingSystemTypeName_", Description = "Heating System Type Name", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramString, ParamVisibility.Binding));

                paramString = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "coolingSystemTypeName_", NickName = "coolingSystemTypeName_", Description = "Cooling System Type Name", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramString, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analyticals", NickName = "analyticals", Description = "SAM Analytical Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_spaces");
            List<Space> spaces = new List<Space>();
            if(index == -1 || !dataAccess.GetDataList(index, spaces) || spaces == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }
            
            index = Params.IndexOfInputParam("analyticalModel_");

            AnalyticalModel analyticalModel = null;
            if(index != -1)
            {
                dataAccess.GetData(index, ref analyticalModel);
            }

            string ventilationSystemTypeName = null;
            index = Params.IndexOfInputParam("ventilationSystemTypeName_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref ventilationSystemTypeName);
            }

            string heatingSystemTypeName = null;
            index = Params.IndexOfInputParam("heatingSystemTypeName_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref heatingSystemTypeName);
            }

            string coolingSystemTypeName = null;
            index = Params.IndexOfInputParam("coolingSystemTypeName_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref coolingSystemTypeName);
            }

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;

            for (int i = 0; i < spaces.Count; i++)
            {
                Space space = spaces[i];
                if(adjacencyCluster != null)
                {
                    space = adjacencyCluster.GetObject<Space>(space.Guid);
                }

                if(space == null)
                {
                    continue;
                }

                InternalCondition internalCondition = space.InternalCondition;
                if(internalCondition == null)
                {
                    internalCondition = new InternalCondition(space.Name);
                }

                if(!string.IsNullOrWhiteSpace(ventilationSystemTypeName))
                {
                    internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, ventilationSystemTypeName);
                }

                if (!string.IsNullOrWhiteSpace(heatingSystemTypeName))
                {
                    internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, heatingSystemTypeName);
                }

                if (!string.IsNullOrWhiteSpace(coolingSystemTypeName))
                {
                    internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, coolingSystemTypeName);
                }

                space.InternalCondition = internalCondition;

                spaces[i] = space;

                if (adjacencyCluster != null)
                {
                    adjacencyCluster.AddObject(spaces[i]);
                }
            }

            if(adjacencyCluster != null)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analyticals");
            if (index != -1)
            {
                if(analyticalModel != null)
                {
                    dataAccess.SetDataList(index, new GooAnalyticalModel[] { new GooAnalyticalModel(analyticalModel)});
                }
                else
                {
                    dataAccess.SetDataList(index, spaces?.ConvertAll(x => new GooSpace(x)));
                }
            }
        }
    }
}