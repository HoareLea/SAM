using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

using SAM.Architectural.Grasshopper;
using SAM.Architectural;

namespace SAM.Analytical.Grasshopper
{
    [Obsolete("Obsolete since 2021.11.24")]
    public class SAMAnalyticalCreateHostPartitionType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0e389d23-0ec7-4da9-a554-1f6d965fc9e9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateHostPartitionType()
          : base("SAMAnalytical.CreateHostPartitionType", "SAMAnalytical.CreateHostPartitionType",
              "Create SAM Analytical HostPartitionType",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_hostPartitionCategory_", "_hostPartitionCategory_", "SAM Architecture HostPartitionCategory", GH_ParamAccess.item, HostPartitionCategory.Wall.ToString());
            inputParamManager.AddParameter(new GooMaterialLayerParam(), "_materialLayers", "_materialLayers", "SAM Architectural Material Layers", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooHostPartitionTypeParam(), "hostPartitionType", "hostPartitionType", "SAM Architectural HostPartitionType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = null;
            if (!dataAccess.GetData(0, ref name) || string.IsNullOrEmpty(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            HostPartitionCategory hostPartitionCategory = HostPartitionCategory.Undefined;

            GH_ObjectWrapper objectWrapper = null;
            dataAccess.GetData(1, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    hostPartitionCategory = Core.Query.Enum<HostPartitionCategory>(((GH_String)objectWrapper.Value).Value);
                else
                    hostPartitionCategory = Core.Query.Enum<HostPartitionCategory>(objectWrapper.Value.ToString());
            }

            List<MaterialLayer> materialLayers = new List<MaterialLayer>();
            if (!dataAccess.GetDataList(2, materialLayers) || materialLayers == null || materialLayers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            HostPartitionType hostPartitionType = Create.HostPartitionType(hostPartitionCategory, name, materialLayers);

            dataAccess.SetData(0, new GooHostPartitionType(hostPartitionType));
        }
    }
}