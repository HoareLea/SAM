using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemoveInternalEdges : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("256a2fd6-8ffb-464e-bd96-19039c13fa18");

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
        public SAMAnalyticalRemoveInternalEdges()
          : base("SAMAnalytical.RemoveInternalEdges", "SAMAnalytical.RemoveInternalEdges",
              "Removes Internal Edges in SAM Analytical Object",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
                return;

            if(sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);
                panel = Create.Panel(panel.Guid, panel, new Face3D(panel.GetFace3D().GetExternalEdge3D()), null, false);

                dataAccess.SetData(0, panel);
                return;
            }

            AnalyticalModel analyticalModel = null;
            if (sAMObject is AnalyticalModel)
            {
                analyticalModel = (AnalyticalModel)sAMObject;
                sAMObject = analyticalModel.AdjacencyCluster;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
            {
                List<Panel> panels = ((AdjacencyCluster)sAMObject).GetPanels();
                List<Panel> panels_Updated = new List<Panel>();
                foreach (Panel panel in panels)
                {
                    Face3D face3D = panel.GetFace3D();

                    List<IClosedPlanar3D> internalEdge3Ds = face3D?.GetInternalEdge3Ds();
                    if (internalEdge3Ds == null || internalEdge3Ds.Count == 0)
                        continue;

                    panels_Updated.Add(Create.Panel(panel.Guid, panel, new Face3D(face3D.GetExternalEdge3D()), null, false));
                }

                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                foreach (Panel panel in panels_Updated)
                    adjacencyCluster.AddObject(panel);
            }
            
            if(analyticalModel == null && adjacencyCluster == null)
            {
                dataAccess.SetData(0, sAMObject);
                return;
            }

            if(analyticalModel != null)
            {
                if(adjacencyCluster == null)
                {
                    dataAccess.SetData(0, sAMObject);
                    return;
                }

                dataAccess.SetData(0, new AnalyticalModel(analyticalModel, adjacencyCluster));
                return;
            }

            if (adjacencyCluster != null)
                dataAccess.SetData(0, adjacencyCluster);
        }
    }
}