using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTransform : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("41a52d4f-159e-4e30-bb4c-167b259eeb7d");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalTransform()
          : base("SAMAnalytical.Transform", "SAMAnalytical.Transform",
              "Transform Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooTransform3DParam(), "_transform", "_transform", "SAM Transform", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Panel", "Panel", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Transform3D transform3D = null;
            if (!dataAccess.GetData(1, ref transform3D))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(sAMObject is Panel)
            {
                Panel panel_New = new Panel((Panel)sAMObject);
                panel_New.Transform(transform3D);
                dataAccess.SetData(0, new GooPanel(panel_New));
            }
            else if(sAMObject is Aperture)
            {
                Aperture panel_Aperture = new Aperture((Aperture)sAMObject);
                panel_Aperture.Transform(transform3D);
                dataAccess.SetData(0, new GooAperture(panel_Aperture));
            }
            
        }
    }
}