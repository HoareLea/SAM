using Grasshopper;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetSpaces : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a831d8de-7746-4390-9ed2-91247d102d01");

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
        public SAMAnalyticalGetSpaces()
          : base("SAMAnalytical.GetSpaces", "SAMAnalytical.GetSpaces",
              "Get Spaces from SAM Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalModelParam(), "_analyticalModel", "_analyticalModel", "SAM Analytical AnalyticalModel", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSpaceParam(), "Spaces", "Spaces", "SAM Geometry Spaces", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AnalyticalModel analyticalModel = null;
            if(!dataAccess.GetData(0, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(1, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if (adjacencyCluster == null)
                return;



            List<Space> result = new List<Space>();
            if(sAMObject is InternalCondition)
            {
                List<Space> spaces = adjacencyCluster.GetSpaces();
                if (spaces == null || spaces.Count == 0)
                    return;

                foreach (Space space in spaces)
                {
                    InternalCondition internalCondition = space?.InternalCondition;
                    if (internalCondition == null)
                        continue;

                    if (!internalCondition.Guid.Equals(sAMObject.Guid))
                        continue;

                    result.Add(space);
                }
            }
            else if(sAMObject is Panel)
            {
                result = adjacencyCluster.GetSpaces((Panel)sAMObject);
            }
            else if(sAMObject is Aperture)
            {
                Panel panel = adjacencyCluster.GetPanel((Aperture)sAMObject);
                if(panel != null)
                    result = adjacencyCluster.GetSpaces(panel);
            }
            else if(sAMObject is ApertureConstruction)
            {
                List<Panel> panels = adjacencyCluster.GetPanels((ApertureConstruction)sAMObject);
                if(panels != null && panels.Count != 0)
                {
                    Dictionary<Guid, Space> dictionary = new Dictionary<Guid, Space>();
                    foreach(Panel panel in panels)
                    {
                        List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                        if (spaces == null || spaces.Count == 0)
                            continue;

                        spaces.ForEach(x => dictionary[x.Guid] = x);
                    }

                    result.AddRange(dictionary.Values);
                }
            }
            else if(sAMObject is Construction)
            {
                List<Panel> panels = adjacencyCluster.GetPanels((Construction)sAMObject);
                if (panels != null && panels.Count != 0)
                {
                    Dictionary<Guid, Space> dictionary = new Dictionary<Guid, Space>();
                    foreach (Panel panel in panels)
                    {
                        List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                        if (spaces == null || spaces.Count == 0)
                            continue;

                        spaces.ForEach(x => dictionary[x.Guid] = x);
                    }

                    result.AddRange(dictionary.Values);
                }
            }
            else if(sAMObject is Profile)
            {
                List<Space> spaces = adjacencyCluster.GetSpaces();
                if (spaces == null || spaces.Count == 0)
                    return;

                ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

                foreach (Space space in spaces)
                {
                    Dictionary<ProfileType, Profile> dictionary = space.InternalCondition?.GetProfileDictionary(profileLibrary, true);
                    if (dictionary == null || dictionary.Count == 0)
                        continue;

                    foreach(Profile profile in dictionary.Values)
                    {
                        if(profile.Guid == sAMObject.Guid)
                        {
                            result.Add(space);
                            break;
                        }
                    }
                }
            }

            DataTree<GooSpace> dataTree = new DataTree<GooSpace>();
            result.ForEach(x => dataTree.Add(new GooSpace(x)));
            dataAccess.SetDataTree(0, dataTree);
        }
    }
}