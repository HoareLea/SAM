using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using SAM.Geometry.Grasshopper;
using Grasshopper.Kernel.Types;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateShellsByElevationsAndAuxiliaryElevations : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e26f2513-a0ab-42bd-a57a-c28b9c95bb41");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateShellsByElevationsAndAuxiliaryElevations()
          : base("SAMAnalytical.CreateShellsByElevationsAndAuxiliaryElevations", "SAMAnalytical.CreateShellsByElevationsAndAuxiliaryElevations",
              "Create Shells ",
              "SAM", "Analytical01")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_points_", NickName = "_points_", Description = "Points", Access = GH_ParamAccess.list, Optional = true };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "elevations_", NickName = "elevations_", Description = "Elevations", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offsets_", NickName = "offsets_", Description = "Offsets", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "auxiliaryElevations_", NickName = "auxiliaryElevations_", Description = "AuxiliaryElevations", Access = GH_ParamAccess.list, Optional = true };
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

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> elevations = new List<double>();
            index = Params.IndexOfInputParam("elevations_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, elevations);
            }

            List<double> offsets = new List<double>();
            index = Params.IndexOfInputParam("offsets_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, offsets);
            }

            List<double> auxiliaryElevations = new List<double>();
            index = Params.IndexOfInputParam("auxiliaryElevations_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, auxiliaryElevations);
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            List<Shell> shells = Analytical.Query.Shells(panels, elevations, offsets, auxiliaryElevations, Core.Tolerance.MacroDistance, Core.Tolerance.MacroDistance, Core.Tolerance.Angle, tolerance);

            index = Params.IndexOfInputParam("_points_");
            if(index != -1)
            {
                List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
                if(dataAccess.GetDataList(index, objectWrappers) && objectWrappers != null && objectWrappers.Count > 0)
                {
                    List<Point3D> point3Ds = new List<Point3D>();
                    foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                        if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Point3D> point3Ds_Temp) && point3Ds_Temp != null)
                            point3Ds.AddRange(point3Ds_Temp);

                    if(point3Ds != null && point3Ds.Count != 0)
                    {
                        bool duplicates = false;
                        List<Shell> shells_Temp = new List<Shell>();
                        foreach(Point3D point3D in point3Ds)
                        {
                            List<Shell> shells_InRange = shells.FindAll(x => x.InRange(point3D, tolerance) || x.Inside(point3D, tolerance: tolerance));
                            if (shells_InRange == null || shells_InRange.Count == 0)
                                continue;

                            foreach(Shell shell in shells_InRange)
                                if(!shells_Temp.Contains(shell))
                                {
                                    shells.Remove(shell);
                                    shells_Temp.Add(shell);
                                }
                                else
                                {
                                    duplicates = true;
                                }
                        }

                        shells = shells_Temp;

                        if(duplicates)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "There are multiple points enclosed in one shell");
                    }
                }
            }


            index = Params.IndexOfOutputParam("Shells");
            if (index != -1)
                dataAccess.SetDataList(index, shells?.ConvertAll(x => new GooSAMGeometry(x)));
        }
    }
}