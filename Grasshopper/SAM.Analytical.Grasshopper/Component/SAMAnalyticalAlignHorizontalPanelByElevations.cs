using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAlignHorizontalPanelByElevations : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c6950b1f-4fa1-42f7-a30b-57da0b6bbbb3");

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
        public SAMAnalyticalAlignHorizontalPanelByElevations()
          : base("SAMAnalytical.AlignHorizontalPanelByElevations", "SAMAnalytical.AlignHorizontalPanelByElevations",
              "Align Horizontal Panel to given Elevations",
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

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elevations", NickName = "_elevations", Description = "Elevations", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber = null;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Optional = true, Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(2);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Optional = true, Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panel", NickName = "panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_panel");
            Panel panel = null;
            if(index == -1 || !dataAccess.GetData(index, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_elevations");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            List<double> elevations = new List<double>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = objectWrapper.Value;
                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                double elevation = double.NaN;

                if (@object is double)
                {
                    elevation = (double)@object;
                }
                else if (@object is string)
                {
                    if (double.TryParse((string)@object, out double elevation_Temp))
                        elevation = elevation_Temp;
                }
                else if (@object is Architectural.Level)
                {
                    elevation = ((Architectural.Level)@object).Elevation;
                }

                if(!double.IsNaN(elevation))
                {
                    elevations.Add(elevation);
                }
            }

            index = Params.IndexOfInputParam("maxDistance_");
            double maxDistance = 2;
            if (index != -1)
                dataAccess.GetData(index, ref maxDistance);

            if (double.IsNaN(maxDistance))
                maxDistance = 2;

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            if (double.IsNaN(tolerance))
                tolerance = Tolerance.Distance;

            panel = Create.Panel(panel);

            if(panel.Horizontal(tolerance))
            {
                double panelElevation = panel.MinElevation();
                elevations.Sort((x, y) => System.Math.Abs(x - panelElevation).CompareTo(System.Math.Abs(y - panelElevation)));
                if (System.Math.Abs(elevations[0] - panelElevation) <= maxDistance)
                {
                    Geometry.Spatial.Vector3D vector3D = new Geometry.Spatial.Vector3D(0, 0, elevations[0] - panelElevation);
                    panel.Move(vector3D);
                }
            }

            dataAccess.SetData(0, new GooPanel(panel));
        }
    }
}