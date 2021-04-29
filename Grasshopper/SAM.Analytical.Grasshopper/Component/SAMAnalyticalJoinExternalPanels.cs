using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalJoinExternalPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("29242fec-1491-4f2e-9bdd-ec815e001907");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalJoinExternalPanels()
          : base("SAMAnalytical.JoinExternalPanels", "SAMAnalytical.JoinExternalPanels",
              "Join External Panels",
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

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_elevation", NickName = "elevation", Description = "Elevation", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.2);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "externalPanels_Old", NickName = "externalPanels_Old", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "externalPanels_New", NickName = "externalPanels_New", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "externalPolygon2Ds", NickName = "externalPolygon2Ds", Description = "SAM Geometry Pnlanar External Polygon2Ds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "extendedPanels", NickName = "extendedPanels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            
            index = Params.IndexOfInputParam("_elevation");
            double elevation = double.NaN;
            if(index == -1 || !dataAccess.GetData(index, ref elevation) || double.IsNaN(elevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("maxDistance_");
            double maxDistance = 0.2;
            if (index != -1)
                dataAccess.GetData(index, ref maxDistance);

            if (double.IsNaN(maxDistance))
                maxDistance = 0.2;

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            if (double.IsNaN(tolerance))
                tolerance = Tolerance.Distance;

            List<Guid> guids = Analytical.Modify.JoinExternal(panels, elevation, maxDistance, out List<Panel> externalPanels_Old, out List<Panel> externalPanels_New, out List<Geometry.Planar.Polygon2D> externalPolygon2Ds, Tolerance.MacroDistance, Tolerance.Angle, tolerance);

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("externalPanels_Old");
            if (index != -1)
                dataAccess.SetDataList(index, externalPanels_Old?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("externalPanels_New");
            if (index != -1)
                dataAccess.SetDataList(index, externalPanels_New?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("externalPolygon2Ds");
            if (index != -1)
                dataAccess.SetDataList(index, externalPolygon2Ds);

            index = Params.IndexOfOutputParam("extendedPanels");
            if (index != -1)
                dataAccess.SetDataList(index, guids?.ConvertAll(x => new GooPanel(panels.Find(y => y.Guid == x))));
        }
    }
}