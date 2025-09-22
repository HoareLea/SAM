using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByAperture : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a2392741-bec2-4646-8205-f9eb36cccd46");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.6";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByAperture()
          : base("SAMAnalytical.AddAperturesByAperture", "SAMAnalytical.AddAperturesByAperture",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model. Component does not copy instance parameters of Aperture!",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);
            int index = inputParamManager.AddParameter(new GooApertureParam(), "_apertures_", "_apertures_", "SAM Analytical Apertures", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Max Distance", GH_ParamAccess.item, 0.1);

            inputParamManager.AddBooleanParameter("trimGeometry_", "trimGeometry_", "trimGeometry (bool, default = true)\r\n\r\nDetermines how apertures are handled when their geometry extends beyond the boundaries of panels in the AdjacencyCluster.\r\n\r\ntrue (default)\r\n\r\nIf an aperture overlaps multiple panels, the geometry will be split so that each portion is placed as a separate aperture on the corresponding panels.\r\n\r\nIf an aperture extends beyond a single panel and no other adjacent panel exists, the aperture will be trimmed to fit within that panel.\r\n\r\nfalse\r\n\r\nApertures will be added without splitting or trimming, even if they extend beyond panel boundaries. ", GH_ParamAccess.item, true);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(2, false);

            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(0, ref analyticalObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> apertures = new List<Aperture>();
            dataAccess.GetDataList(1, apertures);

            double maxDistance = 0.1;
            if (!dataAccess.GetData(2, ref maxDistance) || double.IsNaN(maxDistance))
            {
                maxDistance = 0.1;
            }

            bool trimApertures = true;
            dataAccess.GetData(3, ref trimApertures);

            if (analyticalObject is Panel)
            {
                Panel panel = Create.Panel((Panel)analyticalObject);

                List<Aperture> apertures_Result = null;
                if (apertures != null && apertures.Count != 0)
                {
                    apertures_Result = new List<Aperture>();
                    foreach (Aperture aperture in apertures)
                    {
                        if (aperture == null)
                        {
                            continue;
                        }

                        List<Aperture> apertures_New = panel.AddApertures([aperture], trimApertures, Tolerance.MacroDistance, maxDistance);
                        if (apertures_New != null)
                        {
                            apertures_Result.AddRange(apertures_New);
                        }
                    }
                }

                dataAccess.SetData(0, panel);
                dataAccess.SetDataList(1, apertures_Result?.ConvertAll(x => new GooAperture(x)));
                dataAccess.SetData(2, apertures_Result != null && apertures_Result.Count != 0);
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }
            else if (analyticalObject is AnalyticalModel)
            {
                analyticalModel = ((AnalyticalModel)analyticalObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Tuple<Panel, Aperture>> tuples_Result = null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels != null && panels.Count != 0)
            {
                tuples_Result = new List<Tuple<Panel, Aperture>>();

                foreach (Panel panel in panels)
                {
                    Panel panel_New = Create.Panel(panel);

                    bool updated = false;
                    foreach (Aperture aperture in apertures)
                    {
                        if(aperture == null)
                        {
                            continue;
                        }
                        
                        List<Aperture> apertures_New = Analytical.Modify.AddApertures(panel_New, aperture.ApertureConstruction, aperture.GetFace3D(), trimApertures, Tolerance.MacroDistance, maxDistance);
                        if (apertures_New != null && apertures_New.Count > 0)
                        {
                            updated = true;
                            foreach (Aperture aperture_New in apertures_New)
                            {
                                tuples_Result.Add(new Tuple<Panel, Aperture>(panel_New, aperture_New));
                            }
                        }
                    }

                    if (updated)
                        adjacencyCluster.AddObject(panel_New);
                }
            }

            if (analyticalModel != null)
            {
                AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                dataAccess.SetData(0, analyticalModel_Result);
            }
            else
            {
                dataAccess.SetData(0, adjacencyCluster);
            }

            dataAccess.SetDataList(1, tuples_Result?.ConvertAll(x => new GooAperture(x.Item2)));
            dataAccess.SetData(2, tuples_Result != null && tuples_Result.Count != 0);
        }
    }
}