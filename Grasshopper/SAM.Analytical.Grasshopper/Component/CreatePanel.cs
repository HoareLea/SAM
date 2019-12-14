using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Analytical.Grasshopper.Properties;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class CreatePanel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public CreatePanel()
          : base("CreatePanel", "CrPnl",
              "CreatePanel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Geometry", "geo", "Geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Panel", "pnl", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object obj = objectWrapper.Value;

            Polygon3D polygon3D = null;

            if (obj is IGH_GeometricGoo)
                obj = ((IGH_GeometricGoo)obj).ToSAM();

            if (!(obj is IGeometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            polygon3D = obj as Polygon3D;

            if (obj == null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
            else
                dataAccess.SetData(0, new Panel(Guid.NewGuid(), null, polygon3D));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Small;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("35ef8f3e-1cf2-407d-b2ed-33bf371ea161"); }
        }
    }
}