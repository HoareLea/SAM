using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMapInternalConditions : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0c0f2336-3a4d-495e-ba4c-6e8a09d5e94c");

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
        public SAMAnalyticalMapInternalConditions()
          : base("SAMAnalytical.MapInternalConditions", "SAMAnalytical.MapInternalConditions",
              "Map InternalConditions",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical", GH_ParamAccess.item);
            
            int index;

            index = inputParamManager.AddParameter(new GooTextMapParam(), "_textMap_", "_textMap_", "SAM Core TextMap", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooInternalConditionLibraryParam(), "_internalConditionLibrary_", "_internalConditionLibrary_", "SAM Analytical InternalConditionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddBooleanParameter("_overrideNotFound_", "_overrideNotFound_", "Override with null if InternalCondition not found", GH_ParamAccess.item, false);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooInternalConditionParam(), "_defaultInternalCondition_", "_defaultInternalCondition_", "Default InternalCondition applied if override not found", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Analytical", "Analytical", "SAM Analytical", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooInternalConditionParam(), "InternalCondition", "InternalCondition", "SAM Analytical InternalCondition", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooSpaceParam(), "unassignedSpaces", "unassignedSpaces", "SAM Analytical Spaces has not been assigned", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            TextMap textMap = null;
            if (!dataAccess.GetData(1, ref textMap) || textMap == null)
                textMap = ActiveSetting.Setting.GetValue<TextMap>(AnalyticalSettingParameter.InternalConditionTextMap);

            InternalConditionLibrary internalConditionLibrary = null;
            dataAccess.GetData(2, ref internalConditionLibrary);
            if (internalConditionLibrary == null)
                internalConditionLibrary = ActiveSetting.Setting.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary);

            bool overrideNotFound = false;
            if (!dataAccess.GetData(3, ref overrideNotFound))
                overrideNotFound = false;

            InternalCondition internalCondition_Default = null;
            if (!dataAccess.GetData(4, ref internalCondition_Default))
                internalCondition_Default = null;

            List<InternalCondition> internalConditions = new List<InternalCondition>();
            List<Space> spaces_Unassigned = new List<Space>();

            if (sAMObject is Space)
            {
                Space space = new Space((Space)sAMObject);
                InternalCondition internalCondition = space.MapInternalCondition(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                if (space.InternalCondition != internalCondition)
                    internalConditions.Add(internalCondition);
                else
                    spaces_Unassigned.Add(space);

                sAMObject = space;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                internalConditions = adjacencyCluster.MapInternalConditions(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                List<Space> spaces = adjacencyCluster.GetSpaces();
                for (int i = 0; i < internalConditions.Count; i++)
                    if (internalConditions[i] == spaces[i].InternalCondition)
                        spaces_Unassigned.Add(spaces[i]);

                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                if (adjacencyCluster != null)
                {
                    adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                    internalConditions = adjacencyCluster.MapInternalConditions(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                    List<Space> spaces = adjacencyCluster.GetSpaces();
                    for (int i = 0; i < internalConditions.Count; i++)
                        if (internalConditions[i] == spaces[i].InternalCondition)
                            spaces_Unassigned.Add(spaces[i]);

                    sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                }
            }


            dataAccess.SetData(0, new GooSAMObject<SAMObject>(sAMObject));
            dataAccess.SetDataList(1, internalConditions?.ConvertAll(x => new GooInternalCondition(x)));
            dataAccess.SetDataList(2, spaces_Unassigned?.ConvertAll(x => new GooSpace(x)));
        }
    }
}