using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLayersByMaterials : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0ca969d3-3ad0-4d76-bd56-320be96df64b");

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
        public SAMAnalyticalCreateConstructionLayersByMaterials()
          : base("SAMAnalytical.CreateConstructionLayersByMaterials", "SAMAnalyticalCreate.ConstructionLayersByMaterials",
              "Create Construction Layers By Materials",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                GooMaterialParam gooMaterialParam = new GooMaterialParam() { Name = "_materials", NickName = "_materials", Description = "SAM Materials", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooMaterialParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_thicknesses_", NickName = "_thicknesses_", Description = "Contruction Layer Thicknesses [m]", Optional = true, Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooConstructionLayerParam() { Name = "constructionLayers", NickName = "constructionLayers", Description = "SAM Analytical Construction Layers", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "materials", NickName = "materials", Description = "SAM Materials", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;


            List<IMaterial> materials = new List<IMaterial>();
            index = Params.IndexOfInputParam("_materials");
            if (index == -1 || !dataAccess.GetDataList(index, materials))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> thicknesses = new List<double>();
            index = Params.IndexOfInputParam("_thicknesses_");
            if(index != -1)
            {
                dataAccess.GetDataList(index, thicknesses);
            }

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

                    double thickness = material.GetValue<double>(Core.MaterialParameter.DefaultThickness);
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
            {
                objects[i] = materials[i].Name;
            }

            for (int i = 0; i < thicknesses.Count; i++)
            {
                objects[materials.Count + i] = thicknesses[i];
            }

            List<ConstructionLayer> constructionLayers = Create.ConstructionLayers(objects);

            if (constructionLayers != null)
            {
                double thickness = constructionLayers.ConvertAll(x => x.Thickness).Sum();
                if (thickness > 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Thickness of construction layers exceed 1m");
                }
            }

            index = Params.IndexOfOutputParam("constructionLayers");
            if(index != -1)
            {
                dataAccess.SetDataList(index, constructionLayers?.ConvertAll(x => new GooConstructionLayer(x)));
            }

            index = Params.IndexOfOutputParam("materials");
            if (index != -1)
            {
                dataAccess.SetDataList(index, materials?.ConvertAll(x => new GooMaterial(x)));
            }

        }
    }
}