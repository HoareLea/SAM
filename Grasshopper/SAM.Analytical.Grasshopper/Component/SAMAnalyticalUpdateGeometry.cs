// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8aab6290-b5bc-4052-ad5e-21ff4254946e");

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
        public SAMAnalyticalUpdateGeometry()
          : base("SAMAnalytical.UpdateGeometry", "SAMAnalytical.UpdateGeometry",
              "Update Geometry",
              "SAM", "Analytical04")
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry", NickName = "_geometry", Description = "Geometry", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_copyApertures_", NickName = "_copyApertures_", Description = "Copy apertures if possible", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includeInternalEdges_", NickName = "_includeInternalEdges_", Description = "Include internal Edges if exist", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "simplify_", NickName = "simplify_", Description = "Simplify", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Acceptable area of Aperture", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panel", NickName = "Panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_panel");
            Panel panel = null;
            if (index == -1 || !dataAccess.GetData(index, ref panel))
                return;

            index = Params.IndexOfInputParam("_geometry");
            object @object = null;
            if (index == -1 || !dataAccess.GetData(index, ref @object))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("simplify_");
            bool simplify = true;
            if (index == -1 || !dataAccess.GetData(index, ref simplify))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = null;
            if (!Query.TryConvertToPanelGeometries(@object, out geometry3Ds, simplify))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("minArea_");
            double minArea = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref minArea))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_copyApertures_");
            bool copyApertures = true;
            if (index == -1 || !dataAccess.GetData(index, ref copyApertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_includeInternalEdges_");
            bool includeInternalEdges = true;
            if (index == -1 || !dataAccess.GetData(index, ref includeInternalEdges))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!includeInternalEdges)
            {
                for (int i = 0; i < geometry3Ds.Count; i++)
                {
                    Face3D face3D = geometry3Ds[i] as Face3D;
                    if (face3D == null)
                        continue;

                    geometry3Ds[i] = new Face3D(face3D.GetExternalEdge3D());
                }
            }

            List<Panel> panels = Create.Panels(geometry3Ds, panel.PanelType, panel.Construction, minArea, tolerance);
            if (panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid geometry for panel");
                return;
            }

            panels[0] = Create.Panel(panel.Guid, panel, panels[0].GetFace3D(), null, true, minArea);
            if (!copyApertures)
                panels[0].RemoveApertures();

            if (panels.Count > 1)
            {
                for (int i = 1; i < panels.Count; i++)
                {
                    panels[i] = Create.Panel(panels[i].Guid, panel, panels[i].GetFace3D(), null, true, minArea);
                    if (!copyApertures)
                        panels[i].RemoveApertures();
                }
            }

            index = Params.IndexOfOutputParam("Panel");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
