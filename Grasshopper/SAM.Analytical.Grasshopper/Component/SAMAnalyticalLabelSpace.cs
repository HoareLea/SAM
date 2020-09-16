using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalLabelSpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("432dfea1-3242-4540-816e-d65bf1b28e4a");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalLabelSpace()
          : base("SAMAnalytical.LabelSpace", "SAMAnalytical.LabelSpace",
              "Label SAM Analytical Space",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytical Space", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_name_", "_name_", "Parameter Name", GH_ParamAccess.item, "Name");
            
            index = inputParamManager.AddNumberParameter("_height_", "_height_", "Text Height", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            int index = outputParamManager.AddParameter(new GooText3DParam(), "Value", "Value", "Value", GH_ParamAccess.item);
            outputParamManager.HideParameter(index);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Space space = null;
            if(!dataAccess.GetData(0, ref space))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            dataAccess.GetData(1, ref name);
            if (string.IsNullOrEmpty(name))
                name = "Name";

            string value = null;
            if (!space.TryGetValue(name, out value, true))
                value = "???";

            double height = double.NaN;
            if (!dataAccess.GetData(2, ref height))
            {
                int length = value.Length;
                if (length < 10)
                    length = 10;

                height = 1;
            }

            Point3D point3D = space.Location;
            if(point3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not find proper location of label");
                return;
            }

            Vector3D normal = Plane.WorldXY.Normal;
            if (normal.Z < 0)
                normal.Negate();

            Plane plane = new Plane(point3D, normal);


            Rhino.Display.Text3d text3D = new Rhino.Display.Text3d(value, plane.ToRhino(), height);

            dataAccess.SetData(0, new GooText3D(text3D));
        }
    }
}