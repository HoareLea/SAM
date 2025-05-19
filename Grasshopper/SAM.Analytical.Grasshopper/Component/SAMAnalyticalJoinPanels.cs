using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalJoinPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8a229d7b-6cce-4a28-8be9-07dd8c17aee5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalJoinPanels()
          : base("SAMAnalytical.JoinPanels", "SAMAnalytical.JoinPanels",
              "Join SAM Analytical Panels",
              "SAM", "Analytical02")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elevations", NickName = "elevations", Description = "Elevations", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "distance_", NickName = "distance_", Description = "Distance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.2);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Offset", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "extendedPanels", NickName = "extendedPanels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "trimmedPanels", NickName = "trimmedPanels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "segment3Ds", NickName = "segment3Ds", Description = "SAM Geometry Segment3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_elevations");
            List<GH_ObjectWrapper> objectWrappers_Elevation = new List<GH_ObjectWrapper>() ;
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers_Elevation) || objectWrappers_Elevation == null || objectWrappers_Elevation.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            List<double> elevations = new List<double>();

            foreach(GH_ObjectWrapper objectWrapper_Elevation in objectWrappers_Elevation)
            {
                object @object = objectWrapper_Elevation.Value;
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

                if(double.IsNaN(elevation))
                {
                    continue;
                }

                elevations.Add(elevation);
            }

            index = Params.IndexOfInputParam("distance_");
            double distance = 0.2;
            if (index != -1)
                dataAccess.GetData(index, ref distance);

            if (double.IsNaN(distance))
                distance = 0.2;

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            if (double.IsNaN(tolerance))
                tolerance = Tolerance.Distance;

            index = Params.IndexOfInputParam("offset_");
            double offset = Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref offset);

            if (double.IsNaN(offset))
                offset = Tolerance.MacroDistance;

            List<Panel> panels_Extended = new List<Panel>();
            List<Panel> panels_Trimmed = new List<Panel>();
            List<Geometry.Spatial.Segment3D> segment3Ds = new List<Geometry.Spatial.Segment3D>();
            foreach (double elevation in elevations)
            {
                Analytical.Modify.Join(panels, elevation + offset, distance, out List<Panel> panels_Extended_Temp, out List<Panel> panels_Trimmed_Temp, out List<Geometry.Spatial.Segment3D> segment3Ds_Temp, Tolerance.MacroDistance, Tolerance.Angle, tolerance);

                if(panels_Extended_Temp != null)
                {
                    foreach(Panel panel_Extended in panels_Extended_Temp)
                    {
                        int i = panels_Extended.FindIndex(x => x.Guid == panel_Extended.Guid);
                        if(i == -1)
                        {
                            panels_Extended.Add(panel_Extended);
                        }
                        else
                        {
                            panels_Extended[i] = panel_Extended;
                        }
                    }
                }

                if (panels_Trimmed_Temp != null)
                {
                    foreach (Panel panel_Trimmed in panels_Trimmed_Temp)
                    {
                        int i = panels_Trimmed.FindIndex(x => x.Guid == panel_Trimmed.Guid);
                        if (i == -1)
                        {
                            panels_Trimmed.Add(panel_Trimmed);
                        }
                        else
                        {
                            panels_Trimmed[i] = panel_Trimmed;
                        }
                    }
                }

                if(segment3Ds_Temp != null)
                {
                    segment3Ds.AddRange(segment3Ds_Temp);
                }
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("extendedPanels");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Extended?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("trimmedPanels");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Trimmed?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("segment3Ds");
            if (index != -1)
                dataAccess.SetDataList(index, segment3Ds);
        }
    }
}