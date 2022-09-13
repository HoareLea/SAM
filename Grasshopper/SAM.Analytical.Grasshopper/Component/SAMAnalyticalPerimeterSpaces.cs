using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPerimeterSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("91da0eef-4d3d-43aa-9217-3df6cc490a6f");

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
        public SAMAnalyticalPerimeterSpaces()
          : base("SAMAnalytical.PerimeterSpaces", "SAMAnalytical.PerimeterSpaces",
              "Gets Perimeter Spaces",
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = null;

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_apertureCheck_", NickName = "_apertureCheck_", Description = "Panel has to have aperture to be recognized as external", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "in", NickName = "in", Description = "SAM Space recognized as perimeter spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "out", NickName = "out", Description = "SAM Space not recognized as perimeter spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalObject is AnalyticalModel ? ((AnalyticalModel)analyticalObject).AdjacencyCluster : analyticalObject as AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
            }


            bool apertureCheck = true;
            index = Params.IndexOfInputParam("_apertureCheck_");
            if (index != -1)
            {
                bool apertureCheck_Temp = true;
                if(dataAccess.GetData(index, ref apertureCheck_Temp))
                {
                    apertureCheck = apertureCheck_Temp;
                }
            }

            List<Space> spaces = null;
            index = Params.IndexOfInputParam("spaces_");
            if(index != -1)
            {
                List<Space> spaces_Temp = new List<Space>();

                if (dataAccess.GetDataList(index, spaces_Temp) && spaces_Temp != null && spaces_Temp.Count != 0)
                {
                    spaces = spaces_Temp;
                }
            }

            if (spaces == null)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            List<Space> @in = null;
            List<Space> @out = null;

            if (spaces != null && adjacencyCluster != null)
            {
                @in = new List<Space>();
                @out = new List<Space>();

                foreach (Space space in spaces)
                {
                    bool isPerimeter = adjacencyCluster.IsPerimeter(space, apertureCheck);

                    if(isPerimeter)
                    {
                        @in.Add(space);
                    }
                    else
                    {
                        @out.Add(space);
                    }
                }
            }

            index = Params.IndexOfOutputParam("in");
            if (index != -1)
            {
                dataAccess.SetDataList(index, @in?.ConvertAll(x => new GooSpace(x)));
            }

            index = Params.IndexOfOutputParam("out");
            if (index != -1)
            {
                dataAccess.SetDataList(index, @out?.ConvertAll(x => new GooSpace(x)));
            }
        }
    }
}