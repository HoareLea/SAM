using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalReplaceObject : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("492b0921-b271-42f8-93d7-e47c2e577c9d");

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
        public SAMAnalyticalReplaceObject()
          : base("SAMAnalytical.ReplaceObject", "SAMAnalytical.ReplaceObject",
              "Replace Object in AnalyticalModel",
              "SAM", "Analytical")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "_existingObjects", NickName = "_existingObjects", Description = "SAM Objects", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSAMObjectParam() { Name = "_newObjects", NickName = "_newObjects", Description = "SAM Objects", Access = GH_ParamAccess.list}, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooConstructionManagerParam() { Name = "constructionManager_", NickName = "constructionManager_", Description = "SAM Analytical Construction Manager", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));


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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if(index != -1 )
            {
                dataAccess.GetData(index, ref run);
            }

            if (!run)
            {
                return;
            }

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            analyticalModel = new AnalyticalModel(analyticalModel);

            List<IJSAMObject> sAMObject_Existing = new List<IJSAMObject>();
            index = Params.IndexOfInputParam("_existingObjects");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObject_Existing) || sAMObject_Existing == null || sAMObject_Existing.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IJSAMObject> sAMObject_New = new List<IJSAMObject>();
            index = Params.IndexOfInputParam("_newObjects");
            if (index == -1 || !dataAccess.GetDataList(index, sAMObject_New) || sAMObject_New == null || sAMObject_New.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(sAMObject_Existing.Count != sAMObject_New.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionManager constructionManager = null;
            index = Params.IndexOfInputParam("constructionManager_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref constructionManager);
            }

            if(constructionManager == null)
            {
                constructionManager = Analytical.Query.DefaultConstructionManager();
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);
            }

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            for(int i =0; i < sAMObject_Existing.Count; i++)
            {
                IJSAMObject jSAMObject_Existing = sAMObject_Existing[i];
                IJSAMObject jSAMObject_New = sAMObject_New[i];
                if(sAMObject_Existing == null || sAMObject_New == null)
                {
                    continue;
                }

                if(jSAMObject_Existing is ApertureConstruction)
                {
                    bool replaced = Analytical.Modify.Replace(adjacencyCluster, (ApertureConstruction)jSAMObject_Existing, jSAMObject_New as ApertureConstruction);
                    if(replaced)
                    {
                        ApertureConstruction apertureConstruction = (ApertureConstruction)jSAMObject_New;
                        List<IMaterial> materials = constructionManager.GetMaterials<IMaterial>(apertureConstruction);
                        if(materials != null)
                        {
                            foreach(IMaterial material in materials)
                            {
                                if(!materialLibrary.Contains(material))
                                {
                                    materialLibrary.Add(material);
                                }
                            }
                        }
                    }
                }
                else if (jSAMObject_Existing is Construction)
                {
                    bool replaced = Analytical.Modify.Replace(adjacencyCluster, (Construction)jSAMObject_Existing, jSAMObject_New as Construction);
                    if (replaced)
                    {
                        Construction construction = (Construction)jSAMObject_New;
                        List<IMaterial> materials = constructionManager.GetMaterials<IMaterial>(construction);
                        if (materials != null)
                        {
                            foreach (IMaterial material in materials)
                            {
                                if (!materialLibrary.Contains(material))
                                {
                                    materialLibrary.Add(material);
                                }
                            }
                        }
                    }
                }
                else if (jSAMObject_Existing is Space)
                {
                    bool replaced = Analytical.Modify.Replace(adjacencyCluster, (Space)jSAMObject_Existing, jSAMObject_New as Space);
                    if (replaced)
                    {

                    }
                }
                else if (jSAMObject_Existing is IMaterial)
                {
                    bool replaced = Analytical.Modify.Replace(adjacencyCluster, (IMaterial)jSAMObject_Existing, jSAMObject_New as IMaterial);
                    if (replaced)
                    {
                        Analytical.Modify.Replace(materialLibrary, (IMaterial)jSAMObject_Existing, jSAMObject_New as IMaterial);
                    }
                }
                else if (jSAMObject_Existing is Profile)
                {
                    bool replaced = Analytical.Modify.Replace(adjacencyCluster, (Profile)jSAMObject_Existing, jSAMObject_New as Profile);
                    if (replaced)
                    {
                        Analytical.Modify.Replace(profileLibrary, (Profile)jSAMObject_Existing, jSAMObject_New as Profile);
                    }
                }
            }

            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, profileLibrary);
            index = Params.IndexOfOutputParam("_analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }
        }
    }
}