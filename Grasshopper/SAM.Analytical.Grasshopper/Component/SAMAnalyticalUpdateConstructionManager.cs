using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateConstructionManager : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1143b387-a269-4429-beef-94a58cfb459f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstructionManager()
          : base("SAMAnalytical.UpdateConstructionManager", "SAMAnalytical.UpdateConstructionManager",
              "Update ConstructionManager",
              "SAM", "Analytical04")
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

                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "_constructionManager", NickName = "_constructionManager", Description = "SAM Analytical ConstructionManager", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                GooAnalyticalObjectParam analyticalObjectParam;

                analyticalObjectParam = new GooAnalyticalObjectParam() { Name = "constructions_", NickName = "constructions_", Description = "SAM Analytical Constructions", Access = GH_ParamAccess.list, Optional = true };
                analyticalObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(analyticalObjectParam, ParamVisibility.Binding));

                analyticalObjectParam = new GooAnalyticalObjectParam() { Name = "apertureConstructions_", NickName = "apertureConstructions_", Description = "SAM Analytical ApertureConstructions", Access = GH_ParamAccess.list, Optional = true };
                analyticalObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(analyticalObjectParam, ParamVisibility.Binding));

                GooSAMObjectParam sAMObjectParam = new GooSAMObjectParam() { Name = "materials_", NickName = "materials_", Description = "SAM Materials", Access = GH_ParamAccess.list, Optional = true };
                sAMObjectParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(sAMObjectParam, ParamVisibility.Binding));

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
            index = Params.IndexOfInputParam("_constructionManager");
            if(index == -1 || !dataAccess.GetData(index, ref constructionManager) || constructionManager == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            constructionManager = new ConstructionManager(constructionManager);

            List<IAnalyticalObject> analyticalObjects = null;

            List<Construction> constructions = null;

            index = Params.IndexOfInputParam("_constructions");
            analyticalObjects = new List<IAnalyticalObject>();
            if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
            {
                constructions = new List<Construction>();
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
            }

            List<ApertureConstruction> apertureConstructions = null;

            index = Params.IndexOfInputParam("_apertureConstructions");
            analyticalObjects = new List<IAnalyticalObject>();
            if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
            {
                apertureConstructions = new List<ApertureConstruction>();
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
            }

            List<IMaterial> materials = null;

            index = Params.IndexOfInputParam("_materials");
            List<ISAMObject> sAMObjects = new List<ISAMObject>();
            if (index != -1 && dataAccess.GetDataList(index, sAMObjects) && sAMObjects != null)
            {
                materials = new List<IMaterial>();
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
            }

            materials?.ForEach(x => constructionManager?.Add(x));
            constructions?.ForEach(x => constructionManager?.Add(x));
            apertureConstructions?.ForEach(x => constructionManager?.Add(x));

            index = Params.IndexOfOutputParam("constructionManager");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooConstructionManager(constructionManager));
            }
        }
    }
}