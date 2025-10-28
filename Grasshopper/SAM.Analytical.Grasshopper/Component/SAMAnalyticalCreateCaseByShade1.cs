using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateCaseByShade1 : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3ff7afc9-fe51-43d8-b42f-205c8d609a96");

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
        public SAMAnalyticalCreateCaseByShade1()
          : base("SAMAnalytical.CreateCaseByShade1", "SAMAnalytical.CreateCaseByShade1",
              "AAA",
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

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "_baseAModel", NickName = "_baseAModel", Description = "Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                GooPanelParam gooPanelParam = new GooPanelParam() { Name = "_shades_", NickName = "_shades_", Description = "Shades", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooPanelParam, ParamVisibility.Binding));


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

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "CaseAModel", NickName = "CaseAModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
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
            index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_shades_");
            List<Panel> shades = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, shades);
            }

            if (shades != null && shades.Count != 0)
            {
                AdjacencyCluster adjacencyCluster = new (analyticalModel.AdjacencyCluster, true);;

                foreach (Panel shade in shades)
                {
                    adjacencyCluster.AddObject(shade);
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}