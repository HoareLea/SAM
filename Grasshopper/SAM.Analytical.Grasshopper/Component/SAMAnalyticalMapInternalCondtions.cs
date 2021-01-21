using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMapInternalConditions : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0c0f2336-3a4d-495e-ba4c-6e8a09d5e94c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMapInternalConditions()
          : base("SAMAnalytical.MapInternalConditions", "SAMAnalytical.MapInternalConditions",
              "Map InternalConditions",
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
                
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject()
                {
                    Name = "_analytical",
                    NickName = "_analytical",
                    Description = "SAM Analytical Object",
                    Access = GH_ParamAccess.item
                }, ParamVisibility.Binding));


                GooTextMapParam gooTextMapParam = new GooTextMapParam() { Name = "textMap_", NickName = "textMap_", Description = "SAM Core TextMap", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooTextMapParam, ParamVisibility.Voluntary));

                GooInternalConditionLibraryParam gooInternalConditionLibraryParam = new GooInternalConditionLibraryParam() { Name = "internalConditionLibrary_", NickName = "internalConditionLibrary_", Description = "SAM Analytical InternalConditionLibrary", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooInternalConditionLibraryParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "overrideNotFound_", NickName = "overrideNotFound_", Description = "Override with null if InternalCondition not found", Access = GH_ParamAccess.item};
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Voluntary));

                GooInternalConditionParam gooInternalConditionParam = new GooInternalConditionParam() { Name = "defaultInternalCondition_", NickName = "defaultInternalCondition_", Description = "Default InternalCondition applied if override not found", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooInternalConditionParam, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "UnassignedSpaces", NickName = "UnassignedSpaces", Description = "SAM Analytical Spaces has not been assigneds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            TextMap textMap = null;
            index = Params.IndexOfInputParam("textMap_");
            if (index == -1 || !dataAccess.GetData(index, ref textMap))
                textMap = null;

            if (textMap == null)
                textMap = ActiveSetting.Setting.GetValue<TextMap>(AnalyticalSettingParameter.InternalConditionTextMap);


            InternalConditionLibrary internalConditionLibrary = null;
            index = Params.IndexOfInputParam("internalConditionLibrary_");
            if (index == -1 || !dataAccess.GetData(index, ref internalConditionLibrary))
                internalConditionLibrary = null;

            if (internalConditionLibrary == null)
                internalConditionLibrary = ActiveSetting.Setting.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary);


            bool overrideNotFound = false;
            index = Params.IndexOfInputParam("overrideNotFound_");
            if (index == -1 || !dataAccess.GetData(index, ref overrideNotFound))
                overrideNotFound = false;


            InternalCondition internalCondition_Default = null;
            index = Params.IndexOfInputParam("defaultInternalCondition_");
            if (index == -1 || !dataAccess.GetData(index, ref internalCondition_Default))
                internalCondition_Default = null;

            List<InternalCondition> internalConditions = new List<InternalCondition>();
            List<Space> spaces_Unassigned = new List<Space>();

            if (sAMObject is Space)
            {
                List<Space> spaces = new List<Space>() { new Space((Space)sAMObject) };
                internalConditions = spaces.MapInternalConditions(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                if (internalConditions[0] == spaces[0].InternalCondition)
                    spaces_Unassigned.Add(spaces[0]);

                sAMObject = spaces[0];
            }
            else if (sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                internalConditions = adjacencyCluster.MapInternalConditions(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                List<Space> spaces = adjacencyCluster.GetSpaces();
                for (int i = 0; i < internalConditions.Count; i++)
                {
                    InternalCondition internalCondition = internalConditions[i];
                    if (internalCondition == spaces[i].InternalCondition)
                        spaces_Unassigned.Add(spaces[i]);
                }

                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = (AnalyticalModel)sAMObject;

                AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
                if (adjacencyCluster != null)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                    internalConditions = adjacencyCluster.MapInternalConditions(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                    List<Space> spaces = adjacencyCluster.GetSpaces();
                    for (int i = 0; i < internalConditions.Count; i++)
                        if (internalConditions[i] == spaces[i].InternalCondition)
                            spaces_Unassigned.Add(spaces[i]);

                    sAMObject = new AnalyticalModel(analyticalModel, adjacencyCluster);
                }
            }


            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
                dataAccess.SetDataList(index, internalConditions?.ConvertAll(x => new GooInternalCondition(x)));

            index = Params.IndexOfOutputParam("UnassignedSpaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Unassigned?.ConvertAll(x => new GooSpace(x)));
        }
    }
}