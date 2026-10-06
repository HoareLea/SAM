// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSplitPanelByElevations : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f31eaa6a-e9a8-4e85-9513-36f21ec32c1b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSplitPanelByElevations()
          : base("SAMAnalytical.SplitPanelByElevations", "SAMAnalytical.SplitPanelByElevations",
              "Split SAM Analytical Panel by Elevation or Plane, *aperture will be splited as well",
              "SAM", "Analytical03")
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elevations", NickName = "_elevations", Description = "Elevations or Planes", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "UpperPanels", NickName = "UpperPanels", Description = "Panels above given Elevation", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "LowerPanels", NickName = "LowerPanels", Description = "Panels below given Elevation", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_panel");
            Panel panel = null;
            if (index == -1 || !dataAccess.GetData(index, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            index = Params.IndexOfInputParam("_elevations");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Plane> planes = new List<Plane>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = objectWrapper.Value;

                if (@object is IGH_Goo)
                {
                    try
                    {
                        @object = (@object as dynamic).Value;
                    }
                    catch (Exception)
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
                else if (@object is global::Rhino.Geometry.Plane)
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

            List<Panel> result = Analytical.Query.Cut(panel, planes);

            if (result == null || result.Count == 0)
                result = new List<Panel>() { Create.Panel(panel) };

            List<Panel> result_Upper = new List<Panel>();
            List<Panel> result_Lower = new List<Panel>();
            foreach (Panel panel_Temp in result)
            {
                Point3D point3D = panel_Temp.GetInternalPoint3D();
                if (point3D == null)
                    continue;

                if (planes.TrueForAll(x => x.Above(point3D) || x.On(point3D)))
                    result_Upper.Add(panel_Temp);
                else
                    result_Lower.Add(panel_Temp);
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result?.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("UpperPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_Upper?.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("LowerPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_Lower?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
