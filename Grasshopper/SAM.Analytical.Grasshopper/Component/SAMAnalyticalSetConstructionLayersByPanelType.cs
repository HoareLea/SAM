using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetConstructionLayersByPanelType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("95372c36-3130-4c0c-9ccc-4f2254191cd7");

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
        public SAMAnalyticalSetConstructionLayersByPanelType()
          : base("SAMAnalytical.SetConstructionLayersByPanelType", "SAMAnalytical.SetConstructionLayersByPanelType",
              "Set SAM Analytical ConstructionLayers By PanelType",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooAnalyticalModelParam(), "_analyticalModel", "_analyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
            
            index = inputParamManager.AddParameter(new GooConstructionLibraryParam(), "_constructionLibrary_", "_constructionLibrary_", "SAM Analytical Contruction Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "_apertureConstructionLibrary_", "_apertureConstructionLibrary_", "SAM Analytical Aperture Contruction Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooMaterialLibraryParam(), "_materialLibrary_", "_materialLibrary_", "SAM Material Library", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddBooleanParameter("_emptyOnly_", "_emptyOnly_", "Only Null or Constructions without ConctructionLayers", GH_ParamAccess.item, true);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalModelParam(), "AnalyticalModel", "AnalyticalModel", "SAM Analytical Model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(0, ref analyticalModel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            dataAccess.GetData(1, ref constructionLibrary);

            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            dataAccess.GetData(2, ref apertureConstructionLibrary);

            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            MaterialLibrary materialLibrary = null;
            dataAccess.GetData(3, ref apertureConstructionLibrary);

            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            bool emptyOnly = true;
            dataAccess.GetData(4, ref emptyOnly);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            List<Panel> panels = adjacencyCluster?.GetPanels();

            if(panels == null)
            {
                dataAccess.SetData(0, new GooAnalyticalModel(analyticalModel));
                return;
            }

            MaterialLibrary materialLibrary_AnalyticalModel = analyticalModel.MaterialLibrary;

            for(int i=0; i < panels.Count; i++)//foreach(Panel panel in panels)
            {
                Panel panel = panels[i];
                if (panel == null)
                    continue;

                Construction construction_Old = panel.Construction;

                Construction construction_New = null;
                if (emptyOnly)
                {
                    if (construction_Old == null || !construction_Old.HasConstructionLayers())
                        construction_New = constructionLibrary.GetConstructions(panel.PanelType).FirstOrDefault();
                }
                else
                {
                    construction_New = constructionLibrary.GetConstructions(panel.PanelType).FirstOrDefault();
                }

                bool updated = false;

                if (construction_New != null)
                {
                    IEnumerable<IMaterial> materials_Temp = Analytical.Query.Materials(construction_New, materialLibrary);
                    if (materials_Temp != null)
                    {
                        foreach (IMaterial material in materials_Temp)
                            if (!materialLibrary_AnalyticalModel.Contains(material))
                                materialLibrary_AnalyticalModel.Add(material);
                    }

                    construction_New = new Construction(construction_Old, construction_New.ConstructionLayers);

                    panel = new Panel(panel, construction_New);
                    updated = true;
                }
                    
                if(panel.HasApertures)
                {
                    panel = new Panel(panel);
                    foreach (Aperture aperture in panel.Apertures)
                    {
                        Aperture aperture_Old = panel.GetAperture(aperture.Guid);
                        if (aperture_Old == null)
                            continue;

                        ApertureConstruction apertureConstruction_Old = aperture_Old.ApertureConstruction;

                        ApertureConstruction apertureConstruction_New = null;
                        if (emptyOnly)
                        {
                            if (apertureConstruction_Old == null || !apertureConstruction_Old.HasPaneConstructionLayers())
                                apertureConstruction_New = apertureConstructionLibrary.GetApertureConstructions(apertureConstruction_Old.ApertureType, panel.PanelType).FirstOrDefault();
                        }
                        else
                        {
                            apertureConstruction_New = apertureConstructionLibrary.GetApertureConstructions(apertureConstruction_Old.ApertureType, panel.PanelType).FirstOrDefault();
                        }

                        if (apertureConstruction_New != null)
                        {
                            IEnumerable<IMaterial> materials_Temp = Analytical.Query.Materials(apertureConstruction_New, materialLibrary);
                            if (materials_Temp != null)
                            {
                                foreach (IMaterial material in materials_Temp)
                                    if (!materialLibrary_AnalyticalModel.Contains(material))
                                        materialLibrary_AnalyticalModel.Add(material);
                            }

                            apertureConstruction_New = new ApertureConstruction(apertureConstruction_Old, apertureConstruction_New.PaneConstructionLayers, apertureConstruction_New.FrameConstructionLayers);

                            Aperture aperture_New = new Aperture(aperture_Old, apertureConstruction_New);

                            if (aperture_New == null)
                                continue;

                            panel.RemoveAperture(aperture_Old.Guid);
                            panel.AddAperture(aperture_New);
                            updated = true;
                        }
                    }
                }

                if (updated)
                    adjacencyCluster.AddObject(panel);

            }

            dataAccess.SetData(0, new GooAnalyticalModel(new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary_AnalyticalModel, analyticalModel.ProfileLibrary)));
        }
    }
}