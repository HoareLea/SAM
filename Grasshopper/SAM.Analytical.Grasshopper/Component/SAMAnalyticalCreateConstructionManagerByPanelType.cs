using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionManagerByPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("eed6793b-0425-4bba-ae17-6ae86d01e0d0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstructionManagerByPanelType()
          : base("SAMAnalytical.CreateConstructionManagerByPanelType", "SAMAnalytical.CreateConstructionManagerByPanelType",
              "Create ConstructionManager By PanelType",
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

                GooConstructionManagerParam gooConstructionManagerParam = new GooConstructionManagerParam() { Name = "constructionManager_", NickName = "constructions_", Description = "SAM Analytical Constructions", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionManagerParam, ParamVisibility.Binding));

                GooConstructionParam gooConstructionParam;
                GooApertureConstructionParam gooApertureConstructionParam;

                gooConstructionParam = new GooConstructionParam() { Name = "ceiling_", NickName = "ceiling_", Description = "SAM Analytical Construction for ceiling", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "curtainWall_", NickName = "curtainWall_", Description = "SAM Analytical Construction for curtain wall", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "floorExposed_", NickName = "floorExposed_", Description = "SAM Analytical Construction for floor exposed", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "floorInternal_", NickName = "floorInternal_", Description = "SAM Analytical Construction for floor internal", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "roof_", NickName = "roof_", Description = "SAM Analytical Construction for roof", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "shade_", NickName = "shade_", Description = "SAM Analytical Construction for shade", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "slabOnGrade_", NickName = "slabOnGrade_", Description = "SAM Analytical Construction for slab on grade", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "solarPanel_", NickName = "solarPanel_", Description = "SAM Analytical Construction for solar panel", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "undergroundCeiling_", NickName = "undergroundCeiling_", Description = "SAM Analytical Construction for underground ceiling", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "undergroundSlab_", NickName = "undergroundSlab_", Description = "SAM Analytical Construction for underground slab", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "undegroundWall_", NickName = "undegroundWall_", Description = "SAM Analytical Construction for undeground wall", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "wallExternal_", NickName = "wallExternal_", Description = "SAM Analytical Construction for wall external", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooConstructionParam = new GooConstructionParam() { Name = "wallInternal_", NickName = "wallInternal_", Description = "SAM Analytical Construction for wall internal", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooApertureConstructionParam = new GooApertureConstructionParam() { Name = "doorExternal_", NickName = "doorExternal_", Description = "SAM Analytical Construction for door external", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooApertureConstructionParam, ParamVisibility.Binding));

                gooApertureConstructionParam = new GooApertureConstructionParam() { Name = "doorInternal_", NickName = "doorInternal_", Description = "SAM Analytical Construction for door internal", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooApertureConstructionParam = new GooApertureConstructionParam() { Name = "skylight_", NickName = "skylight_", Description = "SAM Analytical ApertureConstruction for skylight", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooApertureConstructionParam = new GooApertureConstructionParam() { Name = "windowExternal_", NickName = "windowExternal_", Description = "SAM Analytical ApertureConstruction for window external", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                gooApertureConstructionParam = new GooApertureConstructionParam() { Name = "windowInternal_", NickName = "windowInternal_", Description = "SAM Analytical ApertureConstruction for window internal", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooConstructionParam, ParamVisibility.Binding));

                GooSAMObjectParam sAMObjectParam = new GooSAMObjectParam() { Name = "materials_", NickName = "materials_", Description = "SAM Materials", Access = GH_ParamAccess.list, Optional = true};
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
            index = Params.IndexOfInputParam("constructionManager_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref constructionManager);
            }

            Dictionary<PanelType, Construction> constructionDictionary = new Dictionary<PanelType, Construction>();
            Dictionary<PanelType, ApertureConstruction> apertureConstructionDictionary = new Dictionary<PanelType, ApertureConstruction>();

            Construction construction = null;
            ApertureConstruction apertureConstruction = null;

            index = Params.IndexOfInputParam("ceiling_");
            if(index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.Ceiling] = construction;
            }

            index = Params.IndexOfInputParam("curtainWall_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.CurtainWall] = construction;
            }

            index = Params.IndexOfInputParam("floorExposed_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.FloorExposed] = construction;
            }

            index = Params.IndexOfInputParam("floorInternal_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.FloorInternal] = construction;
            }

            index = Params.IndexOfInputParam("roof_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.Roof] = construction;
            }

            index = Params.IndexOfInputParam("shade_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.Shade] = construction;
            }

            index = Params.IndexOfInputParam("slabOnGrade_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.SlabOnGrade] = construction;
            }

            index = Params.IndexOfInputParam("solarPanel_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.SolarPanel] = construction;
            }

            index = Params.IndexOfInputParam("undergroundCeiling_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.UndergroundCeiling] = construction;
            }

            index = Params.IndexOfInputParam("undergroundSlab_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.UndergroundSlab] = construction;
            }

            index = Params.IndexOfInputParam("undegroundWall_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.UndergroundWall] = construction;
            }

            index = Params.IndexOfInputParam("wallExternal_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.WallExternal] = construction;
            }

            index = Params.IndexOfInputParam("wallInternal_");
            if (index != -1 && dataAccess.GetData(index, ref construction) && construction != null)
            {
                constructionDictionary[PanelType.WallInternal] = construction;
            }

            index = Params.IndexOfInputParam("doorExternal_");
            if (index != -1 && dataAccess.GetData(index, ref apertureConstruction) && apertureConstruction != null)
            {
                apertureConstruction = new ApertureConstruction(apertureConstruction, ApertureType.Door);

                apertureConstructionDictionary[PanelType.WallExternal] = apertureConstruction;
            }

            index = Params.IndexOfInputParam("doorInternal_");
            if (index != -1 && dataAccess.GetData(index, ref apertureConstruction) && apertureConstruction != null)
            {
                apertureConstruction = new ApertureConstruction(apertureConstruction, ApertureType.Door);

                apertureConstructionDictionary[PanelType.WallInternal] = apertureConstruction;
            }

            index = Params.IndexOfInputParam("skylight_");
            if (index != -1 && dataAccess.GetData(index, ref apertureConstruction) && apertureConstruction != null)
            {
                apertureConstruction = new ApertureConstruction(apertureConstruction, ApertureType.Window);

                apertureConstructionDictionary[PanelType.Roof] = apertureConstruction;
            }

            index = Params.IndexOfInputParam("windowExternal_");
            if (index != -1 && dataAccess.GetData(index, ref apertureConstruction) && apertureConstruction != null)
            {
                apertureConstruction = new ApertureConstruction(apertureConstruction, ApertureType.Window);

                apertureConstructionDictionary[PanelType.WallExternal] = apertureConstruction;
            }

            index = Params.IndexOfInputParam("windowInternal_");
            if (index != -1 && dataAccess.GetData(index, ref apertureConstruction) && apertureConstruction != null)
            {
                apertureConstruction = new ApertureConstruction(apertureConstruction, ApertureType.Window);
                
                apertureConstructionDictionary[PanelType.WallInternal] = apertureConstruction;
            }

            index = Params.IndexOfInputParam("materials_");
            List<ISAMObject> sAMObjects = new List<ISAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

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

            constructionManager = Create.ConstructionManager(constructionManager, constructionDictionary, apertureConstructionDictionary, materials);

            index = Params.IndexOfOutputParam("constructionManager");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooConstructionManager(constructionManager));
            }
        }
    }
}