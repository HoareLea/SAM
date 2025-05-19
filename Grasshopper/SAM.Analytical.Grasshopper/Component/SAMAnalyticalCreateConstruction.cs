using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("75ebc676-3401-481c-90e5-8f767b8b215b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstruction()
          : base("SAMAnalytical.CreateConstruction", "SAMAnalyticalCreate.Construction",
              "Create Construction, if nothing connect default values: _name = Basic Roof: SIM_EXT_SLD_Roof DA01 \n*The layers should be ordered from inside to outside",
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

                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction_", NickName = "construction_", Description = "Source SAM Analytical Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "Construction Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionLayerParam() { Name = "constructionLayers_", NickName = "constructionLayers_", Description = "SAM Contruction Layers", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "defaultPanelType_", NickName = "defaultPanelType_", Description = "Default PanelType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction", NickName = "construction", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            Construction construction = null;
            index = Params.IndexOfInputParam("construction_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref construction);
            }

            if (construction == null)
            {
                construction = Analytical.Query.DefaultConstruction(PanelType.Roof);
            }
            else
            {
                construction = new Construction(Guid.NewGuid(), construction, construction.Name);
            }

            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1 && dataAccess.GetData(index, ref name) && name != null)
            {
                construction = new Construction(construction, name);
            }

            index = Params.IndexOfInputParam("constructionLayers_");
            if (index != -1)
            {
                List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();
                if (dataAccess.GetDataList(index, constructionLayers))
                {
                    construction = new Construction(construction, constructionLayers);
                }
            }

            string panelType_String = null;
            index = Params.IndexOfInputParam("defaultPanelType_");
            if (index != -1 && dataAccess.GetData(index, ref panelType_String) && !string.IsNullOrWhiteSpace(panelType_String))
            {
                if (Core.Query.TryGetEnum(panelType_String, out PanelType panelType))
                {
                    construction.SetValue(ConstructionParameter.DefaultPanelType, panelType);
                }
            }

            index = Params.IndexOfOutputParam("construction");
            if(index != -1)
            {
                dataAccess.SetData(index, new GooConstruction(construction));
            }
        }
    }
}