using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetExternalSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("426a7dc9-6377-45ed-a616-13668a722de0");

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
        public SAMAnalyticalSetExternalSpaces()
          : base("SAMAnalytical.SetExternalSpaces", "SAMAnalytical.SetExternalSpaces \n*you need to select in T3D surfaces that connect to External Space and - Reverse Building Element",
              "Sets ExternalSpaces for SAM AnalyticalModel",
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "_point3Ds", NickName = "_point3Ds", Description = "Space locations to be changed to ExternalSpaces", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction_Wall", NickName = "construction_Wall", Description = "Wall Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction_Floor", NickName = "construction_Floor", Description = "Floor Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction_Roof", NickName = "construction_Roof", Description = "Roof Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooExternalSpaceParam() { Name = "externalSpaces", NickName = "externalSpaces", Description = "SAM Analytical ExternalSpaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            index = Params.IndexOfInputParam("_point3Ds");
            if (index == -1 || !dataAccess.GetDataList(index, point3Ds) || point3Ds == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Construction construction_Wall = null;
            index = Params.IndexOfInputParam("construction_Wall");
            if(index != -1)
            {
                dataAccess.GetData(index, ref construction_Wall);
            }

            Construction construction_Floor = null;
            index = Params.IndexOfInputParam("construction_Floor");
            if (index != -1)
            {
                dataAccess.GetData(index, ref construction_Floor);
            }

            Construction construction_Roof = null;
            index = Params.IndexOfInputParam("construction_Roof");
            if (index != -1)
            {
                dataAccess.GetData(index, ref construction_Roof);
            }

            List<ExternalSpace> externalSpaces = null;

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                externalSpaces = adjacencyCluster.ChangeExternalSpaces(point3Ds, construction_Wall, construction_Floor, construction_Roof);
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("externalSpaces");
            if (index != -1)
            {
                dataAccess.SetDataList(index, externalSpaces?.ConvertAll(x => new GooExternalSpace(x)));
            }
        }
    }
}