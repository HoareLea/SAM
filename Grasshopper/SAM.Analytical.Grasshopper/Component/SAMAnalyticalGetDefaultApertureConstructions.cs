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
    public class SAMAnalyticalGetDefaultApertureConstructions : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("83e42fe9-79ff-4011-a3b0-3a7f69dabad5");

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
        public SAMAnalyticalGetDefaultApertureConstructions()
          : base("SAMAnalytical.GetDefaultApertureConstructions", "SAMAnalytical.GetDefaultApertureConstructions",
              "Get Default SAM ApertureConstructions",
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

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_apertureTypes_", NickName = "_apertureTypes_", Description = "SAM Analytical ApertureTypes", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_panelTypes_", NickName = "_panelTypes_", Description = "SAM Analytical PanelTypes", Access = GH_ParamAccess.list, Optional = true };
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
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "ApertureConstructions", NickName = "ApertureConstructions", Description = "SAM Analytical Aperture Constructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<string> values = null;

            values = new List<string>();
            index = Params.IndexOfInputParam("_apertureTypes_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, values);
            }

            List<ApertureType> apertureTypes = null;
            if (values != null && values.Count > 0)
            {
                apertureTypes = new List<ApertureType>();

                foreach (string value in values)
                {
                    ApertureType apertureType;
                    if (Enum.TryParse(value, out apertureType))
                        apertureTypes.Add(apertureType);
                }
            }
            else
            {
                apertureTypes = new List<ApertureType>(Enum.GetValues(typeof(ApertureType)).Cast<ApertureType>());
            }

            apertureTypes?.RemoveAll(x => x == ApertureType.Undefined);

            if (apertureTypes == null || apertureTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            values = new List<string>();
            index = Params.IndexOfInputParam("_panelTypes_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, values);
            }

            List<PanelType> panelTypes = null;
            if (values != null && values.Count > 0)
            {
                panelTypes = new List<PanelType>();

                foreach (string value in values)
                {
                    PanelType panelType;
                    if (Enum.TryParse(value, out panelType))
                        panelTypes.Add(panelType);
                }
            }
            else
            {
                panelTypes = new List<PanelType>(Enum.GetValues(typeof(PanelType)).Cast<PanelType>());
            }

            panelTypes?.RemoveAll(x => x == PanelType.Undefined);

            if (panelTypes == null || panelTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();

            List<GooApertureConstruction> gooApertureConstructions = new List<GooApertureConstruction>();
            foreach (ApertureType apertureType in apertureTypes)
                foreach (PanelType panelType in panelTypes)
                {
                    ApertureConstruction apertureConstruction = Analytical.Query.DefaultApertureConstruction(panelType, apertureType);
                    if (apertureConstruction == null)
                        continue;

                    if (apertureConstructions.Find(x => x.Guid.Equals(apertureConstruction.Guid)) == null)
                        apertureConstructions.Add(apertureConstruction);
                }

            index = Params.IndexOfOutputParam("ApertureConstructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertureConstructions?.ConvertAll(x => new GooApertureConstruction(x)));
            }
        }
    }
}
