using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSplitPanelsByGeometries : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3aa380c5-7eec-49c5-a999-482bbfde7ba5");

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
        public SAMAnalyticalSplitPanelsByGeometries()
          : base("SAMAnalytical.SplitPanelsByGeometries", "SAMAnalytical.SplitPanelsByGeometries",
              "Split SAM Analytical Panels by Geometries, *aperture will be splited as well",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analytical", "_analytical", "SAM Analytical Object such as AdjacencyCluster or Analytical Model", GH_ParamAccess.item);
            inputParamManager.AddGeometryParameter("_geometries", "_geometries", "Geometries", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "analytical", "analytical", "SAM Analytical Object", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooPanelParam(), "panels", "panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_analytical");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(0, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            index = Params.IndexOfInputParam("_geometries");
            if(index != -1)
            {
                List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
                dataAccess.GetDataList(1, objectWrappers);
                if(objectWrappers != null)
                {
                    foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
                    {
                        if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds) && face3Ds != null)
                        {
                            face3Ds.ForEach(x => geometry3Ds.Add(x));
                            continue;
                        }

                        if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<ISegmentable3D> segmentable3Ds) && segmentable3Ds != null)
                        {
                            segmentable3Ds.ForEach(x => geometry3Ds.Add(x));
                            continue;
                        }

                        if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Plane> planes) && planes != null)
                        {
                            planes.ForEach(x => geometry3Ds.Add(x));
                            continue;
                        }
                    }
                }
            }

            List<Panel> panels = null;

            if(geometry3Ds != null)
            {
                if(analyticalObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
                    panels = adjacencyCluster.SplitPanels(geometry3Ds);

                    analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
                }
                else if(analyticalObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
                    panels = adjacencyCluster.SplitPanels(geometry3Ds);

                    analyticalObject = adjacencyCluster;
                }
            }

            index = Params.IndexOfOutputParam("analytical");
            if(index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalObject(analyticalObject));
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}