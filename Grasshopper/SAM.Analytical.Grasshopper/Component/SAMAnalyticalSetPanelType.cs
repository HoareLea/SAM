// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("04a7a588-a3e2-4ad7-868e-d432140a5b05");

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
        public SAMAnalyticalSetPanelType()
          : base("SAMAnalytical.SetPanelType", "SAMAnalytical.SetPanelType",
              "Sets Panel Type for Analytical Panel",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_panelType", NickName = "_panelType", Description = "SAM Analytical Panel Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_setDefaultConstruction_", NickName = "_setDefaultConstruction_", Description = "Set Default Construction", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_setDefaultApertureConstruction_", NickName = "_setDefaultApertureConstruction_", Description = "Set Default Aperture Construction", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panel", NickName = "Panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            if (index == -1 || !dataAccess.GetData(index, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_panelType");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_setDefaultConstruction_");
            bool setDefaultConstruction = false;
            if (index == -1 || !dataAccess.GetData(index, ref setDefaultConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_setDefaultApertureConstruction_");
            bool setDefaultApertureConstruction = false;
            if (index == -1 || !dataAccess.GetData(index, ref setDefaultApertureConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object value = objectWrapper.Value;

            if (value is GH_String)
                value = ((GH_String)value).Value;

            PanelType panelType = Analytical.Query.PanelType(value);

            Panel panel_New = Create.Panel(panel, panelType);
            if (setDefaultConstruction)
            {
                Construction construction = Analytical.Query.DefaultConstruction(panelType);
                panel_New = Create.Panel(panel_New, construction);
            }

            if (setDefaultApertureConstruction)
            {
                List<Aperture> apertures = panel_New.Apertures;
                if (apertures != null && apertures.Count != 0)
                {
                    foreach (Aperture aperture in apertures)
                    {
                        ApertureConstruction apertureConstruction = Analytical.Query.DefaultApertureConstruction(panel_New.PanelType, aperture.ApertureType);
                        if (apertureConstruction == null)
                            continue;

                        panel_New.RemoveAperture(aperture.Guid);

                        Aperture aperture_New = new Aperture(aperture, apertureConstruction);
                        panel_New.AddAperture(aperture_New);
                    }
                }
            }

            index = Params.IndexOfOutputParam("Panel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooPanel(panel_New));
            }
        }
    }
}
