using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetSpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("60861d78-829e-4ee0-b8bc-49be767d1de2");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetSpace()
          : base("SAMAnalytical.GetSpace", "SAMAnalytical.GetSpace",
              "Get Space from SAM Analytical Model",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAdjacencyClusterParam(), "_adjacencyCluster", "_adjacencyCluster", "SAM Analytical AdjacencyCluster", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("_point", "_point", "Geometry Point", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSpaceParam(), "Spaces", "Spaces", "SAM Geometry Spaces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AdjacencyCluster adjacencyCluster = null;
            if(!dataAccess.GetData(0, ref adjacencyCluster) || adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrappers = null;
            if (!dataAccess.GetData(1, ref objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Spatial.Point3D> point3Ds = new List<Geometry.Spatial.Point3D>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                Geometry.Spatial.Point3D point3D = null;

                if (objectWrapper.Value is GH_Point)
                {
                    point3D = ((GH_Point)objectWrapper.Value).ToSAM();
                }
                else if (objectWrapper.Value is Geometry.Spatial.Point3D)
                {
                    point3D = ((Geometry.Spatial.Point3D)objectWrapper.Value);
                }

                if (point3D != null)
                    point3Ds.Add(point3D);
            }

            Dictionary<Space, Geometry.Spatial.Shell> dictionary = adjacencyCluster.ShellDictionary();

            List<Space> spaces = new List<Space>();
            foreach(Geometry.Spatial.Point3D point3D in point3Ds)
            {
                Space space = null;
                foreach(KeyValuePair<Space, Geometry.Spatial.Shell> keyValuePair in dictionary)
                {
                    if(keyValuePair.Value.InRange(point3D))
                    {
                        space = keyValuePair.Key;
                        break;
                    }
                }
                spaces.Add(space);
            }

            dataAccess.SetDataList(0, spaces.ConvertAll(x => new GooSpace(x)));
        }
    }
}