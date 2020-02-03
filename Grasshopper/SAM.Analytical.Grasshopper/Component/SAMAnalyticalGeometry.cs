using System;
using System.Collections;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGeometry : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6628ec6c-84ba-4f2b-97cf-f2ccdbe8599a");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGeometry()
          : base("SAMAnalytical.Geometry", "SAMAnalytical.Geometry",
              "Convert SAM Analitical to GH Geometry ie. Panel to Surface",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new Core.Grasshopper.GooSAMObjectParam<Core.SAMObject>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGeometryParameter("Geometry", "Geometry", "Geometry in GH ie.Surface", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            SAMObject sAMObject = null;
            if(!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                dataAccess.SetData(0, ((Panel)sAMObject).PlanarBoundary3D.ToGrasshopper());
                return;
            }

            if(sAMObject is PlanarBoundary3D)
            {
                dataAccess.SetData(0, ((PlanarBoundary3D)sAMObject).ToGrasshopper());
                return;
            }

            if (sAMObject is Space)
            {
                dataAccess.SetData(0, ((Space)sAMObject).ToGrasshopper());
                return;
            }

            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Object");
        }
    }
}