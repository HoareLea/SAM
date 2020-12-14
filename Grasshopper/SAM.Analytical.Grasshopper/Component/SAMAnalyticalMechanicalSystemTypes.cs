using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalMechanicalSystemTypes : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0b370d90-3c1b-4b20-8181-0a751611652e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalMechanicalSystemTypes()
          : base("SAMAnalytical.SystemTypes", "SAMAnalytical.SystemTypes",
              "Gets Analytical System Types",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);

            GooSystemTypeLibraryParam gooSystemTypeLibraryParam = new GooSystemTypeLibraryParam();
            gooSystemTypeLibraryParam.Optional = true;
            inputParamManager.AddParameter(gooSystemTypeLibraryParam, "_systemTypeLibrary_", "_systemTypeLibrary_", "SAM SystemTypeLibrary", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSystemTypeParam(), "Ventilation", "Ventilation", "SAM Analytical Ventilation System Type", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooSystemTypeParam(), "Heating", "Heating", "SAM Analytical Heating System Type", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooSystemTypeParam(), "Cooling", "Cooling", "SAM Analytical Cooling System Type", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SystemTypeLibrary systemTypeLibrary = null;
            dataAccess.GetData(1, ref systemTypeLibrary);

            if (systemTypeLibrary == null)
                systemTypeLibrary = ActiveSetting.Setting.GetValue<SystemTypeLibrary>(AnalyticalSettingParameter.DefaultSystemTypeLibrary);

            InternalCondition internalCondition = null;

            if(sAMObject is InternalCondition)
                internalCondition = (InternalCondition)sAMObject;
            else if(sAMObject is Space)
                internalCondition = ((Space)sAMObject).InternalCondition;

            if(internalCondition == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooSystemType(internalCondition.GetSystemType<VentilationSystemType>(systemTypeLibrary)));
            dataAccess.SetData(0, new GooSystemType(internalCondition.GetSystemType<HeatingSystemType>(systemTypeLibrary)));
            dataAccess.SetData(0, new GooSystemType(internalCondition.GetSystemType<CoolingSystemType>(systemTypeLibrary)));
        }
    }
}