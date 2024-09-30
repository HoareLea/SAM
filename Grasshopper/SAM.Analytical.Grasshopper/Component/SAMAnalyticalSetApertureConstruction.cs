using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetApertureConstruction : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c75e9a22-5c7e-4d9c-9a64-6a0eb39ef2d9");

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
        public SAMAnalyticalSetApertureConstruction()
          : base("SAMAnalytical.SetApertureConstruction", "SAMAnalytical.SetApertureConstruction",
              "Set ApertureConstruction of Aperture",
              "SAM", "Analytical03")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooApertureParam(), "_apertures", "_apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
            inputParamManager.AddParameter(new GooApertureConstructionParam(), "_apertureConstruction", "_apertureConstruction", "SAM Analytical ApertureConstruction", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "AdjacencyCluster", "AdjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooApertureParam(), "Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
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
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<Aperture> apertures = new List<Aperture>();
            if (!dataAccess.GetDataList(1, apertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ApertureConstruction apertureConstruction = null;
            if (!dataAccess.GetData(2, ref apertureConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = sAMObject as AdjacencyCluster;
            AnalyticalModel analyticalModel = sAMObject as AnalyticalModel;
            if (adjacencyCluster == null)
                adjacencyCluster = analyticalModel?.AdjacencyCluster;

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);
            List<Aperture> apertures_Result = new List<Aperture>();
            
            List<Panel> panels = adjacencyCluster_Result.GetPanels();
            if(panels != null && panels.Count != 0)
            {

                List<Panel> panels_Result = new List<Panel>();

                foreach (Panel panel in panels)
                {
                    Aperture aperture_New = null;

                    foreach (Aperture aperture in apertures)
                    {
                        Aperture aperture_Old = panel.GetAperture(aperture.Guid);
                        if (aperture_Old == null)
                            continue;

                        aperture_New = new Aperture(aperture_Old, apertureConstruction);

                        if (aperture_New == null)
                            continue;

                        apertures_Result.Add(aperture_New);

                        panel.RemoveAperture(aperture_Old.Guid);
                        panel.AddAperture(aperture_New);
                    }

                    if (aperture_New == null)
                        continue;

                    panels_Result.Add(panel);
                }


                foreach (Panel panel in panels_Result)
                    adjacencyCluster_Result.AddObject(panel);
            }

            if(analyticalModel != null)
                dataAccess.SetData(0, new AnalyticalModel(analyticalModel, adjacencyCluster_Result));
            else if(adjacencyCluster != null)
                dataAccess.SetData(0, adjacencyCluster_Result);

            dataAccess.SetDataList(1, apertures_Result.ConvertAll(x => new GooAperture(x)));
        }
    }
}