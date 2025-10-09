using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAlignSpaceLocation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e18c5ea9-0f5a-4d14-9b93-9c5e991e7946");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Aligns the space location point with the analytical model.
        /// If the space location point is not positioned on a floor panel,
        /// the component projects a vector in the -Z direction to find the nearest panel hit
        /// and adjusts the Z coordinate of the location point accordingly.
        /// </summary>
        public SAMAnalyticalAlignSpaceLocation()
          : base("SAMAnalytical.AlignSpaceLocation", "SAMAnalytical.AlignSpaceLocation",
              "Aligns the space location point with the analytical model.\n" +
              "If the space location point is not positioned on a floor panel,\n" +
              "a vector is projected in the -Z direction to find the nearest panel hit,\n" +
              "and the Z coordinate of the location point is adjusted accordingly.",
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooAnalyticalObjectParam analyticalObjectParam = new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster, AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalObjectParam, ParamVisibility.Binding));

                GooSpaceParam gooSpaceParam = new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "Spaces", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooSpaceParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Successful", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index_Successful = Params.IndexOfOutputParam("successful");
            if(index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            int index = -1;

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analyticalObject");
            if (!dataAccess.GetData(index, ref analyticalObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AdjacencyCluster adjacencyCluster_Temp)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster_Temp);
            }
            else if(analyticalObject is AnalyticalModel analyticalModel)
            {
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces = [];
            index = Params.IndexOfInputParam("spaces_");
            if (index == -1 || !dataAccess.GetDataList(index, spaces))
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            bool successful = false;
            foreach (Space space in spaces)
            {
                Space space_Temp = adjacencyCluster.GetObject<Space>(x => x.Guid == space.Guid);
                if(space_Temp == null)
                {
                    continue;
                }

                Vector3D vector3D = new Vector3D(0, 0, -1);

                Point3D location = space_Temp.Location;

                List<Panel> panels =adjacencyCluster.GetPanels(space);
                if(panels == null || !panels.Any())
                {
                    continue;
                }

                List<Face3D> face3Ds = panels.ConvertAll(x => x.GetFace3D(false));
                if(face3Ds == null || !face3Ds.Any())
                {
                    continue;
                }

                if(face3Ds.Find(x => x.On(location)) != null)
                {
                    continue;
                }

                Dictionary<Face3D, Point3D> dictionary = Geometry.Spatial.Query.IntersectionDictionary(location, vector3D, face3Ds, true, true);
                if(dictionary == null || !dictionary.Any())
                {
                    continue;
                }

                adjacencyCluster.AddObject(new Space(space_Temp, space_Temp.Name, dictionary.Values.First()));
                successful = true;
            }

            index = Params.IndexOfOutputParam("analyticalObject");
            if(index != -1)
            {
                if (analyticalObject is AdjacencyCluster)
                {
                    dataAccess.SetData(index, adjacencyCluster);
                }
                else if (analyticalObject is AnalyticalModel analyticalModel)
                {
                    AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                    dataAccess.SetData(index, analyticalModel_Result);
                }
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, successful);
            }
        }
    }
}