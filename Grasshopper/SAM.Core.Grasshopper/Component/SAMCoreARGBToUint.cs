using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreARGBToUint : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6a4a2960-7785-422a-8e8d-28eee765d8f6");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreARGBToUint()
          : base("ARGBToUint", "ARGBToUint",
              "Converts Uint to ARGB",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = inputParamManager.AddIntegerParameter("a_", "a_", "Alpha", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddIntegerParameter("_r", "_r", "Red", GH_ParamAccess.item);
            inputParamManager.AddIntegerParameter("_g", "_g", "Green", GH_ParamAccess.item);
            inputParamManager.AddIntegerParameter("_b", "_b", "Blue", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddIntegerParameter("Uint", "Uint", "Uint", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int @a = int.MinValue;
            if (!dataAccess.GetData(0, ref @a))
                a = int.MinValue;

            int @r = int.MinValue;
            if (!dataAccess.GetData(1, ref @r))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            int @g = int.MinValue;
            if (!dataAccess.GetData(2, ref @g))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            int @b = int.MinValue;
            if (!dataAccess.GetData(3, ref @b))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            uint @uint = uint.MinValue;
            if (a == int.MinValue)
                @uint = Core.Convert.ToUint(System.Convert.ToByte(r), System.Convert.ToByte(g), System.Convert.ToByte(b));
            else
                @uint = Core.Convert.ToUint(System.Convert.ToByte(a), System.Convert.ToByte(r), System.Convert.ToByte(g), System.Convert.ToByte(b));

            dataAccess.SetData(0, System.Convert.ToInt32(@uint));
        }
    }
}