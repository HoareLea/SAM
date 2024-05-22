using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
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
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

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
                Construction construction = Analytical.Query.DefaultConstruction(PanelType.Roof);

                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "Contruction Name", Access = GH_ParamAccess.item };
                param_String.SetPersistentData(construction.Name);
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                GooConstructionLayerParam gooConstructionLayerParam = new GooConstructionLayerParam() { Name = "_constructionLayers_", NickName = "_constructionLayers_", Description = "SAM Contruction Layers \n*The layers should be ordered from inside to outside", Access = GH_ParamAccess.list };
                gooConstructionLayerParam.PersistentData.AppendRange(construction.ConstructionLayers.ConvertAll(x => new GooConstructionLayer(x)));
                result.Add(new GH_SAMParam(gooConstructionLayerParam, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "defaultPanelType_", NickName = "defaultPanelType_", Description = "Default PanelType", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Voluntary));

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

            index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index == -1 || !dataAccess.GetData(index, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_constructionLayers_");
            List<ConstructionLayer> constructionLayers = new List<ConstructionLayer>();
            if (index == -1 || !dataAccess.GetDataList(index, constructionLayers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Construction construction = new Construction(name, constructionLayers);

            index = Params.IndexOfInputParam("defaultPanelType_");
            string text = null;
            if(index != -1 && dataAccess.GetData(index, ref text) && !string.IsNullOrWhiteSpace(text))
            {
                if (Core.Query.TryGetEnum(text, out PanelType panelType))
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