using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultGasMaterials : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("bac6bca1-8fae-4cd5-b8c5-df360b3dda35");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetDefaultGasMaterials()
          : base("SAMAnalytical.GetDefaultGasMaterials", "SAMAnalytical.GetDefaultGasMaterials",
              "Get Default Gas Materials",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = inputParamManager.AddTextParameter("_defaultGasType_", "_defaultGasType_", "SAM Analytical DefaultGasType", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialParam(), "GasMaterials", "GasMaterials", "SAM GasMaterials", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<string> defaultGasTypeStrings = new List<string>();
            dataAccess.GetDataList(0, defaultGasTypeStrings);

            List<DefaultGasType> defaultGasTypes = null;
            if(defaultGasTypeStrings != null && defaultGasTypeStrings.Count > 0)
            {
                defaultGasTypes = new List<DefaultGasType>();

                foreach(string panelTypeString in defaultGasTypeStrings)
                {
                    DefaultGasType defaultGasType;
                    if (Enum.TryParse(panelTypeString, out defaultGasType))
                        defaultGasTypes.Add(defaultGasType);
                }
            }
            else
            {
                defaultGasTypes = new List<DefaultGasType>(Enum.GetValues(typeof(DefaultGasType)).Cast<DefaultGasType>());
            }

            defaultGasTypes?.RemoveAll(x => x == DefaultGasType.Undefined);

            if(defaultGasTypes == null || defaultGasTypes.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetDataList(0, defaultGasTypes.ConvertAll(x => new GooMaterial(Analytical.Query.DefaultGasMaterial(x))));
        }
    }
}