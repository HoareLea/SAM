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
    public class SAMAnalyticalSplitPanelsByGeometries : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3aa380c5-7eec-49c5-a999-482bbfde7ba5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

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

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometries", NickName = "_geometries", Description = "Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("_analytical");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            index = Params.IndexOfInputParam("_geometries");
            if(index != -1)
            {
                List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
                dataAccess.GetDataList(index, objectWrappers);
                if(objectWrappers != null)
                {
                    foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
                    {
                        if(objectWrapper.Value is IFace3DObject)
                        {
                            geometry3Ds.Add(((IFace3DObject)objectWrapper.Value).Face3D);
                            continue;
                        }
                        
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

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            List<Panel> panels = null;

            if(adjacencyCluster != null)
            {
                HashSet<Guid> panelGuids = null;
                index = Params.IndexOfInputParam("spaces_");
                if (index != -1)
                {
                    List<Space> spaces = new List<Space>();
                    if(dataAccess.GetDataList(index, spaces) && spaces != null)
                    {
                        panelGuids = new HashSet<Guid>();
                        foreach (Space space in spaces)
                        {
                            adjacencyCluster.GetPanels(space)?.ForEach(x => panelGuids.Add(x.Guid));
                        }
                    }
                }
                if(geometry3Ds != null)
                {
                    panels = adjacencyCluster.SplitPanels(geometry3Ds, panelGuids);
                }
            }

            if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
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