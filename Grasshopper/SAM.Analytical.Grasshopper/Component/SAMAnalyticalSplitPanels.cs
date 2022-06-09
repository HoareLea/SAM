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
    public class SAMAnalyticalSplitPanels : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7a63e85c-3d26-46f6-b9f3-33e9c9a9b3fc");

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
        public SAMAnalyticalSplitPanels()
          : base("SAMAnalytical.SplitPanels", "SAMAnalytical.SplitPanels",
              "Split SAM Analytical Panels by Elevations or Planes, *aperture will be splited as well",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_elevations", "_elevations", "Elevations or Planes", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("threshold_", "threshold_", "Threshold", GH_ParamAccess.item, double.NaN);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddIntervalParameter("intervals", "intervals", "Elevations intervals", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "panels", "panels", "SAM Analytical Panels", GH_ParamAccess.tree);
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
            if(!dataAccess.GetDataList(0, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if(!dataAccess.GetDataList(1, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Plane> planes = new List<Plane>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = objectWrapper.Value;

                if (@object is IGH_Goo)
                {
                    try
                    {
                        @object = (@object as dynamic).Value;
                    }
                    catch (Exception exception)
                    {
                        @object = null;
                    }
                }

                if (@object == null)
                    continue;


                Plane plane = null;

                if (Core.Query.IsNumeric(@object))
                {
                    plane = Geometry.Spatial.Create.Plane((double)@object);
                }
                else if (@object is Plane)
                {
                    plane = (Plane)@object;
                }
                else if (@object is GH_Plane)
                {
                    plane = ((GH_Plane)@object).ToSAM();
                }
                else if(@object is global::Rhino.Geometry.Plane)
                {
                    plane = Geometry.Rhino.Convert.ToSAM(((global::Rhino.Geometry.Plane)@object));
                }
                else if (@object is Architectural.Level)
                {
                    plane = Geometry.Spatial.Create.Plane(((Architectural.Level)@object).Elevation);
                }
                else if (@object is string)
                {
                    double value;
                    if (double.TryParse((string)@object, out value))
                        plane = Geometry.Spatial.Create.Plane(value);
                }

                if (plane != null)
                    planes.Add(plane);
            }

            double threshold = double.NaN;
            if(!dataAccess.GetData(2, ref threshold))
            {
                threshold = double.NaN;
            }

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                List<Panel> panels_Temp = Analytical.Query.Cut(panel, planes, threshold);
                if(panels_Temp != null)
                {
                    result.AddRange(panels_Temp);
                }
            }

            planes.Sort((x, y) => x.Origin.Z.CompareTo(y.Origin.Z));

            BoundingBox3D boundingBox3D_Max = new BoundingBox3D(result.ConvertAll(x => x.GetBoundingBox()));

            List<Tuple<double, double, List<Panel>>> tuples = new List<Tuple<double, double, List<Panel>>>();
            foreach (Panel panel_Temp in result)
            {
                BoundingBox3D boundingBox3D = panel_Temp?.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }

                Point3D point3D_Max = boundingBox3D.Max;
                Point3D point3D_Min = boundingBox3D.Min;

                double max = double.NaN;
                double min = double.NaN;

                List<Plane> planes_Temp = null;

                planes_Temp =  planes.FindAll(x => x.Above(point3D_Max) || x.On(point3D_Max));
                max = planes_Temp == null || planes_Temp.Count == 0 ? boundingBox3D_Max.Max.Z : planes_Temp.First().Origin.Z;

                planes_Temp = planes.FindAll(x => x.Below(point3D_Min) || x.On(point3D_Min));
                min = planes_Temp == null || planes_Temp.Count == 0 ? boundingBox3D_Max.Min.Z : planes_Temp.Last().Origin.Z;

                Tuple<double, double, List<Panel>>  tuple = tuples.Find(x => x.Item1 == max && x.Item2 == min);
                if(tuple == null)
                {
                    tuple = new Tuple<double, double, List<Panel>>(max, min, new List<Panel>());
                    tuples.Add(tuple);
                }

                tuple.Item3.Add(panel_Temp);
            }

            List<global::Rhino.Geometry.Interval> intervals = new List<global::Rhino.Geometry.Interval>();
            DataTree<GooPanel> dataTree_Panel = new DataTree<GooPanel>();
            for (int i = 0; i < tuples.Count; i++)
            {
                GH_Path path = new GH_Path(i);

                intervals.Add(new global::Rhino.Geometry.Interval(tuples[i].Item2, tuples[i].Item1));
                tuples[i].Item3?.ForEach(x => dataTree_Panel.Add(new GooPanel(x), path));
            }

            dataAccess.SetDataList(0, intervals.ConvertAll(x => new GH_Interval(x)));
            dataAccess.SetDataTree(1, dataTree_Panel);
        }
    }
}