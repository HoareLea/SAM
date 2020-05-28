using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelSpacing : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("32fcd1a9-7926-45de-bd27-3dde667ba0d0");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPanelSpacing()
          : base("SAMAnalytical.PanelSpacing", "SAMAnalytical.PanelSpacing",
              "Calculates Spacing between Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_SAMPanels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("max", "max", "Maximal distance to be checked", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
            inputParamManager.AddNumberParameter("min", "min", "Minimal distance to be checked", GH_ParamAccess.item, Core.Tolerance.Distance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double max = Core.Tolerance.MacroDistance;
            if (!dataAccess.GetData(1, ref max))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double min = Core.Tolerance.Distance;
            if (!dataAccess.GetData(2, ref min))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>> tuples = new List<Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                tuples.Add(new Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>(panel, face3D.GetBoundingBox(max), face3D, Geometry.Spatial.Query.Point3Ds(face3D, true, false)));
            }

            HashSet<Point3D> point3Ds = new HashSet<Point3D>();
            foreach (Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>> tuple in tuples)
            {
                List<Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>> tuples_Temp = tuples.FindAll(x => x.Item2.Intersect(tuple.Item2) || x.Item2.Inside(tuple.Item2) || tuple.Item2.Inside(x.Item2));
                if (tuples_Temp == null || tuples_Temp.Count < 2)
                    continue;

                tuples_Temp.Remove(tuple);

                Face3D face3D = tuple.Item3;
                BoundingBox3D boundingBox3D = tuple.Item2;
                foreach (Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>> tuple_Temp in tuples_Temp)
                {
                    foreach (Point3D point3D in tuple_Temp.Item4)
                    {
                        if (point3Ds.Contains(point3D))
                            continue;

                        if (!boundingBox3D.Inside(point3D))
                            continue;

                        double distance = face3D.DistanceToEdges(point3D);
                        if (distance < max && distance > min)
                            point3Ds.Add(point3D);
                    }
                }
            }

            dataAccess.SetDataList(0, point3Ds.ToList().ConvertAll(x => x.ToRhino()));
        }
    }
}