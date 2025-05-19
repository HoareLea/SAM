using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateConstructionsByConstructionManagerAndPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b5a4f281-9157-4d7f-98e6-635bfc3cd8ad");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);


        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstructionsByConstructionManagerAndPanels()
          : base("SAMAnalytical.UpdateConstructionsByConstructionManagerAndPanels", 
                "SAMAnalytical.UpdateConstructionsByConstructionManagerAndPanels", 
                "Update Constructions in Analytical Model By ConstructionManager and Panels", 
                "SAM",
                "Analytical04")
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panel", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "_constructions", NickName = "_constructions", Description = "SAM Analytical Construction", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            global::Grasshopper.Kernel.Data.GH_Structure<GooPanel> structure_GooPanels = null;
            index = Params.IndexOfInputParam("_panels");
            if (index == -1 || !dataAccess.GetDataTree(index, out structure_GooPanels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Construction> constructions = new List<Construction>();
            index = Params.IndexOfInputParam("_constructions");
            if (index == -1 || !dataAccess.GetDataList(index, constructions) || constructions == null || constructions.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if (adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

                List<Panel> panels = adjacencyCluster.GetPanels();
                if (panels != null)
                {
                    analyticalModel = new AnalyticalModel(analyticalModel);

                    for (int i = 0; i < structure_GooPanels.PathCount; i++)
                    {
                        Construction construction = i >= constructions.Count ? constructions.Last() : constructions[i];
                        if (construction == null)
                        {
                            continue;
                        }

                        List<GooPanel> gooPanels = structure_GooPanels[i];
                        if (gooPanels == null || gooPanels.Count == 0)
                        {
                            continue;
                        }

                        Construction construction_Temp = constructionManager.Constructions.Find(x => x.Guid == construction.Guid);
                        if (construction_Temp == null)
                        {
                            construction_Temp = constructionManager.Constructions.Find(x => x.Name == construction.Name);
                        }

                        if (construction_Temp == null)
                        {
                            continue;
                        }

                        foreach (GooPanel gooPanel in gooPanels)
                        {
                            IPanel panel = gooPanel?.Value;
                            if(panel == null)
                            {
                                continue;
                            }

                            panel = panels.Find(x => x.Guid == panel.Guid);
                            if(panel == null)
                            {
                                continue;
                            }

                            if(panel is Panel)
                            {
                                panel = Create.Panel((Panel)panel, construction_Temp);
                            }
                            else if(panel is ExternalPanel)
                            {
                                panel = new ExternalPanel(panel.Face3D, construction_Temp);
                            }

                            adjacencyCluster.AddObject(panel);
                        }

                        List<IMaterial> materials = constructionManager.GetMaterials<IMaterial>(construction_Temp);
                        if(materials != null && materials.Count != 0)
                        {
                            materials.ForEach(x => analyticalModel.AddMaterial(x));
                        }

                    }
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

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