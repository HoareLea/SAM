using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5442e313-bf9f-4292-a740-232f01c182a3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetConstruction()
          : base("SAMAnalytical.SetConstruction", "SAMAnalytical.SetConstruction",
              "Set Construction",
              "SAM", "Analytical03")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "_construction", NickName = "_construction", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "_materials_", NickName = "_materials_", Description = "SAM Materials", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooMaterialParam() { Name = "Materials", NickName = "Materials", Description = "SAM Materials Added with Construction", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels");
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Construction construction = null;
            index = Params.IndexOfInputParam("_construction");
            if (index == -1 || !dataAccess.GetData(index, ref construction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = sAMObject as AdjacencyCluster;
            AnalyticalModel analyticalModel = sAMObject as AnalyticalModel;
            if (adjacencyCluster == null)
                adjacencyCluster = analyticalModel?.AdjacencyCluster;

            List<IMaterial> materials = null;

            if (analyticalModel != null)
            {
                MaterialLibrary materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);
                if (materialLibrary == null)
                    materialLibrary = new MaterialLibrary("Temp");
                else
                    materialLibrary = new MaterialLibrary(materialLibrary);

                materials = new List<IMaterial>();
                index = Params.IndexOfInputParam("_materials_");
                if (index != -1)
                    dataAccess.GetDataList(index, materials);

                if (materials != null && materials.Count > 0 && materialLibrary != null)
                    materials.ForEach(x => materialLibrary.Add(x));

                materials = Analytical.Query.Materials(construction, materialLibrary)?.ToList();
                if (materials != null && materials.Count != 0)
                {
                    analyticalModel = new AnalyticalModel(analyticalModel);
                    materials.ForEach(x => analyticalModel.AddMaterial(x));
                }
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster_Result = new AdjacencyCluster(adjacencyCluster);
            List<Panel> panels_Result = new List<Panel>();

            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Panel panel_AdjacencyCluster = adjacencyCluster_Result.GetObject<Panel>(panel.Guid);
                if (panel_AdjacencyCluster == null)
                    continue;

                panel_AdjacencyCluster = Create.Panel(panel_AdjacencyCluster, construction);
                if (panel_AdjacencyCluster == null)
                    continue;

                if (adjacencyCluster_Result.AddObject(panel_AdjacencyCluster))
                    panels_Result.Add(panel_AdjacencyCluster);
            }

            index = Params.IndexOfOutputParam("Analytical");
            if(index != -1)
            {
                if (analyticalModel != null)
                    dataAccess.SetData(index, new AnalyticalModel(analyticalModel, adjacencyCluster_Result));
                else if (adjacencyCluster != null)
                    dataAccess.SetData(index, adjacencyCluster_Result);
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Result?.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("Materials");
            if (index != -1)
                dataAccess.SetDataList(index, materials?.ConvertAll(x => new GooMaterial(x)));
        }
    }
}