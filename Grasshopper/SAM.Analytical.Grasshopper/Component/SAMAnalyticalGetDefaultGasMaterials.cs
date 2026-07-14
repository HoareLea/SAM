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
    public class SAMAnalyticalGetDefaultGasMaterials : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("bac6bca1-8fae-4cd5-b8c5-df360b3dda35");

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
        public SAMAnalyticalGetDefaultGasMaterials()
          : base("SAMAnalytical.GetDefaultGasMaterials", "SAMAnalytical.GetDefaultGasMaterials",
              "Get Default Gas Materials",
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
                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_defaultGasType_", NickName = "_defaultGasType_", Description = "SAM Analytical DefaultGasType", Access = GH_ParamAccess.list, Optional = true };
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
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "GasMaterials", NickName = "GasMaterials", Description = "SAM GasMaterials", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<string> defaultGasTypeStrings = new List<string>();
            index = Params.IndexOfInputParam("_defaultGasType_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, defaultGasTypeStrings);
            }

            List<DefaultGasType> defaultGasTypes = null;
            if (defaultGasTypeStrings != null && defaultGasTypeStrings.Count > 0)
            {
                defaultGasTypes = new List<DefaultGasType>();

                foreach (string panelTypeString in defaultGasTypeStrings)
                {
                    DefaultGasType defaultGasType;
                    if (Enum.TryParse(panelTypeString, out defaultGasType))
                        defaultGasTypes.Add(defaultGasType);
                }
            }
            else
            {
                defaultGasTypes = new List<DefaultGasType>(Enum.GetValues(typeof(DefaultGasType)).Cast<DefaultGasType>());
            }

            defaultGasTypes?.RemoveAll(x => x == DefaultGasType.Undefined);

            if (defaultGasTypes == null || defaultGasTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("GasMaterials");
            if (index != -1)
            {
                dataAccess.SetDataList(index, defaultGasTypes.ConvertAll(x => new GooMaterial(Analytical.Query.DefaultGasMaterial(x))));
            }
        }
    }
}
