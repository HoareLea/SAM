// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelTypeByText : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("aa788133-30d2-4622-94c2-342b80d438f9");

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
        public SAMAnalyticalPanelTypeByText()
          : base("SAMAnalytical.PanelTypeByText", "SAMAnalytical.PanelTypeByText",
              "Get PanelType By Text",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_text", NickName = "_text", Description = "Text", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "PanelType", NickName = "PanelType", Description = "SAM Analytical PanelType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            index = Params.IndexOfInputParam("_text");
            string text = null;
            if (index == -1 || !dataAccess.GetData(index, ref text) || string.IsNullOrEmpty(text))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = Analytical.Query.PanelType(text, true);
            if (panelType == PanelType.Undefined)
            {
                text = text.ToLower().Trim();
                if (text.Contains("roof"))
                {
                    panelType = PanelType.Roof;
                }
                else if (text.Contains("floor"))
                {
                    panelType = PanelType.Floor;
                    if (text.Contains("ext"))
                        panelType = PanelType.FloorExposed;
                    else if (text.Contains("int"))
                        panelType = PanelType.FloorInternal;
                    else if (text.Contains("grd"))
                        panelType = PanelType.SlabOnGrade;
                }
                else if (text.Contains("shd"))
                {
                    panelType = PanelType.Shade;
                }
                else if (text.Contains("sol"))
                {
                    panelType = PanelType.SolarPanel;

                }
                else
                {
                    panelType = PanelType.Wall;
                    if (text.Contains("ext"))
                        panelType = PanelType.WallExternal;
                    else if (text.Contains("int"))
                        panelType = PanelType.WallInternal;
                }
            }

            index = Params.IndexOfOutputParam("PanelType");
            if (index != -1)
            {
                dataAccess.SetData(index, panelType.ToString());
            }
        }
    }
}
