using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetInternalCondition : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("defb11a7-4605-42d0-8d64-481df23dec7e");

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
        public SAMAnalyticalSetInternalCondition()
          : base("SAMAnalytical.SetInternalCondition", "SAMAnalytical.SetInternalCondition",
              "Set Internal Condition",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Model ot Adjacency Cluster", GH_ParamAccess.list);

            inputParamManager.AddTextParameter("_name", "_name", "InternalCondition Name", GH_ParamAccess.item, "Office");

            index = inputParamManager.AddParameter(new GooSpaceParam(), "spaces_", "spaces_", "SAM Analytical Spaces", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooInternalConditionLibraryParam(), "_internalConditionLibrary", "_internalConditionLibrary", "SAM Analytical InternalConditionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Analyticals", "Analyticals", "SAM Analytical Model, Panels or Adjacency Cluster", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooSpaceParam(), "Spaces", "Spaces", "SAM Analytical Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(1, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces_Input = new List<Space>();
            if (!dataAccess.GetDataList(2, spaces_Input))
                spaces_Input = null;

                InternalConditionLibrary internalConditionLibrary = null;
            dataAccess.GetData(1, ref internalConditionLibrary);
            if (internalConditionLibrary == null)
                internalConditionLibrary = ActiveSetting.Setting.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary);

            List<Space> spaces = new List<Space>();

            List<Space> spaces_Output = new List<Space>();

            InternalCondition internalCondition = internalConditionLibrary.GetInternalConditions(name)?.FirstOrDefault();

            List<SAMObject> result = new List<SAMObject>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Space)
                {
                    spaces.Add((Space)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                    List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
                    if (spaces_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        foreach(Space space in spaces_Temp)
                        {
                            if (spaces_Input != null && spaces_Input.Find(x => x.Guid == space.Guid) == null)
                                continue;

                            space.InternalCondition = internalCondition;
                            spaces_Output.Add(space);
                            adjacencyCluster.AddObject(space);
                        }
                    }

                    result.Add(adjacencyCluster);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
                    if (spaces_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        foreach (Space space in spaces_Temp)
                        {
                            if (spaces_Input != null && spaces_Input.Find(x => x.Guid == space.Guid) == null)
                                continue;

                            space.InternalCondition = internalCondition;
                            spaces_Output.Add(space);
                            adjacencyCluster.AddObject(space);
                        }
                    }

                    result.Add(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster));
                }
            }

            if (spaces != null && spaces.Count != 0)
            {
                foreach(Space space in spaces)
                {
                    result.Add(space);

                    if (spaces_Input != null && spaces_Input.Find(x => x.Guid == space.Guid) == null)
                        continue;

                    space.InternalCondition = internalCondition;
                    spaces_Output.Add(space);
                }
            }

            dataAccess.SetDataList(0, result.ConvertAll(x => new GooSAMObject<SAMObject>(x)));
            dataAccess.SetDataList(1, spaces_Output?.ConvertAll(x => new GooSpace(x)));
        }
    }
}