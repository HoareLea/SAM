using GH_IO.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateSpace : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1571d29d-f01d-4f31-afbe-dd012b0f60c9");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateSpace()
          : base("SAMAnalytical.UpdateSpace ", "SAMAnalytical.UpdateSpace ",
              "Update SAM Analytical Space",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooSpaceParam(), "_space", "_space", "SAM Analytcial Space", GH_ParamAccess.item);

            index = inputParamManager.AddGenericParameter("_location", "_location", "Location", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddGenericParameter("name_", "name_", "Name", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSpaceParam(), "Space", "Space", "SAM Analytical Space", GH_ParamAccess.item);
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
            if (!dataAccess.GetData(0, ref space))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            dataAccess.GetData(2, ref name);

            Point3D location = null;
            GH_ObjectWrapper objectWrapper = null;
            if (dataAccess.GetData(1, ref objectWrapper))
            {
                if (objectWrapper.Value is GH_Point)
                    location = ((GH_Point)objectWrapper.Value).Value.ToSAM();
                else if(objectWrapper.Value is GH_Point3D)
                    location = ((GH_Point3D)objectWrapper.Value).ToSAM();
                else if(objectWrapper.Value is GooSAMGeometry)
                {
                    //location = (GooSAMGeometry)(objectWrapper.Value) as Point3D;
                }
            }

            if (name == null)
                name = space.Name;

            if (location == null)
                location = space.Location;

            Space space_Result = new Space(space, name, location);

            dataAccess.SetData(0, new GooSpace(space_Result));
        }
    }
}