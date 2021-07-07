using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLayersByNames : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("53f53e15-d92b-4515-b94d-0fa6fed4a785");

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
        public SAMAnalyticalCreateConstructionLayersByNames()
          : base("SAMAnalytical.CreateConstructionLayersByNames", "SAMAnalyticalCreate.ConstructionLayersByNames",
              "Create Construction Layers By Material Names",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;
            
            inputParamManager.AddTextParameter("_names", "_names", "Contruction Layer Name", GH_ParamAccess.list);
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
            outputParamManager.AddParameter(new GooConstructionLayerParam(), "ConstructionLayers", "ConstructionLayers", "SAM Analytical Construction Layers", GH_ParamAccess.list);
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
            dataAccess.GetDataList(1, thicknesses);

            if(names.Count != thicknesses.Count)
            {
                MaterialLibrary materialLibrary = null;
                dataAccess.GetData(2, ref materialLibrary);

                if (materialLibrary == null)
                    materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

                int count = thicknesses.Count;
                for (int i = 0; i < names.Count - count; i++)
                {
                    Material material = materialLibrary.GetObject<Material>(names[i + count]);
                    if(material == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }

                    double thickness = material.GetValue<double>(MaterialParameter.DefaultThickness);
                    if(double.IsNaN(thickness))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                        return;
                    }

                    thicknesses.Add(thickness);
                }
            }

            object[] objects = new object[names.Count + thicknesses.Count];
            for (int i = 0; i < names.Count; i++)
                objects[i] = names[i];

            for (int i = 0; i < thicknesses.Count; i++)
                objects[names.Count + i] = thicknesses[i];

            List<ConstructionLayer> constructionLayers = Create.ConstructionLayers(objects);

            dataAccess.SetDataList(0, constructionLayers?.ConvertAll(x => new GooConstructionLayer(x)));
        }
    }
}