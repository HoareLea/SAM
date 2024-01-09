using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateConstructionsByPanelType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("aeb4f3d7-6f35-4107-9e31-3edc53aa6d47");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;


        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstructionsByPanelType()
          : base("SAMAnalytical.UpdateConstructionsByPanelType",
                "SAMAnalytical.UpdateConstructionsByPanelType",
                "Update Constructions By PanelType",
                "SAM WIP",
                "Tas")
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "_constructionManager", NickName = "_constructionManager", Description = "SAM Construction Manager", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean @boolean = null;
                @boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Connect a boolean toggle to run.", Access = GH_ParamAccess.item };
                @boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(@boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index_successful = Params.IndexOfOutputParam("successful");
            if (index_successful != -1)
            {
                dataAccess.SetData(index_successful, false);
            }

            int index;

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
                run = false;

            if (!run)
                return;

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionManager constructionManager = null;
            index = Params.IndexOfInputParam("_constructionManager");
            if (index == -1 || !dataAccess.GetData(index, ref constructionManager) || constructionManager == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels != null && panels.Count != 0)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

                for (int i = 0; i < panels.Count; i++)
                {
                    Panel panel = panels[i];
                    if(panel == null)
                    {
                        continue;
                    }

                    panel = Create.Panel(panel);

                    Construction construction = constructionManager.GetConstructions(panel.PanelType)?.FirstOrDefault();
                    if(construction != null)
                    {
                        panel = Create.Panel(panel, construction);
                    }

                    List<Aperture> apertures = panel.Apertures;
                    if(apertures != null && apertures.Count != 0)
                    {
                        for (int j = 0; j < apertures.Count; j++)
                        {
                            Aperture aperture = apertures[j];
                            if(aperture == null)
                            {
                                continue;
                            }

                            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                            if(apertureConstruction == null)
                            {
                                continue;
                            }

                            ApertureConstruction apertureConstruction_New = constructionManager.GetApertureConstructions(apertureConstruction.ApertureType, panel.PanelType)?.FirstOrDefault();
                            if(apertureConstruction_New == null)
                            {
                                continue;
                            }

                            apertures[j] = new Aperture(aperture, apertureConstruction_New);
                        }

                        panel.RemoveApertures();
                        apertures.ForEach(x => panel.AddAperture(x));
                    }

                    adjacencyCluster.AddObject(panel);
                }
            }

            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            if (index_successful != -1)
            {
                dataAccess.SetData(index_successful, analyticalModel != null);
            }
        }
    }
}