using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Render;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGrid : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("469ea20b-7860-4e77-9f33-5d642056c685");

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
        public SAMAnalyticalGrid()
          : base("SAMAnalytical.Grid", "SAMAnalytical.Grid",
              "Calculates Grid from Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_SAManalytical", "_SAManalytical", "SAM Analytical Object", GH_ParamAccess.list);
            index = inputParamManager.AddPlaneParameter("plane_", "plane_", "GH Plane", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddPointParameter("origin_", "origin_", "GH Origin Point", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddNumberParameter("_x_", "_x_", "Grid size on X Axis", GH_ParamAccess.item, 0.2);
            inputParamManager.AddNumberParameter("_y_", "_y_", "Grid size on Y Axis", GH_ParamAccess.item, 0.2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddLineParameter("Lines", "Lines", "Lines", GH_ParamAccess.list);
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

            List<Panel> panels = new List<Panel>();
            foreach(SAMObject sAMObject in sAMObjects)
            {
                if (sAMObject is Panel)
                {
                    panels.Add((Panel)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    List<Panel> panels_Temp = ((AdjacencyCluster)sAMObject).GetPanels();
                    if (panels_Temp != null)
                        panels.AddRange(panels_Temp);
                }
                else if(sAMObject is AnalyticalModel)
                {
                    List<Panel> panels_Temp = ((AnalyticalModel)sAMObject).AdjacencyCluster?.GetPanels();
                    if (panels_Temp != null)
                        panels.AddRange(panels_Temp);
                }

            }

            Geometry.Spatial.Plane plane = null;

            Plane plane_Rhino = Plane.Unset;
            dataAccess.GetData(1, ref plane_Rhino);
            if (plane_Rhino.IsValid && plane_Rhino != Plane.Unset)
                plane = plane_Rhino.ToSAM();


            Geometry.Spatial.Point3D origin = null;

            Point3d origin_Rhino = Point3d.Unset;
            dataAccess.GetData(2, ref origin_Rhino);
            if (origin_Rhino.IsValid && origin_Rhino != Point3d.Unset)
                origin = origin_Rhino.ToSAM();

            double x = 0.2;
            dataAccess.GetData(3, ref x);
            if (double.IsNaN(x))
                x = 0.2;

            double y = 0.2;
            dataAccess.GetData(4, ref y);
            if (double.IsNaN(y))
                y = 0.2;


            List<Geometry.Spatial.Segment3D> segment3Ds = Analytical.Query.Grid(panels, x, y, plane, origin);

            dataAccess.SetDataList(0, segment3Ds?.ConvertAll(segment2D => segment2D.ToRhino_Line()));
        }
    }
}