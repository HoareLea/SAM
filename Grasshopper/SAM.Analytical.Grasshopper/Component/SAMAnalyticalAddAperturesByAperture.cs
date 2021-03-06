﻿using Grasshopper.Kernel;
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
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByAperture()
          : base("SAMAnalytical.AddAperturesByAperture", "SAMAnalytical.AddAperturesByAperture",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooApertureParam(), "_apertures", "_apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
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

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> apertures = new List<Aperture>();
            if(!dataAccess.GetDataList(1, apertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {              
                Panel panel = Create.Panel((Panel)sAMObject);

                List<Aperture> apertures_Result = new List<Aperture>();
                foreach (Aperture aperture in apertures)
                    if (panel.AddAperture(aperture))
                        apertures_Result.Add(aperture);

                dataAccess.SetData(0, panel);
                dataAccess.SetDataList(1, apertures_Result.ConvertAll(x => new GooAperture(x)));
                dataAccess.SetData(2, true);
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if(sAMObject is AnalyticalModel)
            {
                analyticalModel = ((AnalyticalModel)sAMObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels != null)
            {
                List<Tuple<Panel, Aperture>> tuples_Result = new List<Tuple<Panel, Aperture>>();

                foreach (Panel panel in panels)
                {
                    Panel panel_New = Create.Panel(panel);

                    bool updated = false;
                    foreach(Aperture aperture in apertures)
                    {
                        if(panel_New.AddAperture(aperture))
                        {
                            updated = true;
                            tuples_Result.Add(new Tuple<Panel, Aperture>(panel_New, aperture));
                        }
                    }

                    if (updated)
                        adjacencyCluster.AddObject(panel_New);
                }

                if(analyticalModel != null)
                {
                    AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                    dataAccess.SetData(0, analyticalModel_Result);
                }
                else
                {
                    dataAccess.SetData(0, adjacencyCluster);
                }
                
                dataAccess.SetDataList(1, tuples_Result.ConvertAll(x => new GooAperture(x.Item2)));
                dataAccess.SetData(2, true);
            }
        }
    }
}