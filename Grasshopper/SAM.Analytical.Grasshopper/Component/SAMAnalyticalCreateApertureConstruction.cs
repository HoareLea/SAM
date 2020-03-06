using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateApertureConstruction : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("92dadf21-4b38-4f76-8e24-3de677eaeb1a");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateApertureConstruction()
          : base("SAMAnalytical.CreateApertureConstruction", "SAMAnalyticalCreate.ApertureConstruction",
              "Create Aperture Construction, if nothing connect default values: _name = WinInst: SIM_EXT_GLZ ",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name_", "_name_", "Name", GH_ParamAccess.item, "WinInst: SIM_EXT_GLZ");
            int index = inputParamManager.AddGenericParameter("_apertureType", "_apertureType", "Aperture Type", GH_ParamAccess.item);
            
            Param_GenericObject genericObjectParameter = (Param_GenericObject)inputParamManager[index];
            genericObjectParameter.PersistentData.Append(new GH_ObjectWrapper(ApertureType.Window.ToString()));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooApertureConstructionParam(), "ApertureConstruction", "ApertureConstruction", "SAM Analytical Aperture Construction", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData<string>(0, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ApertureType apertureType = ApertureType.Undefined;

            GH_ObjectWrapper objectWrapper = null;
            dataAccess.GetData(1, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    apertureType = Query.ApertureType(((GH_String)objectWrapper.Value).Value);
                else
                    apertureType = Query.ApertureType(objectWrapper.Value);
            }

            dataAccess.SetData(0, new GooApertureConstruction(new ApertureConstruction(name, apertureType)));
        }
    }
}