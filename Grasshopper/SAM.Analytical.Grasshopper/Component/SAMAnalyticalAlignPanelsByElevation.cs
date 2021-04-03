using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAlignPanelsByElevation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6cdf46cb-5e46-407d-847d-0e12a9b2545a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAlignPanelsByElevation()
          : base("SAMAnalytical.AlignPanelsByElevation", "SAMAnalytical.AlignPanelsByElevation",
              "Align Panels By Upper and Lower Elevation",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_upperElevation", NickName = "_upperElevation", Description = "Upper Elevation", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lowerElevation", NickName = "_lowerElevation", Description = "Lower Elevation", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "UpperPanels", NickName = "UpperPanels", Description = "SAM Analytical Upper Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "LowerPanels", NickName = "LowerPanels", Description = "SAM Analytical Lower Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if(index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }
            
            index = Params.IndexOfInputParam("_upperElevation");
            double upperElevation = double.NaN;
            if(index == -1 || !dataAccess.GetData(index, ref upperElevation) || double.IsNaN(upperElevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_lowerElevation");
            double lowerElevation = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref lowerElevation) || double.IsNaN(lowerElevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            if (double.IsNaN(tolerance))
                tolerance = Tolerance.Distance;

            Geometry.Spatial.Plane plane_Upper = Geometry.Spatial.Plane.WorldXY.GetMoved(new Geometry.Spatial.Vector3D(0, 0, upperElevation)) as Geometry.Spatial.Plane;
            Geometry.Spatial.Plane plane_Lower = Geometry.Spatial.Plane.WorldXY.GetMoved(new Geometry.Spatial.Vector3D(0, 0, lowerElevation)) as Geometry.Spatial.Plane;

            List<Panel> panels_Result = new List<Panel>();
            List<Panel> panels_Upper = new List<Panel>();
            List<Panel> panels_Lower = new List<Panel>();
            for (int i=0; i < panels.Count;i++)
            {
                if(panels[i] == null)
                    continue;

                Panel panel_Temp = new Panel(panels[i]);

                panel_Temp = Analytical.Modify.Extend(panel_Temp, plane_Upper, Tolerance.Angle, tolerance);
                panel_Temp = Analytical.Modify.Extend(panel_Temp, plane_Lower, Tolerance.Angle, tolerance);

                List<Panel> panels_Temp = Analytical.Query.Cut(panel_Temp, new Geometry.Spatial.Plane[] { plane_Upper, plane_Lower });
                if (panels_Temp == null || panels_Temp.Count == 0)
                    continue;

                foreach(Panel panel_Result in panels_Temp)
                {
                    Geometry.Spatial.Point3D point3D = panel_Result.GetInternalPoint3D();
                    if (Geometry.Spatial.Query.Above(plane_Upper, point3D))
                        panels_Upper.Add(panel_Result);
                    else if (Geometry.Spatial.Query.Below(plane_Lower, point3D))
                        panels_Lower.Add(panel_Result);
                    else
                        panels_Result.Add(panel_Result);
                }
            }

           

            dataAccess.SetDataList(0, panels_Result?.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, panels_Upper?.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(2, panels_Lower?.ConvertAll(x => new GooPanel(x)));
        }
    }
}