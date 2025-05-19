using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionManagerByPanelAperture : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("adc6afa1-e5e6-4c1b-98eb-2218d271f9ca");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstructionManagerByPanelAperture()
          : base("SAMAnalytical.CreateConstructionManagerByPanelAperture", "SAMAnalyticalCreate.CreateConstructionManagerByPanelAperture",
              "Creates Construction Manager By Panel Or Aperture",
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

                GooAnalyticalModelParam gooAnalyticalModelParam = new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(gooAnalyticalModelParam, ParamVisibility.Binding));

                GooPanelParam gooPanelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooPanelParam, ParamVisibility.Binding));

                GooApertureParam gooApertureParam = new GooApertureParam() { Name = "_apertures", NickName = "_apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "constructions", NickName = "constructions", Description = "SAM Analytical Constructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "apertureConstructions", NickName = "apertureConstructions", Description = "SAM Analytical ApertureConstructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels");
            if (index != -1)
            {
                dataAccess.GetDataList(index, panels);
            }

            List<Aperture> apertures = new List<Aperture>();
            index = Params.IndexOfInputParam("_apertures");
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;

            ConstructionManager constructionManager = new ConstructionManager();
            foreach(Panel panel in panels)
            {
                Construction construction = panel?.Construction;
                if(construction == null)
                {
                    continue;
                }

                List<Construction> constructions = constructionManager.GetConstructions(construction.Name, TextComparisonType.Equals, true);
                if(constructions != null && constructions.Count > 0)
                {
                    continue;
                }

                constructionManager.Add(construction);

                IEnumerable<IMaterial> materials =  Analytical.Query.Materials(construction, materialLibrary);
                if(materials == null)
                {
                    continue;
                }

                foreach(IMaterial material in materials)
                {
                    if(constructionManager.GetMaterial(material.Name) == null)
                    {
                        constructionManager.Add(material);
                    }
                }

            }

            foreach(Aperture aperture in apertures)
            {
                ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                if (apertureConstruction == null)
                {
                    continue;
                }

                List<ApertureConstruction> apertureConstructions = constructionManager.GetApertureConstructions(apertureConstruction.Name, TextComparisonType.Equals, true);
                if (apertureConstructions != null && apertureConstructions.Count > 0)
                {
                    continue;
                }

                constructionManager.Add(apertureConstruction);

                IEnumerable<IMaterial> materials = Analytical.Query.Materials(apertureConstruction, materialLibrary);
                if (materials == null)
                {
                    continue;
                }

                foreach (IMaterial material in materials)
                {
                    if (constructionManager.GetMaterial(material.Name) == null)
                    {
                        constructionManager.Add(material);
                    }
                }
            }



            index = Params.IndexOfOutputParam("constructionManager");
            if(index != -1)
            {
                dataAccess.SetData(index, new GooConstructionManager(constructionManager));
            }

            index = Params.IndexOfOutputParam("constructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, constructionManager?.Constructions?.ConvertAll(x => new GooConstruction(x)));
            }

            index = Params.IndexOfOutputParam("apertureConstructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, constructionManager?.ApertureConstructions?.ConvertAll(x => new GooApertureConstruction(x)));
            }

        }
    }
}