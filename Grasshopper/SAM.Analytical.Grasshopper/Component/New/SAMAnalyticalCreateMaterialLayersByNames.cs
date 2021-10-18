using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

using SAM.Architectural.Grasshopper;
using SAM.Architectural;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateMaterialLayersByNames : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0e781669-2b9d-4a97-9ec2-9ea0a9dbc8d2");

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
        public SAMAnalyticalCreateMaterialLayersByNames()
          : base("SAMAnalytical.CreateMaterialLayersByNames", "SAMAnalytical.CreateMaterialLayersByNames",
              "Create Material Layers By Material Names",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;
            
            inputParamManager.AddTextParameter("_names", "_names", "Material Layer Name", GH_ParamAccess.list);
            
            index = inputParamManager.AddNumberParameter("_thicknesses_", "_thicknesses_", "Contruction Layer Thicknesses", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            GooMaterialLibraryParam gooMaterialLibraryParam = new GooMaterialLibraryParam();
            gooMaterialLibraryParam.Optional = true;
            inputParamManager.AddParameter(gooMaterialLibraryParam, "_materialLibrary_", "_materialLibrary_", "SAM Material Library", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialLayerParam(), "materialLayers", "materialLayers", "SAM Analytical Material Layers", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<string> names = new List<string>();
            if (!dataAccess.GetDataList(0, names))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> thicknesses = new List<double>();
            if (!dataAccess.GetDataList(1, thicknesses))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<MaterialLayer> materialLayers = new List<MaterialLayer>();

            int count = System.Math.Min(names.Count, thicknesses.Count);
            for (int i = 0; i < count; i++)
            {
                MaterialLayer materialLayer = new MaterialLayer(names[i], thicknesses[i]);
                materialLayers.Add(materialLayer);
            }

            dataAccess.SetDataList(0, materialLayers?.ConvertAll(x => new GooMaterialLayer(x)));
        }
    }
}