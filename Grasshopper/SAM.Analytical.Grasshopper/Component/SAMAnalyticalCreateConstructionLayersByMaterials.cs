using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLayersByMaterials : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0ca969d3-3ad0-4d76-bd56-320be96df64b");

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
        public SAMAnalyticalCreateConstructionLayersByMaterials()
          : base("SAMAnalytical.CreateConstructionLayersByMaterials", "SAMAnalyticalCreate.ConstructionLayersByMaterials",
              "Create Construction Layers By Materials",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;
            
            inputParamManager.AddParameter(new GooMaterialParam(), "_materials", "_materials", "Materials", GH_ParamAccess.list);
            index = inputParamManager.AddNumberParameter("_thicknesses_", "_thicknesses_", "Contruction Layer Thicknesses [m]", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;
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
            List<IMaterial> materials = new List<IMaterial>();
            if (!dataAccess.GetDataList(0, materials))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> thicknesses = new List<double>();
            dataAccess.GetDataList(1, thicknesses);

            if(materials.Count != thicknesses.Count)
            {
                int count = thicknesses.Count;
                for (int i = 0; i < materials.Count - count; i++)
                {
                    Material material = materials[i + count] as Material;
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

            object[] objects = new object[materials.Count + thicknesses.Count];
            for (int i = 0; i < materials.Count; i++)
                objects[i] = materials[i].Name;

            for (int i = 0; i < thicknesses.Count; i++)
                objects[materials.Count + i] = thicknesses[i];

            List<ConstructionLayer> constructionLayers = Create.ConstructionLayers(objects);

            if (constructionLayers != null)
            {
                double thickness = constructionLayers.ConvertAll(x => x.Thickness).Sum();
                if (thickness >= 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Thickness of construction layers exceed 1m");
                    return;
                }
            }

            dataAccess.SetDataList(0, constructionLayers?.ConvertAll(x => new GooConstructionLayer(x)));
        }
    }
}