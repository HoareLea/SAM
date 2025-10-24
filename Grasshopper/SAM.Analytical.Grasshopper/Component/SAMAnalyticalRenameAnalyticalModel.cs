using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRenameAnalyticalModel : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5b1d7c8a-8e5f-40c5-8b4e-5f759e49e8d2");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRenameAnalyticalModel()
          : base("SAMAnalytical.RenameAnalyticalModel", "SAMAnalytical.RenameAnalyticalModel",
              "Rename AnalyticalModel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Name", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "caseDescription_", NickName = "caseDescription_", Description = "Case Description", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));



                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new GooAnalyticalModelParam() { Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                return [.. result];
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
            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_name");
            string name = null;
            if (index == -1 || !dataAccess.GetData(index, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("caseDescription_");
            string codeDescription = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref codeDescription);
            }

            if(analyticalModel != null)
            {
                analyticalModel = new AnalyticalModel(analyticalModel);
                analyticalModel.Name = name;

                if(codeDescription != null)
                {
                    Core.Modify.SetValue(analyticalModel, "CodeDescription", codeDescription);
                }
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}