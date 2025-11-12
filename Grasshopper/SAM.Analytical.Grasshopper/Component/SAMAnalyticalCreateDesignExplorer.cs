using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateDesignExplorer : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new ("5324c64c-7cd4-4298-bcab-d780275c278a");

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
        public SAMAnalyticalCreateDesignExplorer()
          : base("SAMAnalytical.CreateDesignExplorer", "SAMAnalytical.CreateDesignExplorer",
              "CreateDesignExplorer",
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

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "_analyticalModels", NickName = "_analyticalModels", Description = "Analytical Models", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                Param_FilePath param_FilePath = new Param_FilePath
                {
                    Name = "_path",
                    NickName = "_path",
                    Description = "Design Explorer file path",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_FilePath.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_FilePath, ParamVisibility.Binding));

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

                Param_String param_String = new() { Name = "lines", NickName = "lines", Description = "Lines", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

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
            index = Params.IndexOfInputParam("_analyticalModels");
            List<AnalyticalModel> analyticalModels = [];
            if (index == -1 || !dataAccess.GetDataList(index, analyticalModels) || analyticalModels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_path");
            string path = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref path);
            }

            List<string> lines = Analytical.Convert.ToDesignExplorer(analyticalModels);

            if(lines != null && !string.IsNullOrWhiteSpace(path))
            {
                System.IO.File.WriteAllLines(path, lines);
            }

            index = Params.IndexOfOutputParam("lines");
            if (index != -1)
            {
                dataAccess.SetData(index, lines);
            }
        }
    }
}