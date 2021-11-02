using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAnalyticalModelMapInternalConditions : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3603c2db-47ac-4ee7-bb55-5738dc585246");

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
        public SAMAnalyticalAnalyticalModelMapInternalConditions()
          : base("SAMAnalytical.AnalyticalModelMapInternalConditions", "SAMAnalytical.AnalyticalModelMapInternalConditions",
              "Map InternalConditions for AnalyticalModel",
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
                
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam()
                {
                    Name = "_analyticalModel",
                    NickName = "_analyticalModel",
                    Description = "SAM Analytical AnalyticalModel",
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

                GooProfileLibraryParam gooProfileLibraryParam = new GooProfileLibraryParam() { Name = "profileLibrary_", NickName = "profileLibrary_", Description = "SAM Analytical ProfileLibrary", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooProfileLibraryParam, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "UnassignedSpaces", NickName = "UnassignedSpaces", Description = "SAM Analytical Spaces has not been assigneds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "Profiles", NickName = "Profiles", Description = "SAM Analytical Profiles", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
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

            ProfileLibrary profileLibrary = null;
            index = Params.IndexOfInputParam("profileLibrary_");
            if (index == -1 || !dataAccess.GetData(index, ref profileLibrary))
                profileLibrary = null;

            if (profileLibrary == null)
                profileLibrary = Analytical.Query.DefaultProfileLibrary();


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

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            IEnumerable<Profile> profiles = null;
            if (adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                internalConditions = adjacencyCluster.MapInternalConditions(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                List<Space> spaces = adjacencyCluster.GetSpaces();
                for (int i = 0; i < internalConditions.Count; i++)
                    if (internalConditions[i] == spaces[i].InternalCondition)
                        spaces_Unassigned.Add(spaces[i]);

                profiles = Analytical.Query.Profiles(adjacencyCluster, profileLibrary);
                profileLibrary = new ProfileLibrary("Default Material Library", profiles);

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
            }


            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
                dataAccess.SetDataList(index, internalConditions?.ConvertAll(x => new GooInternalCondition(x)));

            index = Params.IndexOfOutputParam("UnassignedSpaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Unassigned?.ConvertAll(x => new GooSpace(x)));

            index = Params.IndexOfOutputParam("Profiles");
            if (index != -1)
                dataAccess.SetDataList(index, profiles?.ToList().ConvertAll(x => new GooProfile(x)));
        }
    }
}