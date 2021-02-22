using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using SAM.Geometry.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateShells : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0bd2351f-f558-471a-b212-05c757c0e33f");

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
        public SAMAnalyticalCreateShells()
          : base("SAMAnalytical.CreateShells", "SAMAnalytical.CreateShells",
              "Create Shells ",
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

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Offset", Access = GH_ParamAccess.item };
                number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Shells", NickName = "Shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_geometries");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("offset_");
            double offset = Core.Tolerance.Distance;
            if (index != -1)
            {
                double offset_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref offset_Temp) && !double.IsNaN(offset_Temp))
                    offset = offset_Temp;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            List<Shell> shells = Analytical.Query.Shells(panels, offset, tolerance);

            index = Params.IndexOfInputParam("Shells");
            if (index != -1)
                dataAccess.SetDataList(index, shells?.ConvertAll(x => new GooSAMGeometry(x)));
        }
    }
}