using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPaths : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("72cc05aa-ce8c-4fe3-ba30-26e2b8d17083");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalPaths()
          : base("SAMAnalytical.Paths", "SAMAnalytical.Paths",
              "Gets Analytical Paths",
              "SAM WIP", "Analytical")
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

                global::Grasshopper.Kernel.Parameters.Param_String number = null;

                string directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "SAM Simulation");

                number = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_directory_", NickName = "_directory_", Description = "Directory", Access = GH_ParamAccess.item };
                number.SetPersistentData(directory);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Project Name", Access = GH_ParamAccess.item };
                number.SetPersistentData("000000_SAM_AnalyticalModel");
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "json", NickName = "json", Description = "SAM Analytical Model Path", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "t3d", NickName = "t3d", Description = "T3D File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tbd", NickName = "tbd", Description = "TPD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tsd", NickName = "tsd", Description = "TSD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "tpd", NickName = "tpd", Description = "TPD File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_FilePath() { Name = "gem", NickName = "gem", Description = "GEM File Path", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_directory_");
            string directory = null;
            if (index != -1 || !dataAccess.GetData(index, ref directory) || string.IsNullOrWhiteSpace(directory))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index != -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.Create.Directory(directory);

            index = Params.IndexOfOutputParam("json");
            if (index != -1)
            {
                dataAccess.SetDataList(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", name, "json")));
            }

            index = Params.IndexOfOutputParam("t3d");
            if (index != -1)
            {
                dataAccess.SetDataList(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", name, "t3d")));
            }

            index = Params.IndexOfOutputParam("tbd");
            if (index != -1)
            {
                dataAccess.SetDataList(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", name, "tbd")));
            }

            index = Params.IndexOfOutputParam("tsd");
            if (index != -1)
            {
                dataAccess.SetDataList(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", name, "tsd")));
            }

            index = Params.IndexOfOutputParam("tpd");
            if (index != -1)
            {
                dataAccess.SetDataList(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", name, "tpd")));
            }

            index = Params.IndexOfOutputParam("gem");
            if (index != -1)
            {
                dataAccess.SetDataList(index, System.IO.Path.Combine(directory, string.Format("{0}.{1}", name, "gem")));
            }

        }
    }
}