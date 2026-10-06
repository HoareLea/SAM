// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultConstructions : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ef93bf07-c910-4bbf-b76c-53b028640ac7");

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
        public SAMAnalyticalGetDefaultConstructions()
          : base("SAMAnalytical.GetDefaultConstructions", "SAMAnalytical.GetDefaultConstructions",
              "Get Default SAM Constructions",
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

                global::Grasshopper.Kernel.Parameters.Param_String param_String;
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_panelTypes_", NickName = "_panelTypes_", Description = "SAM PanelTypes", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "Constructions", NickName = "Construction", Description = "SAM Constructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<string> panelTypeStrings = new List<string>();
            index = Params.IndexOfInputParam("_panelTypes_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, panelTypeStrings);
            }

            List<PanelType> panelTypes = null;
            if (panelTypeStrings != null && panelTypeStrings.Count > 0)
            {
                panelTypes = new List<PanelType>();

                foreach (string panelTypeString in panelTypeStrings)
                {
                    PanelType panelType;
                    if (Enum.TryParse(panelTypeString, out panelType))
                        panelTypes.Add(panelType);
                }
            }
            else
            {
                panelTypes = new List<PanelType>(Enum.GetValues(typeof(PanelType)).Cast<PanelType>());
            }

            panelTypes?.RemoveAll(x => x == PanelType.Undefined || x == PanelType.Air);

            if (panelTypes == null || panelTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("Constructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panelTypes.ConvertAll(x => new GooConstruction(Analytical.Query.DefaultConstruction(x))));
            }
        }
    }
}
