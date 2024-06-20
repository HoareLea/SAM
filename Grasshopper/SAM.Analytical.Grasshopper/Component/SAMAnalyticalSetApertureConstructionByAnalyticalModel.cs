using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetApertureConstructionByAnalyticalModel : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("63fd1e81-4ada-4539-920e-ff2ca49c0991");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetApertureConstructionByAnalyticalModel()
          : base("SAMAnalytical.SetApertureConstructionByAnalyticalModel", "SAMAnalytical.SetApertureConstructionByAnalyticalModel",
              "Set Aperture Construction By AnalyticalModel \n*The layers should be ordered from inside to outside",
              "SAM WIP", "Analytical")
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "_apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "_apertureConstruction", NickName = "apertureConstruction", Description = "SAM Analytical ApertureConstruction \n*The layers should be ordered from inside to outside" }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "constructionManager_", NickName = "constructionManager_", Description = "SAM Analytical ConstructionManager", Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertures");
            List<Aperture> apertures = new List<Aperture>();
            if (index == -1 || !dataAccess.GetDataList(index, apertures) || apertures == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertureConstruction");
            ApertureConstruction apertureConstruction = null;
            if (index == -1 || !dataAccess.GetData(index, ref apertureConstruction) || apertureConstruction == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("constructionManager_");
            ConstructionManager constructionManager = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref constructionManager);
            }

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            adjacencyCluster = adjacencyCluster == null ? null : new AdjacencyCluster(adjacencyCluster, true);

            apertureConstruction = new ApertureConstruction(apertureConstruction);

            for (int i = 0; i < apertures.Count; i++)
            {
                apertures[i] = new Aperture(apertures[i], apertureConstruction);
            }

            apertures = adjacencyCluster.UpdateApertures(apertures);

            if(apertures != null && apertures.Count != 0)
            {
                List<string> materialNames_Missing = new List<string>();

                MaterialLibrary materialLibrary_Default = Analytical.Query.DefaultMaterialLibrary();

                MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
                List<string> materialNames = materialLibrary?.MissingMaterialsNames(apertureConstruction);
                if(materialNames != null && materialNames.Count != 0)
                {
                    foreach(string materialName in materialNames)
                    {
                        IMaterial material = constructionManager?.GetMaterial(materialName);
                        if(material == null)
                        {
                            material = materialLibrary_Default?.GetMaterial(materialName);
                        }

                        if(material == null)
                        {
                            materialNames_Missing.Add(materialName);
                            continue;
                        }

                        materialLibrary.Add(material);
                    }
                }

                if (materialNames_Missing != null && materialNames_Missing.Count != 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("{0}: {1}", "ConstructionManager is missing following materials:", string.Join(", ", materialNames_Missing)));
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
                dataAccess.SetDataList(index, apertures?.ConvertAll(x => new GooAperture(x)));
        }
    }
}