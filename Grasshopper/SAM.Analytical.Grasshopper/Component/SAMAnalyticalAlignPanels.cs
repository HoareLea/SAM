using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAlignPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("68f43a0e-abb0-40ec-9032-a2195366ceb1");

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
        public SAMAnalyticalAlignPanels()
          : base("SAMAnalytical.AlignPanels", "SAMAnalytical.AlignPanels",
              "Align Panels",
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

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_elevations", NickName = "elevations", Description = "Elevations", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_referenceElevation", NickName = "_referenceElevation", Description = "Reference Elevation", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.2);
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "referencePanels", NickName = "referencePanels", Description = "Reference SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            
            index = Params.IndexOfInputParam("_elevations");
            List<double> elevations = new List<double>(); 
            if(index == -1 || !dataAccess.GetDataList(index, elevations) || elevations == null || elevations.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_referenceElevation");
            double referenceElevation = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref referenceElevation) || double.IsNaN(referenceElevation))
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

            foreach (double elevation in elevations)
            {
                Analytical.Modify.Align(panels, elevation, referenceElevation, maxDistance, Tolerance.Angle, tolerance);
            }

            List<Panel> panels_Lower = null;
            List<Panel> panels_Upper = null;

            DataTree<GooPanel> dataTree_Panel = new DataTree<GooPanel>();
            for(int i=0; i < elevations.Count; i++)
            {
                GH_Path path = new GH_Path(i);

                List<Panel> panels_Temp = Geometry.Spatial.Query.FilterByElevation(panels, elevations[i], out panels_Lower, out panels_Upper, tolerance);
                panels_Temp?.ForEach(x => dataTree_Panel.Add(new GooPanel(x), path));
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataTree(index, dataTree_Panel);

            panels = Geometry.Spatial.Query.FilterByElevation(panels, referenceElevation, out panels_Lower, out panels_Upper, tolerance);

            index = Params.IndexOfOutputParam("referencePanels");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}