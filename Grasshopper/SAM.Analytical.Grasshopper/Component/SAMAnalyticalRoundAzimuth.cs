using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRoundAzimuth : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e2426ad0-6206-4307-bb26-d0030221d8f0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_angles_", NickName = "_angles_", Description = "Rounding Angles", Access = GH_ParamAccess.list, Optional = true };
                param_Number.PersistentData.Append(new global::Grasshopper.Kernel.Types.GH_Number(0));
                param_Number.PersistentData.Append(new global::Grasshopper.Kernel.Types.GH_Number(90));
                param_Number.PersistentData.Append(new global::Grasshopper.Kernel.Types.GH_Number(180));
                param_Number.PersistentData.Append(new global::Grasshopper.Kernel.Types.GH_Number(270));
                param_Number.PersistentData.Append(new global::Grasshopper.Kernel.Types.GH_Number(360));
                result.Add( new GH_SAMParam(param_Number, ParamVisibility.Voluntary) );

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_range_", NickName = "_range_", Description = "Range +-", Access = GH_ParamAccess.item, Optional = true };
                param_Number.PersistentData.Append(new global::Grasshopper.Kernel.Types.GH_Number(5));
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));
                
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() {Name = "panel", NickName = "panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalRoundAzimuth()
          : base("SAMAnalytical.RoundAzimuth", "SAMAnalytical.RoundAzimuth",
              "Updates Azimuth",
              "SAM", "SAM")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_panel");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel panel = null;
            if (!dataAccess.GetData(index, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> angles = new List<double>();
            index = Params.IndexOfInputParam("_angles_");
            if (index == -1 || !dataAccess.GetDataList(index, angles) || angles == null)
            {
                angles = new List<double>() { 0, 90, 180, 270, 360 };
            }

            double range = double.NaN;
            index = Params.IndexOfInputParam("_range_");
            if (index == -1 || !dataAccess.GetData(index, ref range) || double.IsNaN(range))
            {
                range = 5;
            }

            Face3D face3D = panel?.GetFace3D();
            if(face3D != null)
            {

                Point3D point3D = face3D?.GetCentroid();
                Vector3D normal = face3D.GetPlane().Normal;

                Plane plane = Plane.WorldXY;

                normal = plane.Project(normal);
                if(normal.Length > 0.5)
                {
                    double rad_Range = range * System.Math.PI / 180;

                    double rad = normal.Angle(Vector3D.WorldX);
                    foreach(double angle_Temp in angles)
                    {
                        double rad_Temp = angle_Temp * System.Math.PI / 180;

                        if (rad > rad_Temp - rad_Range && rad < rad_Temp + rad_Range)
                        {
                            double difference = rad_Temp - rad;
                            if(System.Math.Abs(difference) > Core.Tolerance.Distance)
                            {
                                Panel panel_1 = Create.Panel(panel);
                                panel_1.Transform(Transform3D.GetRotation(point3D, Vector3D.WorldZ, -difference));

                                Panel panel_2 = Create.Panel(panel);
                                panel_2.Transform(Transform3D.GetRotation(point3D, Vector3D.WorldZ, difference));

                                Vector3D normal_1 = panel_1.GetFace3D().GetPlane().Normal;
                                normal_1 = plane.Project(normal_1);

                                Vector3D normal_2 = panel_2.GetFace3D().GetPlane().Normal;
                                normal_2 = plane.Project(normal_2);

                                panel = System.Math.Abs(rad_Temp - normal_1.Angle(Vector3D.WorldX)) < System.Math.Abs(rad_Temp - normal_2.Angle(Vector3D.WorldX)) ? panel_1 : panel_2;
                            }
                            break;
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("panel");
            if (index != -1)
            {
                dataAccess.SetData(index, panel);
            }
        }
    }
}