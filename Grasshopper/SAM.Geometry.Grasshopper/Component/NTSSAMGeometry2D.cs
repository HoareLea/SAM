using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class NTSSAMGeometry2D : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("93610b4d-c36a-4294-9290-27bc4c9afca2");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public NTSSAMGeometry2D()
          : base("NTS.SAMGeometry2D", "NTS.SAMGeometry2D",
              "NetTopologySuite To SAM Geometry",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_NTS", "NTS", "NTS Text", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMGeometryParam(), "Geometry2Ds", "Geometry2Ds", "SAM Geometry 2D", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<string> lines_NTS = new List<string>();
            if (!dataAccess.GetDataList(0, lines_NTS))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry2D> geometry2Ds = Geometry.Convert.ToSAM(lines_NTS);
            if(geometry2Ds == null)
                dataAccess.SetDataList(0, null);
            else
                dataAccess.SetDataList(0, geometry2Ds.ConvertAll(x => new GooSAMGeometry(x)));


        }
    }
}