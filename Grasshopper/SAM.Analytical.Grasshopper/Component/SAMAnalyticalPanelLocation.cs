// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelLocation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("161f6ee4-60a3-4699-ade7-a6fbe9a1e222");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPanelLocation()
          : base("SAMAnalytical.PanelLocation", "SAMAnalytical.PanelLocation",
              "Location of SAM Analytical Panel",
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "Origin", NickName = "Origin", Description = "Origin Point", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Tilt", NickName = "Tilt", Description = "Tilt of SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Azimuth", NickName = "Azimuth", Description = "Azimuth of SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Vector() { Name = "Normal", NickName = "Normal", Description = "Normal of SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "InternalPoint", NickName = "InternalPoint", Description = "InternalPoint of SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_panel");
            Panel panel = null;
            if (index == -1 || !dataAccess.GetData(index, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Plane plane = panel.Plane;
            if (panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Origin = Params.IndexOfOutputParam("Origin");
            if (index_Origin != -1)
            {
                dataAccess.SetData(index_Origin, Geometry.Rhino.Convert.ToRhino(plane.Origin));
            }

            int index_Tilt = Params.IndexOfOutputParam("Tilt");
            if (index_Tilt != -1)
            {
                dataAccess.SetData(index_Tilt, panel.Tilt());
            }

            int index_Azimuth = Params.IndexOfOutputParam("Azimuth");
            if (index_Azimuth != -1)
            {
                dataAccess.SetData(index_Azimuth, panel.Azimuth());
            }

            int index_Normal = Params.IndexOfOutputParam("Normal");
            if (index_Normal != -1)
            {
                dataAccess.SetData(index_Normal, Geometry.Rhino.Convert.ToRhino(plane.Normal));
            }

            int index_InternalPoint = Params.IndexOfOutputParam("InternalPoint");
            if (index_InternalPoint != -1)
            {
                dataAccess.SetData(index_InternalPoint, Geometry.Rhino.Convert.ToRhino(panel.GetInternalPoint3D()));
            }
        }
    }
}
