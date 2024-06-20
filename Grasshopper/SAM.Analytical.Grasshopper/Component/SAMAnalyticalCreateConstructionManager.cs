using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionManager : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5f5255d2-a9d4-4a3f-b3ba-762ff6c09825");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstructionManager()
          : base("SAMAnalytical.CreateConstructionManager", "SAMAnalytical.CreateConstructionManager",
              "Create ConstructionManager",
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

                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "constructionManager_", NickName = "constructionManager_", Description = "SAM Analytical ConstructionManager", Access = GH_ParamAccess.item, Optional = true }));

                GooAnalyticalObjectParam analyticalObjectParam;
                analyticalObjectParam = new GooAnalyticalObjectParam() { Name = "constructions_", NickName = "constructions_", Description = "SAM Analytical Constructions", Access = GH_ParamAccess.list, Optional = true };
                analyticalObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(analyticalObjectParam, ParamVisibility.Binding));

                analyticalObjectParam = new GooAnalyticalObjectParam() { Name = "apertureConstructions_", NickName = "apertureConstructions_", Description = "SAM Analytical ApertureConstructions", Access = GH_ParamAccess.list, Optional = true };
                analyticalObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(analyticalObjectParam, ParamVisibility.Binding));

                GooSAMObjectParam sAMObjectParam = new GooSAMObjectParam() { Name = "materials_", NickName = "materials_", Description = "SAM Materials", Optional = true, Access = GH_ParamAccess.list };
                sAMObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(sAMObjectParam, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "removeUnused_", NickName = "removeUnused_", Description = "Remove unused materials from Construction Manager", Access = GH_ParamAccess.item, Optional = true }));

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
                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "constructionManager", NickName = "constructionManager", Description = "SAM Analytical ConstructionManager", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ConstructionManager constructionManager = null;
            index = Params.IndexOfInputParam("constructions_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref constructionManager);
            }

            if(constructionManager == null)
            {
                constructionManager = new ConstructionManager();
            }
            else
            {
                constructionManager = new ConstructionManager(constructionManager);
            }

            List<IAnalyticalObject> analyticalObjects = null;

            index = Params.IndexOfInputParam("constructions_");
            analyticalObjects = new List<IAnalyticalObject>();
            if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
            {
                List<Construction> constructions = new List<Construction>();
                foreach (IAnalyticalObject analyticalObject in analyticalObjects)
                {
                    if (analyticalObject is ConstructionLibrary)
                    {
                        ((ConstructionLibrary)analyticalObject)?.GetConstructions()?.ForEach(x => constructions.Add(x));
                    }
                    else if (analyticalObject is Construction)
                    {
                        constructions.Add((Construction)analyticalObject);
                    }
                    else if (analyticalObject is ConstructionManager)
                    {
                        ((ConstructionManager)analyticalObject)?.Constructions?.ForEach(x => constructions.Add(x));
                    }
                }

                if(constructions != null && constructions.Count != 0)
                {
                    constructions?.ForEach(x => constructionManager.Add(x));
                }
            }

            index = Params.IndexOfInputParam("apertureConstructions_");
            analyticalObjects = new List<IAnalyticalObject>();
            if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
            {
                List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();
                foreach (IAnalyticalObject analyticalObject in analyticalObjects)
                {
                    if (analyticalObject is ApertureConstructionLibrary)
                    {
                        ((ApertureConstructionLibrary)analyticalObject)?.GetApertureConstructions()?.ForEach(x => apertureConstructions.Add(x));
                    }
                    else if (analyticalObject is ApertureConstruction)
                    {
                        apertureConstructions.Add((ApertureConstruction)analyticalObject);
                    }
                    else if (analyticalObject is ConstructionManager)
                    {
                        ((ConstructionManager)analyticalObject)?.ApertureConstructions?.ForEach(x => apertureConstructions.Add(x));
                    }
                }

                if (apertureConstructions != null && apertureConstructions.Count != 0)
                {
                    apertureConstructions?.ForEach(x => constructionManager.Add(x));
                }
            }

            index = Params.IndexOfInputParam("materials_");
            List<ISAMObject> sAMObjects = new List<ISAMObject>();
            if (index != -1 && dataAccess.GetDataList(index, sAMObjects) && sAMObjects != null)
            {
                List<IMaterial> materials = new List<IMaterial>();
                foreach (ISAMObject sAMObject in sAMObjects)
                {
                    if (sAMObject is MaterialLibrary)
                    {
                        ((MaterialLibrary)sAMObject)?.GetMaterials()?.ForEach(x => materials.Add(x));
                    }
                    else if (sAMObject is IMaterial)
                    {
                        materials.Add((IMaterial)sAMObject);
                    }
                    else if (sAMObject is ConstructionManager)
                    {
                        ((ConstructionManager)sAMObject)?.Materials?.ForEach(x => materials.Add(x));
                    }
                }

                materials?.ForEach(x => constructionManager.Add(x));
            }

            bool removeUnusedMaterials = false;
            index = Params.IndexOfInputParam("removeUnused_");
            if (index != -1  && dataAccess.GetData(index, ref removeUnusedMaterials))
            {
                if(removeUnusedMaterials)
                {
                    constructionManager.RemoveUnusedMaterials();
                }
            }

            List<string> names = Analytical.Query.MissingMaterialsNames(constructionManager);
            if(names != null && names.Count != 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("{0}: {1}", "ConstructionManager is missing following materials:" , string.Join(", ", names)));
            }

            index = Params.IndexOfOutputParam("constructionManager");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooConstructionManager(constructionManager));
            }
        }
    }
}